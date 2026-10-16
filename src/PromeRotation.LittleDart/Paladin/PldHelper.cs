using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.Logging;
using LittleDart.Common;
using LittleDart.Paladin.GCD;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using static PromeRotation.Helpers.JobGaugeHelper;
using PromeRotation.Managers;

namespace LittleDart.Paladin;

public class PldHelper
{
	public enum SupportTargetKind
	{
		Partner,
		Healer,
		LowestHp
	}

	private const float MeleeRangeFallback = 3f;

	private const float CastableAllyRange = 30f;

	private const int AoeEnemyCountRange = 5;

	public const int GcdReentryGuardMs = 1500;

	public const int FofRecentCastWindowMs = 1500;

	public const int ToastStyleInfo = 1;

	public const int ToastStyleWarn = 2;

	public static bool HasTankStance()
	{
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return BattleCharaExtensions.HasStatus(me, 79u);
		}
		return false;
	}

	public static IBattleChara? GetCurrentTarget()
	{
		return Core.Target;
	}

	public static double GetAuraTimeLeft(uint auraId)
	{
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return BattleCharaExtensions.GetStatusLeftTime(me, auraId);
		}
		return 0.0;
	}

	public static double GetComboTimeLeftSeconds()
	{
		return ActionHelper.GetComboLeftTime();
	}

	/// <summary>
	/// 连击窗口是否**还在走** —— 读游戏自己的组合计时（<c>ActionManager.Combo.Timer</c>，与
	/// <see cref="GetLastComboSpellId"/> 同一个 <c>ComboDetail</c>）。
	///
	/// <para>★ 2026-10-16 与绝枪侧对齐：绝枪 GCD 路径**从来没信过「id 还在 ⇒ 连击还在」**——
	/// <c>GnbHelper.IsBrutalShellComboAlive()</c> 判的就是 <c>GetLastComboSpellId()==16139 &amp;&amp; GetComboTimeLeftSeconds()&gt;0</c>，
	/// <c>GnbBaseComboGcd</c> 也把 <c>comboActive = GetComboTimeLeftSeconds() &gt; 0</c> 单独拿出来判。
	/// 骑士侧原来只判 id，缺的那一半只能用 <c>IsRecentlyCast()</c>（本地记账）去补；
	/// 而本地记账在「那一发被游戏拒收」时会记成假事实 —— 日志里连挑同一发 5~6 次 ×400ms 的那一族，
	/// 根因之一就是这里（详见 GCD 延迟那轮排查）。本方法把这一半交回游戏回答。</para>
	///
	/// <para>⚠️ <b>纯收紧</b>：只在「id 还挂着、但计时已经走完」时把判定翻成 false
	///（计时还在走的一切情形行为逐字不变）。因此改了正向判据就必须**同时**改负向写法
	///（<c>FastBladeGcd</c>/<c>FofFastBladeGcd</c> 的「连击进行中，让位」、<c>RiotBladeGcd</c>/<c>FofRiotBladeGcd</c> 的
	/// 「上一段不是先锋剑」、<c>ProminenceGcd</c>/<c>TotalEclipseGcd</c> 那一对）——
	/// 否则会出现「正向被计时挡住、负向被 id 挡住」的双向封死，那一格谁都接不了（违反 docs\18 铁律②）。</para>
	///
	/// <para>⚠️ 上机验证前提：<c>Combo.Action</c> 在计时归零后到底会不会被游戏清空，本机还没实测过。
	/// 开 <c>Diagnostics.Enabled</c> 看 <c>[LittleDart-PLD/Combo]</c> 的 <c>combo=</c>/<c>left=</c> 两列即可分辨：
	/// 若计时归零时 <c>combo=</c> 也跟着变 0，本改动等价空操作；若 <c>combo=</c> 仍挂着旧 id，本改动就是那条修复。</para>
	/// </summary>
	public static bool IsComboAlive()
	{
		return GetComboTimeLeftSeconds() > 0.0;
	}

	public static uint GetLastComboSpellId()
	{
		return ActionHelper.GetLastComboID();
	}

	public static bool HasAura(uint auraId, int timeLeft = 0)
	{
		IBattleChara me = Core.Me;
		if (me == null)
		{
			return false;
		}
		if (!BattleCharaExtensions.HasStatus(me, auraId))
		{
			return false;
		}
		if (timeLeft <= 0)
		{
			return true;
		}
		return BattleCharaExtensions.GetStatusLeftTime(me, auraId) * 1000f >= (float)timeLeft;
	}

	public static bool IsRecentlyCast(uint spellId, int ms = 1500)
	{
		return ActionHelper.RecentlyUsed(spellId, ms);
	}

	public static int GetAuraStack(uint auraId)
	{
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return BattleCharaExtensions.GetStatusStackCount(me, auraId);
		}
		return 0;
	}

	public static bool HasCombo()
	{
		return GetLastComboSpellId() != 0 && IsComboAlive();
	}

	/// <summary>连击是否走到「暴乱剑刚打完」这一步（王权剑 / 强化圣灵的入口）。★ 口径同 <see cref="IsComboAlive"/>。</summary>
	public static bool IsAtRoyalAuthorityStep()
	{
		return GetLastComboSpellId() == 15 && IsComboAlive();
	}

	public static bool HasDivineMight()
	{
		return HasAura(2673u);
	}

	/// <summary>
	/// 悔罪 16459 的**入场券**（「悔罪连链头」唯一的合法性判据）。
	///
	/// <para>★ 2026-10-16 实测（本机 PRLog，Lv100 副本，逐行对齐毫秒）：
	/// <c>绝对统治 36921</c> 生效时**同时**给 <c>1368 安魂祈祷(4 层)</c> 与 <c>3019 悔罪预备</c>；
	/// <b>悔罪 16459 消耗的正是 3019</b>（日志里 3019 的 Remove 与悔罪的 ActionEffect 同一毫秒），
	/// 而 1368 要一路到 英勇之剑 才被最后一层扣光（4→3→2→1→0，每把剑扣一层；
	/// 注意 <b>圣环 16458 也会扣一层</b> ⇒ 层数**不能**当链条游标用）。
	/// 结论：链头的正确判据是 <b>3019（一次性入场券）</b>，不是 1368（整条链里恒真 30 秒）。</para>
	///
	/// <para>旧写法 <c>HasAura(1368) || HasAura(3019)</c> 造成的就是玩家报的
	/// 「Lv100 战逃内悔罪连打不出去 / 一直卡手」：链一旦开始 1368 还在 ⇒ 链头判定**每一轮都成立**
	/// ⇒ ACR 反复再交一次悔罪，游戏因 3019 已消耗而拒收（日志里这些 400ms 全是
	/// 「超过500ms未释放出技能 + 零 UAL」）。又因为 <c>RecentlyUsed</c> 是**决策时**记账
	///（本工程 <c>Resolve()</c> 里那句 <c>RecordAction</c>），被拒的那一发照样刷新 16459 的时间戳
	/// ⇒ 链头那个 1.5s「刚打过」闸门永不过期，并把 16459→25748→25749 的 5 秒窗口一路续下去，
	/// 链节因此要等前一段窗口走完（实测三种形态：2.9s / 4.5~5.2s / 12s）。</para>
	///
	/// <para>低等级回落：Lv96 以下没有绝对统治（<c>36921</c> 未解锁），按老口径
	/// (<c>1368 || 3019</c>) 保持不变 —— 本方法**只在 Lv96+ 收紧**，低等级本行为逐字不变。</para>
	/// </summary>
	public static bool CanStartConfiteor()
	{
		if (ActionHelper.IsUnlocked(36921u))
		{
			return HasAura(3019u);
		}
		return HasAura(1368u) || HasAura(3019u);
	}

	public static bool CanUseEmpoweredHolySpirit()
	{
		if (!HasDivineMight())
		{
			return false;
		}
		return IsAtRoyalAuthorityStep();
	}

	public static int GetAtonementChainStep()
	{
		if (HasAura(3828u))
		{
			return 3;
		}
		if (HasAura(3827u))
		{
			return 2;
		}
		if (HasAura(1902u))
		{
			return 1;
		}
		return 0;
	}

	public static bool IsAtonementChainRunning()
	{
		return GetAtonementChainStep() != 0;
	}

	public static bool HasValidTarget()
	{
		return GetCurrentTarget() != null;
	}

	/// <summary>
	/// 近战射程（米）—— 读 9（先锋剑）在 Action 表里的 Range，取不到就按 <see cref="MeleeRangeFallback"/>（3 米）。
	///
	/// <para>★ 2026-10-16 从 <see cref="IsInMeleeRange"/> 里抽出来：AOE 轴的
	/// 「要选目标的技能该打哪一只」要用它当兜底半径（<see cref="GetCastRadius"/>）。
	/// ⚠️ 先锋剑 9 在表里 Range = **-1**（实测）⇒ 实际一直走的是 3 米兜底。</para>
	/// </summary>
	public static float GetMeleeRange()
	{
		float actionRange = default;
		if (ActionHelper.TryGetActionRange(9u, out actionRange) && actionRange > 0f)
		{
			return actionRange;
		}
		return MeleeRangeFallback;
	}

	public static bool IsInMeleeRange()
	{
		if (GetCurrentTarget() == null)
		{
			return false;
		}
		return GetDistanceToTargetCircle() <= GetMeleeRange();
	}

	public static bool IsTargetFar()
	{
		if (!HasValidTarget())
		{
			return false;
		}
		return !IsInMeleeRange();
	}

	public static float GetDistanceToTarget()
	{
		IBattleChara currentTarget = GetCurrentTarget();
		IBattleChara me = Core.Me;
		if (currentTarget == null || me == null)
		{
			return float.MaxValue;
		}
		return PlanarDistance(me, currentTarget) - ((IGameObject)me).HitboxRadius - ((IGameObject)currentTarget).HitboxRadius;
	}

	public static float GetDistanceToTargetCircle()
	{
		IBattleChara currentTarget = GetCurrentTarget();
		IBattleChara me = Core.Me;
		if (currentTarget == null || me == null)
		{
			return float.MaxValue;
		}
		return PlanarDistance(me, currentTarget) - ((IGameObject)currentTarget).HitboxRadius;
	}

	public static bool PassesRangedHolySpiritGate()
	{
		return PassesRangedGate(PldSettings.Instance.RangedHolySpiritMinDistance);
	}

	public static bool PassesRangedShieldLobGate()
	{
		return PassesRangedGate(PldSettings.Instance.RangedShieldLobMinDistance);
	}

	public static bool IsDailyMode()
	{
		PldSettings instance = PldSettings.Instance;
		if (instance != null && instance.Mode == 1)
		{
			return true;
		}
		return PromeSettings.Instance.GetQt("日常模式");
	}

	private static bool PassesRangedGate(int minDistance)
	{
		if (!HasValidTarget())
		{
			return false;
		}
		return GetDistanceToTarget() >= (float)minDistance;
	}

	public static bool IsMoving()
	{
		return IsMovingForCast();
	}

	public static bool IsMovingForCast()
	{
		return MoveManager.IsLocalPlayerActuallyMoving;
	}

	public static double GetFofCooldownSeconds()
	{
		return ActionHelper.GetActionCooldown(20u);
	}

	public static uint GetBurstStarterSkill()
	{
		if (!ActionHelper.IsUnlocked(36921u))
		{
			return 7383u;
		}
		return 36921u;
	}

	public static double GetBurstStarterCooldownSeconds()
	{
		return ActionHelper.GetActionCooldown(GetBurstStarterSkill());
	}

	public static bool IsFofActive()
	{
		return HasAura(76u);
	}

	public static bool IsFofRecentCast()
	{
		return ActionHelper.RecentlyUsed(20u, 1500);
	}

	public static bool IsFofInside()
	{
		if (!IsFofActive())
		{
			return IsFofRecentCast();
		}
		return true;
	}

	public static bool IsFofOutside()
	{
		return !IsFofInside();
	}

	public static int GetNearbyEnemyCount(int range)
	{
		int enemyCount = 0;
		foreach (IGameObject item in (IEnumerable<IGameObject>)Svc.Objects)
		{
			IBattleChara enemy = (IBattleChara)(object)((item is IBattleChara) ? item : null);
			if (enemy != null && BattleCharaExtensions.IsEnemy(enemy) && GetDistanceTo(enemy) <= (float)range)
			{
				enemyCount++;
			}
		}
		return enemyCount;
	}

	public static double GetTargetAuraTimeLeft(uint auraId)
	{
		IBattleChara currentTarget = GetCurrentTarget();
		if (currentTarget == null)
		{
			return 0.0;
		}
		return BattleCharaExtensions.GetStatusLeftTime(currentTarget, auraId);
	}

	public static IBattleChara? GetAnotherTank()
	{
		foreach (IBattleChara item in PartyHelper.GetParty())
		{
			if (IsSupportCandidate(item) && IsTankJob(item))
			{
				return item;
			}
		}
		return null;
	}

	public static IBattleChara? GetLowestHpPartner(float maxDistance = 30f)
	{
		IBattleChara lowestHpPartner = null;
		float lowestHpPercent = float.MaxValue;
		foreach (IBattleChara item in PartyHelper.GetParty())
		{
			if (IsSupportCandidate(item) && !(GetDistanceTo(item) > maxDistance))
			{
				float hpPercent = PartyHelper.GetHpPercent(item);
				if (hpPercent < lowestHpPercent)
				{
					lowestHpPercent = hpPercent;
					lowestHpPartner = item;
				}
			}
		}
		return lowestHpPartner;
	}

	public static IBattleChara? GetSupportTarget(SupportTargetKind kind)
	{
		return kind switch
		{
			SupportTargetKind.Partner => GetAnotherTank(), 
			SupportTargetKind.Healer => GetLowestHpHealer(), 
			SupportTargetKind.LowestHp => GetLowestAbsoluteHpPartner(), 
			_ => (IBattleChara?)null, 
		};
	}

	public static IBattleChara? GetLowestAbsoluteHpPartner()
	{
		IBattleChara lowestHpPartner = null;
		uint lowestHp = uint.MaxValue;
		foreach (IBattleChara item in PartyHelper.GetParty())
		{
			if (IsSupportCandidate(item) && ((ICharacter)item).CurrentHp < lowestHp)
			{
				lowestHp = ((ICharacter)item).CurrentHp;
				lowestHpPartner = item;
			}
		}
		return lowestHpPartner;
	}

	public static IBattleChara? GetLowestHpHealer()
	{
		IBattleChara lowestHpHealer = null;
		uint lowestHp = uint.MaxValue;
		foreach (IBattleChara item in PartyHelper.GetParty())
		{
			if (IsSupportCandidate(item) && IsHealerJob(item) && !(GetDistanceTo(item) > 30f) && ((ICharacter)item).CurrentHp < lowestHp)
			{
				lowestHp = ((ICharacter)item).CurrentHp;
				lowestHpHealer = item;
			}
		}
		return lowestHpHealer;
	}

	private static bool IsSupportCandidate(IBattleChara? member)
	{
		if (member == null)
		{
			return false;
		}
		IBattleChara me = Core.Me;
		if (me != null && ((IGameObject)member).EntityId == ((IGameObject)me).EntityId)
		{
			return false;
		}
		return ((ICharacter)member).CurrentHp != 0;
	}

	private static bool IsTankJob(IBattleChara member)
	{
		return HealerHelper.IsTank((Job)((ICharacter)member).ClassJob.RowId);
	}

	private static bool IsHealerJob(IBattleChara member)
	{
		return HealerHelper.IsHealer((Job)((ICharacter)member).ClassJob.RowId);
	}

	public static float GetDistanceTo(IBattleChara? target)
	{
		IBattleChara me = Core.Me;
		if (target == null || me == null)
		{
			return float.MaxValue;
		}
		return PlanarDistance(me, target) - ((IGameObject)target).HitboxRadius;
	}

	public static float GetActionRange(uint actionId)
	{
		float actionRange = default;
		if (!ActionHelper.TryGetActionRange(actionId, out actionRange))
		{
			return 0f;
		}
		return actionRange;
	}

	public static bool IsReadyWithCanCast(uint actionId)
	{
		if (!AbilityGate.ReadyNow(actionId))
		{
			return false;
		}
		return IsInCastRange(actionId);
	}

	public static bool IsReadyAndInRangeForNextGcd(uint actionId)
	{
		if (AbilityGate.ReadyForNextGcd(actionId))
		{
			return IsInCastRange(actionId);
		}
		return false;
	}

	private static bool IsInCastRange(uint actionId)
	{
		float actionRange = default;
		if (!ActionHelper.TryGetActionRange(actionId, out actionRange) || actionRange <= 0f)
		{
			return IsInMeleeRange();
		}
		return GetDistanceToTargetCircle() <= actionRange;
	}

	public static bool ShouldUseAoe()
	{
		if (!PromeSettings.Instance.GetQt("AOE"))
		{
			return false;
		}
		return GetNearbyEnemyCount(5) >= PldSettings.Instance.AoeEnemyCount;
	}

	// ========================================================================
	//  ★★ 2026-10-16 玩家口径（与绝枪那一轮同一份口径）：
	//    「当自身 5 米内 ≥ 3 只怪物 ⇒ **不管当前目标，直接释放 AOE 技能**；
	//      若有要选目标的群体技能，则**对离自己最近的目标**释放」
	//
	//  ⚠️ 骑士这一轮**悔罪连一格不动**（玩家口径「悔罪连不动」）：
	//     悔罪 16459 / 信念之剑 25748 / 真理之剑 25749 / 英勇之剑 25750 / 荣耀之剑 36922
	//     以及 ConfiteorChain / ConfiteorDelay / FofAoeConfiteorGcd / FofConfiteor* /
	//     BladeOfHonorOffGcd —— 全部保持原判据（仍走带「有有效目标」的
	//     AoeQueue.ShouldUseAoe() 与 IsReadyAndInRangeForNextGcd）。
	//
	//  游戏内 Action 表实测（2026-10-16，`sheet_query Action <id>`，只抄字段）：
	//    · **自身中心（不选目标也能放）**：
	//        全蚀斩 7381 / 日珥斩 16457 / 圣环 16458 / 厄运流转 23
	//        —— Range = 0、EffectRange = 5、CastType = 2、CanTargetSelf = 是、CanTargetHostile = 否；
	//        战逃反应 20 —— Range = 0、EffectRange = 0（自身 buff）。
	//    · **必须选一只敌人**：
	//        偿赎剑 25747 —— Range = **-1**、EffectRange = 5（打目标周围的 AOE）、CanTargetSelf = 否；
	//        安魂祈祷 7383 —— Range = **-1**（近战）；
	//        绝对统治 36921 / 悔罪 16459 —— Range = **25**、EffectRange = 5。
	//    · 对照（证明「Range 的正负 0」这条判据是干净的）：
	//        先锋剑 9 / 绝枪的利刃斩 16137 —— Range = -1、EffectRange = 0（单体近战）。
	// ========================================================================

	/// <summary>
	/// 自身中心 AOE 技能的 EffectRange（米）—— 全蚀斩 7381 / 日珥斩 16457 / 圣环 16458 /
	/// 厄运流转 23 的 Action 表实测值都是 **5**（见上面那张表）。
	///
	/// <para>⚠️ 它和设置里的「AOE 半径」（<c>PldSettings.AoeRadius</c>，本机存档 = 6）
	/// **不是同一个量**：那个是玩家自己调的**数怪门槛半径**，这个是**技能真正能打到多远**。
	/// 「眼前有没有能打的怪」按后者算，才不会出现「门槛说够、出手却打空」。</para>
	/// </summary>
	public const int AoeEffectRange = 5;

	/// <summary>AOE 数怪半径（米）= 设置「AOE 半径」（读不到时按 <see cref="AoeEffectRange"/>）。</summary>
	public static float GetAoeRadius()
	{
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return AoeEffectRange;
		}
		return instance.AoeRadius;
	}

	/// <summary>
	/// AOE 轴是否成立（唯一实现在 <c>AoeQueue.ShouldUseAoeIgnoringTarget()</c>）：
	/// QT「AOE」开着 + 自身 <see cref="GetAoeRadius"/> 米内 ≥ 设置门槛 只怪 + 5 米内挑得出最近的一只，
	/// **完全不读当前选定的目标**。判定器要在「群体 / 单体」之间分流时读这一个。
	/// </summary>
	public static bool IsAoeClusterMet()
	{
		return AoeQueue.ShouldUseAoeIgnoringTarget();
	}

	/// <summary>
	/// <b><paramref name="range"/> 米内离自己最近的一只能打的怪</b>；没有 ⇒ <c>null</c>。
	///
	/// <para>过滤条件与 PR 的 <c>TargetHelper.EnemyInRange</c> 对齐：
	/// 只认 <c>IsDead: false</c> + <c>IsTargetable: true</c> + <see cref="BattleCharaExtensions.IsEnemy"/>
	/// （= PR 的 <c>CanUseAttackActionOn</c>）的怪；距离用 <see cref="GetDistanceTo"/>（**到目标圈**，
	/// 与 <see cref="GetNearbyEnemyCount"/> 同一口径）⇒「N 米内有几只」与「N 米内最近的是谁」
	/// 是同一把尺子。</para>
	/// </summary>
	public static IBattleChara? GetNearestEnemy(float range)
	{
		if (Core.Me == null)
		{
			return null;
		}
		IBattleChara nearest = null;
		float nearestDistance = float.MaxValue;
		foreach (IGameObject obj in Svc.Objects)
		{
			if (obj is not IBattleChara { IsDead: false, IsTargetable: true } enemy)
			{
				continue;
			}
			if (!BattleCharaExtensions.IsEnemy(enemy))
			{
				continue;
			}
			float distance = GetDistanceTo(enemy);
			if (distance > range || distance >= nearestDistance)
			{
				continue;
			}
			nearestDistance = distance;
			nearest = enemy;
		}
		return nearest;
	}

	/// <summary>Action 是不是**自身中心**（Action 表 Range = 0 ⇒ <c>ActionTargetType.Self</c>）。</summary>
	private static bool IsSelfCenteredAction(uint actionId)
	{
		ActionTargetType target = default;
		return ActionHelper.TryResolveActionTargetType(actionId, out target) && target == ActionTargetType.Self;
	}

	/// <summary>
	/// 「出手时离自己最近的那只必须在多远之内」—— 按技能自己的射程算：
	/// <list type="bullet">
	/// <item>Range &gt; 0（绝对统治 36921 / 悔罪 16459 = 25 米）⇒ 就用它；</item>
	/// <item>Range = 0（**自身中心** AOE：全蚀斩 / 日珥斩 / 圣环 / 厄运流转）⇒
	/// <see cref="AoeEffectRange"/>（5 米，技能真正的 EffectRange）；</item>
	/// <item>Range = -1（**必须选敌人的近战技**：偿赎剑 25747 / 安魂祈祷 7383）⇒
	/// <see cref="GetMeleeRange"/>（3 米）。</item>
	/// </list>
	///
	/// <para>⚠️ <c>ActionHelper.TryGetActionRange</c> 对负数返回 false ⇒ Range = 0 与 -1 在它眼里
	/// 都是「读不出来」，必须靠 <see cref="IsSelfCenteredAction"/> 区分；否则会拿 5 米去挑一只
	/// 4 米外的怪、让一个近战（-1）技能打空 —— 那就是铁律②的反面。</para>
	/// </summary>
	private static float GetCastRadius(uint actionId)
	{
		float actionRange = GetActionRange(actionId);
		if (actionRange > 0f)
		{
			return actionRange;
		}
		return IsSelfCenteredAction(actionId) ? AoeEffectRange : GetMeleeRange();
	}

	/// <summary>AOE 成立时：<see cref="GetCastRadius"/> 内挑得出最近的一只（不看当前选定目标）。</summary>
	public static bool HasEnemyInCastRange(uint actionId)
	{
		return GetNearestEnemy(GetCastRadius(actionId)) != null;
	}

	/// <summary>
	/// 【需要选中一只怪的技能】这一格该打谁 —— ★ 2026-10-16 玩家口径：
	/// <list type="number">
	/// <item><b>AOE 成立</b> ⇒ **不看当前选定的目标**：取「<see cref="GetCastRadius"/> 内离自己最近的一只」。
	/// 出手目标由 <c>PldAction.Aoe</c> 写进 <c>PAction.NetworkTid</c>（**不动玩家游戏里的选中目标**，
	/// 与绝枪那一轮同一套机制）；</item>
	/// <item><b>AOE 不成立</b> ⇒ **原样返回当前选定的目标**（连射程都不在这里判 ——
	/// 各判定器原本就有自己那道闸门，本次改动**只放宽、不收紧**）。</item>
	/// </list>
	/// </summary>
	public static IBattleChara? GetCastTarget(uint actionId)
	{
		if (IsAoeClusterMet())
		{
			return GetNearestEnemy(GetCastRadius(actionId));
		}
		return GetCurrentTarget();
	}

	/// <summary>
	/// 「眼前有能打的怪」的**目标无关**版本：AOE 成立 ⇒ 5 米内挑得出最近的一只；
	/// 否则 ⇒ 当前选定目标在近战射程内（= 老的 <see cref="IsInMeleeRange"/>）。
	///
	/// <para>给**本来就有近战射程要求**的那几格用（双伤：厄运流转 23 / 偿赎剑 25747 ——
	/// 它们原来的判据正是 <c>HasValidTarget() &amp;&amp; IsInMeleeRange()</c>）⇒ AOE 不成立时逐字等价。</para>
	/// </summary>
	public static bool HasEnemyToFight()
	{
		return IsAoeClusterMet() ? GetNearestEnemy(AoeEffectRange) != null : IsInMeleeRange();
	}

	/// <summary>
	/// 「有能打的怪」的**宽松档**：当前选定目标 != null（= 老判据，一个字不收紧）
	/// **或** AOE 成立且 5 米内挑得出最近的一只（= 本次新增的那条放宽）。
	///
	/// <para>给原来**只要求「有目标」**的那几格用（战逃反应 20 / 安魂祈祷 7383 / 绝对统治 36921）：
	/// 它们的技能自己还有更严的射程判据（<c>CanCast</c> / <c>ReadyWithCanCast</c>），
	/// 所以这里**只能放宽、不能顺手加近战要求** —— 那会把「25 米外也开得出的绝对统治」
	/// 悄悄收紧成 3 米（本次改动的一条硬规矩：**只放宽、不收紧**）。</para>
	/// </summary>
	public static bool HasTargetOrAoeCluster()
	{
		if (GetCurrentTarget() != null)
		{
			return true;
		}
		return IsAoeClusterMet() && GetNearestEnemy(AoeEffectRange) != null;
	}

	/// <summary>
	/// 【自身中心 AOE GCD 专用】<see cref="IsReadyAndInRangeForNextGcd"/> 的 AOE 版：
	/// 冷却那一半完全不看目标（<c>AbilityGate</c>），射程那一半在 **AOE 成立时**换成
	/// <see cref="HasEnemyInCastRange"/>（目标无关），**否则照旧**走 <see cref="IsInCastRange"/>。
	///
	/// <para>★ 「只放宽、不收紧」：AOE 不成立时这一支与老函数逐字等价
	/// ⇒ 单体轴的时序一行没变。</para>
	/// </summary>
	public static bool IsReadyAndInRangeForNextGcdAoe(uint actionId)
	{
		if (!AbilityGate.ReadyForNextGcd(actionId))
		{
			return false;
		}
		return IsAoeClusterMet() ? HasEnemyInCastRange(actionId) : IsInCastRange(actionId);
	}

	/// <summary>【AOE 轴 oGCD 专用】<see cref="IsReadyWithCanCast"/> 的 AOE 版（同上「只放宽、不收紧」）。</summary>
	public static bool IsReadyWithCanCastAoe(uint actionId)
	{
		if (!AbilityGate.ReadyNow(actionId))
		{
			return false;
		}
		return IsAoeClusterMet() ? HasEnemyInCastRange(actionId) : IsInCastRange(actionId);
	}

	public static void SetTankStance(bool on)
	{
		ActionQueueManager.Enqueue(PldAction.Self(on ? 28u : 32065u, (ActionType)1), false);
	}

	public static void ClearHighPriorityQueues()
	{
		ActionQueueManager.ClearAllQueues();
		Log("已清扫技能队列（PR：ClearAllQueues 会连强制/普通队列一起清）");
		Toast("已清扫技能队列", 1200);
	}

	public static void Log(string message)
	{
		LogTagged("[LittleDart-PLD] " + message);
	}

	public static void LogTagged(string taggedMessage)
	{
		PluginLog.Information(taggedMessage);
	}

	public static void Toast(string message, int time = 2000, int style = 1)
	{
		PldSettings instance = PldSettings.Instance;
		if (instance == null || instance.UseToast)
		{
			PluginLog.Information("[LittleDart-PLD] [提示] " + message);
		}
	}

	public static void ToastModeBanner(bool daily)
	{
		Toast("当前模式：" + (daily ? "日常" : "高难"), 1000, daily ? 1 : 2);
	}

	public static int GetOath()
	{
		try
		{
			return PLD.Oath;
		}
		catch
		{
			return 0;
		}
	}

	private static float PlanarDistance(IBattleChara a, IBattleChara b)
	{
		Vector3 position = ((IGameObject)a).Position;
		Vector3 position2 = ((IGameObject)b).Position;
		float deltaX = position.X - position2.X;
		float deltaZ = position.Z - position2.Z;
		return MathF.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
	}
}
