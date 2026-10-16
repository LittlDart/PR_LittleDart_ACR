using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using PromeRotation.Data;

namespace LittleDart.Common.Mitigation.Calc;

/// <summary>
/// 威胁检测的**两份判据**（2026-10-14 玩家需求）。
///
/// <para><b>为什么分两份</b>：读条型威胁（死刑、AOE 读条）**只对 boss 有意义** ——
/// 小怪不会放死刑；反过来，群怪堆里真正该驱动减伤的是「几只怪在打你 + 掉血多快」。
/// 所以用「在不在 boss 房」选判据。</para>
///
/// <para><b>分档来源</b>：<c>MitigationJobProfile.InBossRoom</c>（绝枪填 <c>GameData.IsInBossRoom</c>）。
/// 该 API 带 <c>[Obsolete(DiagnosticId="PR_EXPERIMENTAL")]</c>，PR 自认「可能出现误判」，
/// 所以退化路径是安全的：读不到 ⇒ 按群怪判据（不会因为没有 boss 房标记就完全不交减伤）。</para>
///
/// <para><b>相对旧实现的行为变化（重要）</b>：旧的 <see cref="DeathSentenceDetector"/> 与
/// <see cref="EnemyAoeDetector"/> **只看当前目标**（<c>profile.CurrentTarget()</c>）——
/// 没选中正在读条的那个怪，减伤就不交。本类两个判据都改成**扫描指定半径内的全部敌人**，
/// 这是严格更优的，也是玩家要的「检测到在 boss 房启用死刑和 aoe 的检测」。</para>
/// </summary>
public sealed class ThreatContext
{
	/// <summary>群怪判据：附近多少米内数敌人。</summary>
	public const float PackRadius = 15f;

	/// <summary>读条判据：扫多大范围内的敌人读条。比 <see cref="PackRadius"/> 大，
	/// 因为 boss 可能要跑位拉开距离才读条。</summary>
	public const float CastScanRadius = 30f;

	/// <summary>群怪判据：敌人数 ≥ 这个值才算「群怪堆」，才走「按人数出个人减伤/保护资源」那一路。</summary>
	public const int PackThreshold = 3;

	/// <summary>
	/// AOE 团队减伤锁的**保险丝**（毫秒）。
	///
	/// <para>正常情况下锁由「这一次 AOE 读条结束」释放（见 <see cref="ConsumeTeamMitigationLock"/>）。
	/// 但如果同一个 <c>(来源, 技能)</c> 被反复读到、或者来源突然消失导致状态卡住，
	/// 这个保险丝保证锁一定会松 —— 否则减伤会被**永久**锁死，那比多交一个严重得多。</para>
	/// </summary>
	public const int AoeMitigationLockFuseMs = 8000;

	private readonly MitigationJobProfile _p;

	/// <summary>
	/// 已经为哪一次 AOE 交过团队减伤了：<c>(来源 EntityId, 技能 Id)</c>。<c>null</c> = 没锁。
	/// 用「来源 + 技能」而不是时间窗 —— 同一个 boss 在同一场里放两次同名 AOE，
	/// 中间只要读条停过一次（下面 <see cref="TickAoeLock"/> 会清掉），第二次就能重新交。
	/// </summary>
	private (ulong SourceId, uint ActionId)? _teamMitigationLock;

	/// <summary>上锁时刻（TickCount64），配合 <see cref="AoeMitigationLockFuseMs"/> 用。</summary>
	private long _teamMitigationLockAtMs;

	public ThreatContext(MitigationJobProfile profile)
	{
		_p = profile;
	}

	/// <summary>
	/// 现在用哪一份判据。<c>false</c> / 读不到 ⇒ <see cref="Criteria.Pack"/>（安全的退化方向）。
	/// </summary>
	public Criteria CurrentCriteria
	{
		get
		{
			bool? inBossRoom = ReadInBossRoom();
			if (!inBossRoom.HasValue)
			{
				return Criteria.Pack;
			}
			return inBossRoom.Value ? Criteria.BossRoom : Criteria.Pack;
		}
	}

	public enum Criteria
	{
		/// <summary>不在 boss 房 ⇒ 群怪判据（按人数与掉血压力）。</summary>
		Pack = 0,

		/// <summary>在 boss 房 ⇒ 读条判据（死刑 / AOE 读条）。</summary>
		BossRoom = 1,
	}

	public static string Describe(Criteria criteria)
	{
		return criteria == Criteria.BossRoom
			? "boss 房·读条判据"
			: "非 boss 房·群怪判据";
	}

	// ========================================================================
	//  判据一：boss 房 —— 读条
	// ========================================================================

	/// <summary>
	/// 范围内是否有敌人在读**死刑类**技能（<c>PromeBattleData.TankDeathActions</c>）。
	/// <b>扫全部敌人</b>，不要求你正选中它。
	/// </summary>
	public bool IsCastingDeathSentence()
	{
		return AnyEnemyCastingAction(PromeBattleData.TankDeathActions, remainingMs: 0);
	}

	/// <summary>
	/// 范围内是否有敌人在读**AOE**技能（<c>PromeBattleData.AoeActions</c>）。
	/// <paramref name="warnMs"/> 为「还剩多少毫秒以内才算要来了」；<c>&lt;= 0</c> = 只要在读就算。
	/// </summary>
	public bool IsCastingAoe(int warnMs)
	{
		return FindIncomingAoe(warnMs).HasValue;
	}

	// ========================================================================
	//  AOE 团队减伤锁：每次 AOE 读条只准交**一个**团队减伤
	// ========================================================================

	/// <summary>
	/// 现在能不能为这次 AOE 交团队减伤。
	///
	/// <para><b>要解决的问题</b>：`IsSkillActive` 只挡「同一个技能 1500ms 内不重交」，
	/// 于是**不同**的团队减伤（光之心 / 血仇 / 亲疏自行…）会各自为**同一次** AOE 读条交一遍 ——
	/// 一次 AOE 吃掉好几个大招。玩家口径：每个 AOE 只用一个群体减伤。</para>
	///
	/// <para><b>判据</b>：锁看的是 <c>(来源 EntityId, 技能 Id)</c>。只要这次读条还在进行，
	/// 就一直锁着 —— 期间不管是哪个团队减伤都不给交。等这次读条消失（放完了 / 被打断了 / 目标没了），
	/// 锁自动释放，下一次 AOE 可以再交一个。</para>
	///
	/// <para><b>为什么不用固定时间窗</b>：读条长度因技能而异（2.5s~5s+）。
	/// 时间窗短了挡不住，长了会误挡紧随其后的第二个 AOE。
	/// 用「同一次读条」当边界是精确的；<see cref="AoeMitigationLockFuseMs"/> 只当保险丝。</para>
	/// </summary>
	public bool CanUseTeamMitigation()
	{
		return !_teamMitigationLock.HasValue;
	}

	/// <summary>
	/// 每帧调一次：这次 AOE 读条还在 ⇒ 继续锁；读条没了 / 超保险丝 ⇒ 释放。
	/// 由 <see cref="MitigationEngine.Tick"/> 驱动（`Evaluate` 有 80ms 缓存，不适合做状态推进）。
	/// </summary>
	public void TickAoeLock(int warnMs)
	{
		if (!_teamMitigationLock.HasValue)
		{
			return;
		}
		// 保险丝：来源消失 + 状态卡住时一定要松锁，否则减伤会被永久锁死
		if (Environment.TickCount64 - _teamMitigationLockAtMs >= AoeMitigationLockFuseMs)
		{
			ReleaseTeamMitigationLock("保险丝到点");
			return;
		}
		(ulong s, uint a)? incoming = FindIncomingAoe(warnMs);
		if (!incoming.HasValue || incoming.Value.s != _teamMitigationLock.Value.SourceId
			|| incoming.Value.a != _teamMitigationLock.Value.ActionId)
		{
			ReleaseTeamMitigationLock("这次 AOE 读条已结束");
		}
	}

	/// <summary>
	/// 交出一个团队减伤 ⇒ 上锁。由 <see cref="MitigationEngine.NotifyCast"/> 调用。
	/// 只对**团队**类减伤上锁（个人减伤不受此锁约束）。
	/// </summary>
	public void OnTeamMitigationCast(uint skillId)
	{
		(ulong s, uint a)? incoming = FindIncomingAoe(_p.Thresholds.AoeWarnMs);
		if (!incoming.HasValue)
		{
			// 读不到 AOE 来源（例如刚消失）⇒ 用 (0, skillId) 兜底，让锁至少挡住「同一个技能重交」以外的兄弟技能
			_teamMitigationLock = (0UL, skillId);
		}
		else
		{
			_teamMitigationLock = (incoming.Value.s, incoming.Value.a);
		}
		_teamMitigationLockAtMs = Environment.TickCount64;
	}

	public void ResetAoeLock()
	{
		_teamMitigationLock = null;
		_teamMitigationLockAtMs = 0L;
	}

	public string DescribeAoeLock()
	{
		if (!_teamMitigationLock.HasValue)
		{
			return "未上锁";
		}
		long ageMs = Environment.TickCount64 - _teamMitigationLockAtMs;
		return $"已上锁（来源={_teamMitigationLock.Value.SourceId} 技能={_teamMitigationLock.Value.ActionId} 已 {ageMs}ms）";
	}

	private void ReleaseTeamMitigationLock(string reason)
	{
		_teamMitigationLock = null;
		_teamMitigationLockAtMs = 0L;
		if (_p.DiagnosticsEnabled())
		{
			_p.Log($"{_p.LogPrefix}/AOE锁 释放：{reason}");
		}
	}

	/// <summary>
	/// 找到「最近要来的那一次 AOE 读条」，返回 <c>(来源 EntityId, 技能 Id)</c>。
	/// 多条同时在读时取**剩余时间最短**的那个 —— 那是马上要吃的那个。
	/// </summary>
	private (ulong SourceId, uint ActionId)? FindIncomingAoe(int warnMs)
	{
		IReadOnlySet<uint> actionIds = PromeBattleData.AoeActions;
		if (actionIds == null || actionIds.Count == 0)
		{
			return null;
		}
		IBattleChara? me = _p.Me();
		if (me == null)
		{
			return null;
		}
		(ulong SourceId, uint ActionId)? best = null;
		float bestRemainMs = float.MaxValue;
		try
		{
			foreach (IGameObject obj in Svc.Objects)
			{
				if (obj is not IBattleChara { IsDead: false, IsTargetable: true } enemy)
				{
					continue;
				}
				if (obj.ObjectKind != ObjectKind.BattleNpc || !_p.IsEnemy(enemy))
				{
					continue;
				}
				if (_p.DistanceTo(enemy) > CastScanRadius)
				{
					continue;
				}
				if (!enemy.IsCasting || enemy.CastActionId == 0 || !actionIds.Contains(enemy.CastActionId))
				{
					continue;
				}
				float remainMs = Math.Max(0f, enemy.TotalCastTime - enemy.CurrentCastTime) * 1000f;
				if (warnMs > 0 && remainMs > warnMs)
				{
					continue;
				}
				if (remainMs < bestRemainMs)
				{
					bestRemainMs = remainMs;
					best = (obj.EntityId, enemy.CastActionId);
				}
			}
		}
		catch
		{
			return null;
		}
		return best;
	}

	// ========================================================================
	//  判据二：非 boss 房 —— 群怪
	// ========================================================================

	/// <summary>群怪判据：交战半径内的敌人数（≥ <see cref="PackThreshold"/> 才算堆）。</summary>
	public int PackEnemyCount()
	{
		return _p.NearbyEnemyCount(PackRadius);
	}

	public bool IsPack()
	{
		return PackEnemyCount() >= PackThreshold;
	}

	/// <summary>
	/// 群怪判据：堆里有没有敌人在读 AOE。**仍然扫读条** —— 只是不把它当唯一闸门，
	/// 群怪的主闸门是人数（见 <see cref="IsPack"/>）。
	/// </summary>
	public bool IsPackCastingAoe(int warnMs)
	{
		return AnyEnemyCastingAction(PromeBattleData.AoeActions, warnMs);
	}

	// ========================================================================
	//  共用扫描
	// ========================================================================

	/// <summary>
	/// 扫 <see cref="CastScanRadius"/> 内的敌人，看有没有在读 <paramref name="actionIds"/> 里的技能。
	/// 跳过玩家 / 宠物 / 已死 / 不可选中。
	/// </summary>
	private bool AnyEnemyCastingAction(IReadOnlySet<uint> actionIds, int remainingMs)
	{
		if (actionIds == null || actionIds.Count == 0)
		{
			return false;
		}
		IBattleChara? me = _p.Me();
		if (me == null)
		{
			return false;
		}
		try
		{
			foreach (IGameObject obj in Svc.Objects)
			{
				if (obj is not IBattleChara { IsDead: false, IsTargetable: true } enemy)
				{
					continue;
				}
				// 排除玩家 / 宠物 / 召唤物：它们的 ObjectKind 不是 BattleNpc
				if (obj.ObjectKind != ObjectKind.BattleNpc)
				{
					continue;
				}
				if (!_p.IsEnemy(enemy))
				{
					continue;
				}
				if (_p.DistanceTo(enemy) > CastScanRadius)
				{
					continue;
				}
				if (!enemy.IsCasting || enemy.CastActionId == 0)
				{
					continue;
				}
				if (!actionIds.Contains(enemy.CastActionId))
				{
					continue;
				}
				if (remainingMs > 0)
				{
					float remainMs = Math.Max(0f, enemy.TotalCastTime - enemy.CurrentCastTime) * 1000f;
					if (remainMs > remainingMs)
					{
						continue;
					}
				}
				return true;
			}
		}
		catch
		{
			return false;
		}
		return false;
	}

	private bool? ReadInBossRoom()
	{
		try
		{
			return _p.InBossRoom();
		}
		catch
		{
			return null;
		}
	}
}
