using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.Logging;
using LittleDart.Common;
using LittleDart.Gunbreaker.GCD;
using LittleDart.Gunbreaker.Setting;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using static PromeRotation.Helpers.JobGaugeHelper;
using PromeRotation.Managers;
using PromeRotation.PureTimeline;
using PromeRotation.Resolvers;
using PromeRotation.Timeline;

namespace LittleDart.Gunbreaker;

public static class GnbHelper
{
	public enum SupportTargetKind
	{
		Partner,

		Healer,

		LowestHp
	}

	private const float MeleeRangeFallback = 3f;

	private const float CastableAllyRange = 30f;

	public const int AoeEnemyCountRange = 5;

	private const double GnashingFangMaxCharges = 2.0;

	private const double GnashingFangRechargeSeconds = 30.0;

	private static DateTime _trajectoryPulledAtUtc = DateTime.MinValue;

	public const double OpenerBridgeWindowSeconds = 30.0;

	private static bool _openerBridgeConsumed;

	public const double OpenerPlanWindowSeconds = 40.0;

	private const double NoMercyReadyEpsilonSeconds = 0.1;

	public const double NoMercyTtkSeconds = 15.0;

	private const int TtkSampleWindowMs = 3000;

	private const int TtkSampleIntervalMs = 250;

	public const int NoMercyPrepGcdCount = 3;

	public const double NoMercyPrepMinGnashingCharges = 1.5;

	private const double BlastingZoneHoldSeconds = 10.0;

	private const float DefaultGcdSeconds = 2.5f;

	private const double GnashingChargeEpsilon = 0.98;

	private const int AmmoCapBeforeCartridgeTrait = 2;

	private const int AmmoCapAfterCartridgeTrait = 3;

	private const int BloodfestAmmoCap = 6;

	private const int CartridgeTraitLevel = 88;

	private const int LionHeartWindowReserveGcds = 2;

	public const int DefaultAoeComboEnemyCount = 4;

	private static readonly HpTracker Hp = new HpTracker(GetCurrentTarget);

	private const int HpSampleIntervalMs = 250;

	public const int Level100Floor = 100;

	public static bool HasTankStance()
	{
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return BattleCharaExtensions.HasStatus(me, GnbSkills.王室亲卫buff);
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

	public static uint GetLastComboSpellId()
	{
		return ActionHelper.GetLastComboID();
	}

	public static bool IsBrutalShellComboAlive()
	{
		if (GetLastComboSpellId() == GnbSkills.残暴弹)
		{
			return GetComboTimeLeftSeconds() > 0.0;
		}
		return false;
	}

	/// <summary>
	/// 基础三连「还需要继续」的唯一判定（docs\18 S1）：
	/// 上一段是 利刃斩 / 残暴弹，**且**连击窗口还活着。
	/// <para><b>机制依据（不是推理）</b>：游戏 <c>Action.PreservesCombo</c> 实测 ——
	/// 烈牙 16146 / 猛兽爪 16147 / 凶禽爪 16150 / 音速破 16153 / 爆发击 16162 / 倍攻 25760 /
	/// 崛起之心 36937 / 终结之心 36939 都是「是」（docs\16:73-77，游戏内 sheet_row 实读；
	/// docs\23 §11.1 的同名实测表覆盖爆发击 / 倍攻 / 音速破 / 狮心连）。
	/// 也就是说这些 GCD **不会**清掉基础连击窗口：打烈牙连期间 <c>GetLastComboID()</c>
	/// 仍可能停在 利刃斩/残暴弹，本方法也就会持续 true。
	/// 这正是「进无情前必须先把 123 补完 / 别在 1-2 段上起多 GCD 技能」的判据来源。</para>
	/// <para>⚠️ 不要改成「只看 GetComboTimeLeftSeconds() &gt; 0」——迅连斩打完（连击已完整）时
	/// 计时器同样还活着，但那时**没有待续段**，不需要补。</para>
	/// <para>与 <see cref="IsBrutalShellComboAlive"/> 的分工：那个是「停在残暴弹且窗口活着」的单段判据
	/// （起手相位尾巴在用）；本方法覆盖 1/2 两段，供连击护栏用。</para>
	/// </summary>
	public static bool IsBaseComboInProgress()
	{
		if (GetComboTimeLeftSeconds() <= 0.0)
		{
			return false;
		}
		uint lastComboId = GetLastComboSpellId();
		return lastComboId == GnbSkills.利刃斩 || lastComboId == GnbSkills.残暴弹;
	}

	/// <summary>
	/// 当前可用的续剑形态 ID；没有可打的续剑时返回 <c>0</c>。
	/// <para>「续剑能不能打」的判定**只允许存在这一处**（docs\18 S1）：
	/// <c>Always\GnbContinuationAlways</c>、<c>OffGcd\GnbBloodfestOffGcd</c>、
	/// <c>OffGcd\GnbBlastingZoneOffGcd</c>、<c>OffGcd\GnbBowShockOffGcd</c> 都调它 ——
	/// 后三个靠它让位，保证伤害 oGCD 永远不会抢在续剑前面出（2026-10-19 实测两次：
	/// ① 凶禽爪 → 无情 → **血壤** → 音速破，穿目整发丢失；
	/// ② 凶禽爪 → 无情 → **爆破领域** → 音速破，穿目整发丢失（01:20:18.708 / 19.603 / 20.407 / 21.253）。</para>
	/// <para>判据与原作者 <c>续剑.cs:15-19</c> 逐字一致：调整后 ID 已被替换 + 游戏侧高亮
	/// （<c>IsActionHighlighted</c> = 游戏自己告诉你「这个技能现在按得出来」）。</para>
	/// </summary>
	public static uint GetContinuationReadyActionId()
	{
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(GnbSkills.续剑);
		if (adjustedActionId == GnbSkills.续剑 || !adjustedActionId.IsActionHighlighted())
		{
			return 0u;
		}
		return adjustedActionId;
	}

	public static bool HasContinuationReady()
	{
		return GetContinuationReadyActionId() != 0;
	}

	/// <summary>
	/// 「续剑现在打得到、且这一格该优先给它」——伤害 oGCD 让位的统一前置（docs\18 S1）：
	/// 续剑形态可用 + 在近战射程内。血壤 / 爆破领域 / 弓形冲波三格都调它。
	/// <para>⚠️ 无反例时不要在调用点另写一份「续剑能不能打」的条件 —— 续剑的完整判据
	/// （GCD 窗口 / 防吞增伤 / 无情让位）在 <c>GnbContinuationAlways</c> 里，这里只回答
	/// 「有没有一发续剑正等着占这个 oGCD 窗口」。</para>
	/// </summary>
	public static bool HasContinuationReadyInMelee()
	{
		return HasContinuationReady() && IsInMeleeRange();
	}

	public static bool HasAura(uint auraId, int timeLeftMs = 0)
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
		if (timeLeftMs <= 0)
		{
			return true;
		}
		return BattleCharaExtensions.GetStatusLeftTime(me, auraId) * 1000f >= (float)timeLeftMs;
	}

	public static bool IsRecentlyCast(uint actionId, int ms = 1500)
	{
		return ActionHelper.RecentlyUsed(actionId, ms);
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

	public static int GetAmmo()
	{
		try
		{
			return GNB.Ammo;
		}
		catch
		{
			return 0;
		}
	}

	public static int GetAmmoComboStep()
	{
		try
		{
			return GNB.AmmoComboStep;
		}
		catch
		{
			return 0;
		}
	}

	public static double GetGnashingFangCharges()
	{
		double cooldownSeconds = SkillExtensions.GetActionCooldown(GnbSkills.烈牙);
		if (cooldownSeconds < 0.0)
		{
			cooldownSeconds = 0.0;
		}
		double maxCooldownSeconds = 60.0;
		if (cooldownSeconds > maxCooldownSeconds)
		{
			cooldownSeconds = maxCooldownSeconds;
		}
		double charges = 2.0 - cooldownSeconds / 30.0;
		if (charges < 0.0)
		{
			charges = 0.0;
		}
		if (charges > 2.0)
		{
			charges = 2.0;
		}
		return charges;
	}

	public static double GetNoMercyCooldownSeconds()
	{
		return SkillExtensions.GetActionCooldown(GnbSkills.无情);
	}

	public static bool IsNoMercyActive()
	{
		return HasAura(GnbSkills.无情buff);
	}

	public static bool IsNoMercyRecentCast()
	{
		return IsRecentlyCast(GnbSkills.无情);
	}

	public static bool IsNoMercyInside()
	{
		if (!IsNoMercyActive())
		{
			return IsNoMercyRecentCast();
		}
		return true;
	}

	public static bool IsDumpResource()
	{
		return PromeSettings.Instance.GetQt(GnbQt.DumpResource);
	}

	public static bool IsBattleStart()
	{
		return EngageManager.GetBattleTime() <= 0.0;
	}

	public static void NotifyTrajectoryPull()
	{
		_trajectoryPulledAtUtc = DateTime.UtcNow;
	}

	public static bool WasPulledByTrajectory()
	{
		if (_trajectoryPulledAtUtc != DateTime.MinValue)
		{
			return (DateTime.UtcNow - _trajectoryPulledAtUtc).TotalSeconds <= 3.0;
		}
		return false;
	}

	public static void ResetCountdownPullState()
	{
		_trajectoryPulledAtUtc = DateTime.MinValue;
	}

	public static bool HasTimelineRunning()
	{
		if (!TimelineManager.HasLoaded && TimelineManager.CurrentMeta == null && !PtlManager.HasLoaded)
		{
			return (int)PtlManager.State == 2;
		}
		return true;
	}

	public static bool IsPostOpenerBridgeWindow()
	{
		if (_openerBridgeConsumed || !PromeSettings.Instance.OpenerHasBeenExecuted)
		{
			return false;
		}
		double battleTime = EngageManager.GetBattleTime();
		if (battleTime > 0.0)
		{
			return battleTime <= 30.0;
		}
		return false;
	}

	public static void ConsumeOpenerBridge()
	{
		_openerBridgeConsumed = true;
	}

	public static void ResetOpenerBridgeState()
	{
		_openerBridgeConsumed = false;
	}

	public static bool IsPostOpenerPlanWindow()
	{
		if (!PromeSettings.Instance.OpenerHasBeenExecuted)
		{
			return false;
		}
		double battleTime = EngageManager.GetBattleTime();
		if (battleTime > 0.0)
		{
			return battleTime <= 40.0;
		}
		return false;
	}

	public const float NoMercyMinWeaveGcdRemain = 0.65f;

	public const float ContinuationMinWeaveGcdRemain = 0.6f;

	/// <summary>
	/// 血壤可以先于续剑占「第一织入格」的 GCD 余量下限（秒）。
	/// <para>来源：2026-10-19 玩家拍板「按方案 A 收窄血壤让位闸门」（docs\44）。
	/// 取值依据（按**客户端下发间隔**算，不是 AEP 口径）：
	/// 实测血壤→续剑的 UAL 下发间隔 0.753~0.813s ＋ 续剑自身织入门槛
	/// <see cref="ContinuationMinWeaveGcdRemain"/> = 0.6s ＋ 约 0.3s 余量 = <b>1.7s</b>。
	/// GCD 余量不到这个数时血壤仍然让位 —— 这是「血壤不许顶掉续剑」的余量保证，
	/// <b>不要把它调小</b>（独立复核：取 1.5 时最坏余量只有 0.087s≈5 帧，撑不住「绝不」）。</para>
	/// </summary>
	public const float BloodfestBeforeContinuationMinGcdRemain = 1.7f;

	public const long LandingNoMercyExpireMs = 3000L;

	public const long LandingRescueWindowMs = 2500L;

	private const float ReadyWindowPredictionSeconds = 0.3f;

	private static bool _hadAttackableTargetInCombat;

	private static bool _bossUntargetableInCombat;

	private static long _bossUntargetableSinceTick;

	private static long _bossLandedAtTick;

	private static bool _landingNoMercyAutoArmed;

	/// <summary>
	/// 技能当前帧硬闸门（P1-3）：用于真正出手前的判定。
	/// 避免与 <c>ActionHelper.IsReady</c> 同名，且单充能技能要求 CD ≤ 0.05s，防止 0.3s 提前量导致 UseAction 被拒并触发 RecentlyUsed 空转锁。
	/// <para>批次 D（docs\35 §8 T11）：转发到 <see cref="AbilityGate.IsReady"/>，本处不再自带判据。</para>
	/// </summary>
	public static bool CanCastNow(uint actionId)
	{
		return AbilityGate.IsReady(actionId, AbilityGate.ReadyNowEpsilonSeconds);
	}

	/// <summary>
	/// 多充能技能层数判定（P1-3）：明确要求充能进度 ≥ 指定层数（默认 1 层）。
	/// <para>批次 D（docs\35 §8 T11）：转发到 <see cref="AbilityGate.HasCharges"/>，本处不再自带判据。</para>
	/// </summary>
	public static bool HasCharge(uint actionId, float minCharges = 1f)
	{
		return AbilityGate.HasCharges(actionId, minCharges);
	}

	/// <summary>
	/// 未来窗口就绪预测（P1-3）：仅用于「续剑是否让位给即将转好的无情」等前瞻判断，不作为出手硬闸门。
	/// <para>批次 D（docs\35 §8 T11）：转发到 <see cref="AbilityGate.IsReady"/>，本处不再自带判据。</para>
	/// </summary>
	public static bool IsReadyWithinWindow(uint actionId, float windowSeconds = ReadyWindowPredictionSeconds)
	{
		return AbilityGate.IsReady(actionId, windowSeconds);
	}

	public static float GetGcdRemainSeconds()
	{
		return ActionHelper.GetGcdRemain();
	}

	public static bool IsNoMercyReady()
	{
		return CanCastNow(GnbSkills.无情);
	}

	/// <summary>
	/// 判断《无情》是否可在本 GCD 窗口内放出（用于续剑让位给无情，解除 6 分钟互锁）。
	/// P3 修复：统一使用 <see cref="NoMercyMinWeaveGcdRemain"/>（0.65s）消除与无情本体闸门的 0.05s 死区，并识别倾泻资源与日常 TTK 锁。
	/// </summary>
	public static bool WillNoMercyBeReadyInCurrentGcd()
	{
		if (!PromeSettings.Instance.GetQt(GnbQt.UseNoMercy) && !IsDumpResource())
		{
			return false;
		}
		if (IsDailyMode() && IsNoMercyLockedByDyingTarget())
		{
			return false;
		}
		if (GetGcdRemainSeconds() <= NoMercyMinWeaveGcdRemain)
		{
			return false;
		}
		if (IsReadyWithinWindow(GnbSkills.无情, ReadyWindowPredictionSeconds))
		{
			return true;
		}
		return SkillExtensions.AbilityCoolDownInNextXGcdsWindow(GnbSkills.无情, 0);
	}

	/// <summary>
	/// 续剑「本 GCD 主动给无情让位」的原因；不让位时返回 <c>null</c>。
	/// <para>★ 判定只此一处（docs\18 S1）：这组条件原先写在 <c>Always\GnbContinuationAlways</c> 的
	/// 第 1/2 条里，2026-10-19 抽到这里，因为 <c>OffGcd\GnbBloodfestOffGcd</c> 的让位闸门
	/// 必须用**同一份**判据 —— 续剑给无情让位的这段窗口里，血壤若抢走另一个 oGCD 槽，
	/// 续剑就会整发被挤掉（2026-10-19 实测：凶禽爪 → 无情 → 血壤 → 音速破，穿目整发丢失）。</para>
	/// <para>语义与 <c>GnbContinuationAlways.CheckReal</c> 原第 1/2 条逐字一致：
	/// ① 无情本 GCD 内可开（<see cref="WillNoMercyBeReadyInCurrentGcd"/>）；
	/// ② 无情刚按下但 buff 还没生效的 1s 防吞窗口（<c>RecentlyUsed(无情, 1000)</c>）。
	/// 无情 buff 已在身（<see cref="IsNoMercyActive"/>）时两条都不成立 —— 那不是「让位」，是正常窗口。</para>
	/// </summary>
	public static string? GetContinuationNoMercyYieldReason()
	{
		if (IsNoMercyActive())
		{
			return null;
		}
		if (WillNoMercyBeReadyInCurrentGcd())
		{
			return "无情已就绪：续剑让位，优先释放无情";
		}
		if (ActionHelper.RecentlyUsed(GnbSkills.无情, 1000))
		{
			return "无情刚释放但 Buff 尚未生效：暂缓续剑防止吞增伤";
		}
		return null;
	}

	/// <summary>
	/// 判定是否处于「高难战斗中 Boss 上天（不可选中）」状态（P1-1）：
	/// 严格区分「Boss 不可选中/消失」与「目标死亡 / 选中队友 / 走出 3 米近战射程」。
	/// </summary>
	public static bool IsBossUntargetableInCombat()
	{
		if (IsDailyMode() || EngageManager.GetBattleTime() <= 0.0 || !_hadAttackableTargetInCombat)
		{
			return false;
		}
		IBattleChara? target = Core.Target;
		if (target != null)
		{
			if (target.IsDead || !BattleCharaExtensions.IsEnemy(target))
			{
				return false;
			}
			if (((IGameObject)target).IsTargetable)
			{
				return false;
			}
		}
		return GetNearestEnemy(30f) == null;
	}

	/// <summary>
	/// 战斗中 Boss 上天与落地状态机每帧维护（P1-1 / P1-2）：
	/// 含落地时间戳记录、3 秒过期自动解除、以及无情长 CD 落地即刻卸载。
	/// </summary>
	public static void TickLandingNoMercyState()
	{
		if (IsDailyMode() || EngageManager.GetBattleTime() <= 0.0)
		{
			if (_landingNoMercyAutoArmed || _bossUntargetableInCombat || _hadAttackableTargetInCombat || _bossLandedAtTick != 0)
			{
				ResetLandingState();
			}
			return;
		}
		long now = Environment.TickCount64;
		IBattleChara? target = Core.Target;
		bool hasTargetableEnemy = (target != null && !target.IsDead && ((IGameObject)target).IsTargetable && BattleCharaExtensions.IsEnemy(target)) || GetNearestEnemy(30f) != null;
		if (hasTargetableEnemy)
		{
			_hadAttackableTargetInCombat = true;
			if (_bossUntargetableInCombat)
			{
				_bossUntargetableInCombat = false;
				_bossLandedAtTick = now;
				if (_landingNoMercyAutoArmed && GetNoMercyCooldownSeconds() > (double)(LandingNoMercyExpireMs / 1000.0))
				{
					DisarmLandingNoMercy("Boss 落地时无情仍在冷却中，解除落地无情武装");
				}
			}
			if (_landingNoMercyAutoArmed)
			{
				if (IsNoMercyActive())
				{
					DisarmLandingNoMercy("无情 Buff 已生效，解除落地无情");
				}
				else if (_bossLandedAtTick > 0 && now - _bossLandedAtTick > LandingNoMercyExpireMs)
				{
					DisarmLandingNoMercy($"落地已超过 {LandingNoMercyExpireMs}ms 过期窗口，自动解除落地无情");
				}
			}
		}
		else if (IsBossUntargetableInCombat())
		{
			if (!_bossUntargetableInCombat)
			{
				_bossUntargetableInCombat = true;
				_bossUntargetableSinceTick = now;
				_bossLandedAtTick = 0L;
			}
			if ((PromeSettings.Instance.GetQt(GnbQt.UseNoMercy) || IsDumpResource()) && !IsNoMercyInside() && !_landingNoMercyAutoArmed)
			{
				_landingNoMercyAutoArmed = true;
				PromeSettings.Instance.SetQt(GnbQt.LandingNoMercy, true);
				Log("[LandingNM] 检测到战斗中 Boss 上天（无可选中目标） → 已自动武装「落地无情」");
			}
		}
	}

	/// <summary>
	/// 是否处于「Boss 刚落地重获目标」的抢救窗口（P1-2：供 Always 续剑在刚落地时安全放行，避免平时 GCD=0 边界吞 GCD）。
	/// </summary>
	public static bool IsInLandingRescueWindow()
	{
		if (IsDailyMode() || EngageManager.GetBattleTime() <= 0.0 || _bossLandedAtTick <= 0)
		{
			return false;
		}
		long elapsed = Environment.TickCount64 - _bossLandedAtTick;
		return elapsed >= 0 && elapsed <= LandingRescueWindowMs;
	}

	/// <summary>
	/// 收到 ActionEffect 确认后解除落地无情（P1-1：配合 RequiresVerification，失败可重试、成功即卸载）。
	/// </summary>
	public static void ConfirmLandingNoMercyCast(uint actionId)
	{
		if (actionId == GnbSkills.无情 && (_landingNoMercyAutoArmed || PromeSettings.Instance.GetQt(GnbQt.LandingNoMercy)))
		{
			DisarmLandingNoMercy("收到无情 ActionEffect 出手确认，已关闭落地无情");
		}
	}

	public static void DisarmLandingNoMercy(string? reason = null)
	{
		bool wasArmed = _landingNoMercyAutoArmed || PromeSettings.Instance.GetQt(GnbQt.LandingNoMercy);
		_landingNoMercyAutoArmed = false;
		if (PromeSettings.Instance.GetQt(GnbQt.LandingNoMercy))
		{
			PromeSettings.Instance.SetQt(GnbQt.LandingNoMercy, false);
		}
		if (wasArmed && !string.IsNullOrEmpty(reason))
		{
			Log("[LandingNM] " + reason);
		}
	}

	public static void ResetLandingState()
	{
		_hadAttackableTargetInCombat = false;
		_bossUntargetableInCombat = false;
		_bossUntargetableSinceTick = 0L;
		_bossLandedAtTick = 0L;
		DisarmLandingNoMercy();
	}

	public static bool IsNoMercyComingSoon(int gcdCount = 3)
	{
		if (IsNoMercyReady())
		{
			return false;
		}
		return GetNoMercyCooldownSeconds() <= (double)((float)gcdCount * GetGcdSeconds());
	}

	public static bool IsGnashingAvailable()
	{
		if (HasGnashingChainInProgress())
		{
			return true;
		}
		if (GetGnashingFangCharges() >= 0.98)
		{
			return GetAmmo() > 0;
		}
		return false;
	}

	public static bool HasGnashingChainInProgress()
	{
		return GnbGnashingFangGcd.NextComboStep() != 0;
	}

	public static bool HasGnashingReserveCharge()
	{
		if (!HasGnashingChainInProgress())
		{
			return GetGnashingFangCharges() >= 1.96;
		}
		return true;
	}

	public static bool IsDumpAmmoAvailable()
	{
		return GetAmmo() > 0;
	}

	public static int GetNormalAmmoCap()
	{
		if (!IsLevelAtLeast(CartridgeTraitLevel))
		{
			return AmmoCapBeforeCartridgeTrait;
		}
		return AmmoCapAfterCartridgeTrait;
	}

	public static int GetAmmoCap()
	{
		if (IsLevelAtLeast(CartridgeTraitLevel) && HasAura(GnbSkills.血壤buff))
		{
			return BloodfestAmmoCap;
		}
		return GetNormalAmmoCap();
	}

	public static bool IsAmmoAtCap()
	{
		return GetAmmo() >= GetAmmoCap();
	}

	/// <summary>
	/// 日随无情外《血壤》超上限晶壤（&gt;3）防吞判定（待办 #11 · 已完成（docs\35 §8.3））：
	/// 当处于日常模式、不在无情窗口内、当前晶壤 &gt; 常规上限（3），且血壤超上限 Buff（5051）剩余时间 ≤ 无情剩余 CD（或即将到期吞晶壤）时返回 true。
	/// </summary>
	public static bool ShouldDumpBloodfestOvercapAmmo()
	{
		if (!IsDailyMode() || IsNoMercyInside())
		{
			return false;
		}
		int normalCap = GetNormalAmmoCap();
		int ammo = GetAmmo();
		if (ammo <= normalCap)
		{
			return false;
		}
		if (!HasAura(GnbSkills.血壤buff))
		{
			return true;
		}
		double bloodfestBuffLeft = GetAuraTimeLeft(GnbSkills.血壤buff);
		double noMercyCd = GetNoMercyCooldownSeconds();
		double minDrainSeconds = (double)(ammo - normalCap + 1) * (double)GetGcdSeconds();
		return bloodfestBuffLeft <= noMercyCd || bloodfestBuffLeft <= minDrainSeconds;
	}

	public static bool IsNoMercyPrepWindow()
	{
		if (IsNoMercyReady() || IsBattleStart())
		{
			return true;
		}
		return IsNoMercyComingSoon(NoMercyPrepGcdCount);
	}

	public static bool IsLionHeartHeld()
	{
		if (IsNoMercyInside() || IsDumpResource() || ShouldUseAoeCombo())
		{
			return false;
		}
		double auraTimeLeft = GetAuraTimeLeft(GnbSkills.心有灵狮);
		if (auraTimeLeft <= 0.0)
		{
			return false;
		}
		return auraTimeLeft > GetNoMercyCooldownSeconds() + (double)(2f * GetGcdSeconds());
	}

	/// <summary>
	/// QT「先打血壤连」是否真的改变**无情窗口的 GCD 先后**（2026-10-19 玩家拍板：倍攻抢在狮心连前可接受）。
	///
	/// <para>成立条件（全部满足）：QT「先打血壤连」开（默认开、预设两支都写 true）；
	/// 血壤已解锁、且技能位已被替换成狮心连（= 心有灵狮 buff 还在，狮心连打得到）。</para>
	///
	/// <para>调用点（判定只此一处，S1）：<c>GnbDoubleDownGcd</c>（决定倍攻是否给子弹连让位）、
	/// <c>GnbLionHeartGcd</c>（决定狮心连是否让位给子弹连）与 <c>GnbSonicBreakGcd</c>（决定音速破
	/// 是否让位给「未起头」的子弹连）。QT 关 ⇒ 全部回到社区轴
	/// （烈牙 → 倍攻 → 音速破 → 猛兽爪 → 凶禽爪 → 狮心连×3），与 2026-10-19 之后的行为完全一致。</para>
	/// </summary>
	public static bool ShouldPrioritizeLionHeartChain()
	{
		if (!PromeSettings.Instance.GetQt(GnbQt.BloodfestFirst))
		{
			return false;
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.血壤))
		{
			return false;
		}
		return ActionHelper.GetAdjustedActionId(GnbSkills.血壤) != GnbSkills.血壤;
	}

	public static bool IsPotionAmmoDumpEnabled()
	{
		if (PromeSettings.Instance.GetQt(GnbQt.PotionAmmoDump))
		{
			return HasPotionBuff();
		}
		return false;
	}

	public static bool IsNoMercyHeldOpen()
	{
		if (!IsNoMercyReady())
		{
			return false;
		}
		if (IsDumpResource())
		{
			return true;
		}
		return HasAura(GnbSkills.音速破预备);
	}

	public static bool DelayBloodfestUntilNoMercy()
	{
		return PromeSettings.Instance.GetQt(GnbQt.DelayBloodfest);
	}

	public static bool IsBlastingZoneHeld()
	{
		if (ShouldUseAoeCombo())
		{
			return false;
		}
		if (IsNoMercyInside())
		{
			return false;
		}
		if (IsNoMercyReady())
		{
			return true;
		}
		if (IsDumpResource())
		{
			return false;
		}
		return GetNoMercyCooldownSeconds() <= 10.0;
	}

	public static bool IsInGcdSecondHalf()
	{
		float gcdTotal = ActionHelper.GetGcdTotal();
		if (gcdTotal <= 0f)
		{
			return true;
		}
		float gcdElapsed = ActionHelper.GetGcdElapsed();
		if (gcdElapsed <= 0f)
		{
			return true;
		}
		return gcdElapsed * 2f >= gcdTotal;
	}

	public static bool IsLastNoMercyGcd()
	{
		if (!IsNoMercyActive())
		{
			return false;
		}
		double auraTimeLeft = GetAuraTimeLeft(GnbSkills.无情buff);
		if (auraTimeLeft > 0.0)
		{
			return auraTimeLeft <= (double)GetGcdSeconds();
		}
		return false;
	}

	public static bool HasPotionBuff()
	{
		return HasAura(49u);
	}

	public static float GetGcdSeconds()
	{
		float gcdTotal = ActionHelper.GetGcdTotal();
		if (!(gcdTotal > 0f))
		{
			return 2.5f;
		}
		return gcdTotal;
	}

	public static bool HasValidTarget()
	{
		return GetCurrentTarget() != null;
	}

	public static float GetMeleeRange()
	{
		float actionRange = default;
		if (ActionHelper.TryGetActionRange(GnbSkills.利刃斩, out actionRange) && actionRange > 0f)
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

	public static bool IsMoving()
	{
		return MoveManager.IsLocalPlayerActuallyMoving;
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


	public static IBattleChara? GetNearestEnemy(float range = AoeEnemyCountRange)
	{
		if (Core.Me == null)
		{
			return null;
		}
		IBattleChara? nearest = null;
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

	public static IBattleChara? GetCastTarget(uint actionId)
	{
		if (ShouldUseAoeCombo())
		{
			float actionRange = GetActionRange(actionId);
			return GetNearestEnemy(actionRange > 0f ? actionRange : GetMeleeRange());
		}
		if (!IsInMeleeRange())
		{
			return null;
		}
		return GetCurrentTarget();
	}

	public static IBattleChara? GetBloodfestCastTarget()
	{
		return ShouldUseAoeCombo() ? GetNearestEnemy() : GetCurrentTarget();
	}

	public static bool HasEnemyToFight()
	{
		return ShouldUseAoeCombo() ? GetNearestEnemy() != null : IsInMeleeRange();
	}

	public static CheckResult CheckAoeCluster(int threshold)
	{
		if (!PromeSettings.Instance.GetQt(GnbQt.UseAoe))
		{
			return new CheckResult(false, "QT「AOE」关着");
		}
		if (Core.Me == null)
		{
			return new CheckResult(false, "玩家还没加载出来");
		}
		int enemyCount = GetNearbyEnemyCount(AoeEnemyCountRange);
		if (enemyCount < threshold)
		{
			return new CheckResult(false, $"自身 {AoeEnemyCountRange} 米内怪不够（{enemyCount} < {threshold}）");
		}
		if (GetNearestEnemy() == null)
		{
			return new CheckResult(false, $"自身 {AoeEnemyCountRange} 米内没有能打的怪（{enemyCount} 只都不可选中 / 已死）");
		}
		return new CheckResult(true, $"AOE 成立：自身 {AoeEnemyCountRange} 米内 {enemyCount} 只（≥{threshold}）⇒ 不看当前选中目标");
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

	public static void SetTankStance(bool on)
	{
		ActionQueueManager.Enqueue(GnbAction.Self(on ? GnbSkills.王室亲卫 : GnbSkills.解除王室亲卫, (ActionType)1), false);
	}

	public static bool IsDailyMode()
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance != null && instance.Mode == 1)
		{
			return true;
		}
		return PromeSettings.Instance.GetQt(GnbQt.DailyMode);
	}

	public static bool ShouldUseAoe(int threshold = 3)
	{
		if (!PromeSettings.Instance.GetQt(GnbQt.UseAoe))
		{
			return false;
		}
		return GetNearbyEnemyCount(AoeEnemyCountRange) >= threshold;
	}

	public static int AoeComboThreshold => GnbSettings.Instance?.AoeComboEnemyCount ?? DefaultAoeComboEnemyCount;

	public static bool ShouldUseAoeCombo()
	{
		return ShouldUseAoe(AoeComboThreshold);
	}

	public static int GetAoeEnemyCount()
	{
		return GetNearbyEnemyCount(5);
	}

	public static void TickCombatTracking()
	{
		Hp.Tick(Environment.TickCount64, TtkSampleIntervalMs, TtkSampleWindowMs);
	}

	public static void ResetCombatTracking()
	{
		Hp.Reset();
		ResetLandingState();
	}

	public static bool IsLevelAtLeast(int level)
	{
		IBattleChara me = Core.Me;
		return ((me != null) ? ((ICharacter)me).Level : 0) >= level;
	}

	public static bool IsLevelAtLeast100()
	{
		return IsLevelAtLeast(100);
	}

	public static bool TryGetTargetTimeToDeath(out double seconds)
	{
		return TargetDeathPredictor.TryPredictSecondsToDeath(Hp, TtkSampleWindowMs, out seconds);
	}

	public static bool TargetSurvivesLongerThan(double seconds)
	{
		return Hp.SurvivesLongerThan(seconds, TtkSampleWindowMs);
	}

	public static bool IsNoMercyAoeConditionMet()
	{
		if (ShouldUseAoeCombo())
		{
			return TargetSurvivesLongerThan(NoMercyTtkSeconds);
		}
		return false;
	}

	public static bool IsNoMercyLockedByDyingTarget()
	{
		return !TargetSurvivesLongerThan(NoMercyTtkSeconds);
	}

	public static TargetProfile GetTargetProfile()
	{
		try
		{
			return TargetClassifier.Classify(GetNearbyEnemyCount((int)TargetClassifier.EngageRadius), TargetSurvivesLongerThan);
		}
		catch
		{
			return TargetProfile.Unknown;
		}
	}

	public static bool IsSingleBossFight()
	{
		return GetTargetProfile() == TargetProfile.SingleBoss;
	}

	public static bool IsPackFight()
	{
		return GetTargetProfile() == TargetProfile.Pack;
	}

	public static string DescribeTargetProfile()
	{
		int enemyCount = GetNearbyEnemyCount((int)TargetClassifier.EngageRadius);
		bool? survives = null;
		if (TryGetTargetTimeToDeath(out double ttk))
		{
			survives = ttk > TargetClassifier.BossSurvivesSeconds;
		}
		return TargetClassifier.DescribeWithInputs(GetTargetProfile(), enemyCount, survives);
	}

	public static void ClearHighPriorityQueues()
	{
		ActionQueueManager.ClearAllQueues();
		Log("已清扫技能队列（PR：ClearAllQueues 会连强制/普通队列一起清）");
		// ★ 2026-10-19「骑士同款」：清完弹一条 FF14 原生提示（PldHelper 逐字同款）。
		//   热键 GnbClearQueueHotkey 与 `/LittleDart-GNB clear` 都走这里 —— 提示只在这一个地方发。
		Toast("已清扫技能队列", 1200);
	}

	public static void Log(string message)
	{
		LogTagged("[LittleDart-GNB] " + message);
	}

	public static void LogTagged(string taggedMessage)
	{
		PluginLog.Information(taggedMessage);
	}

	/// <summary>
	/// 游戏内提示 —— 与骑士 <c>PldHelper.Toast</c> 逐字同构（2026-10-19 补齐 GNB 侧缺失的这条管道）：
	/// 走 PR 的 <c>HintHelper.ShowToast2</c>（sdk-ref:80665），落到的就是 FF14 **原生**那条画面中央提示
	/// （<see cref="HintHelper.HintType.Info"/> = 蓝 / <see cref="HintHelper.HintType.Warning"/> = 红）。
	/// <para>
	/// 受设置项「使用游戏内提示」（<see cref="GnbSettings.UseToast"/>，默认开）控制；
	/// 以前这个设置项在绝枪侧是**死的**（全工程没有任何读点，见 docs\39 §10 B4），本方法接上之后才有意义。
	/// </para>
	/// <para>失败只记日志、不影响主流程（拿不到 RaptureAtkModule 之类的极端情况不该拖垮切模式）。</para>
	/// </summary>
	/// <param name="message">提示正文。</param>
	/// <param name="time">显示时长（毫秒；内部 clamp 到 1–10 秒，与骑士同口径）。</param>
	/// <param name="style">1 = 蓝（Info），2 = 红（Warning）。取值口径与骑士 <c>PldHelper.Toast</c> 一致。</param>
	public static void Toast(string message, int time = 2000, int style = 1)
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance != null && !instance.UseToast)
		{
			return;
		}
		float seconds = Math.Clamp(time / 1000f, 1f, 10f);
		try
		{
			HintHelper.ShowToast2(message, seconds, (style == 2) ? HintHelper.HintType.Warning : HintHelper.HintType.Info);
		}
		catch (Exception exception)
		{
			Log("Toast 失败（不影响主流程）：" + exception.Message);
		}
		Log("[提示] " + message);
	}

	/// <summary>
	/// 切换「日常 / 高难」之后弹的那一条 —— 与骑士 <c>PldHelper.ToastModeBanner</c> 逐字同款：
	/// 日常 = 蓝（Info）、高难 = 红（Warning），时长 1 秒。
	/// <para>★ 调用点只允许两处入口（设置页开关 / <c>/LittleDart-GNB mode</c>）+ 一次 ACR 进场，
	/// 由 <see cref="GnbRotationEventHandler.ApplyModePreset"/> 统一发，**一条路径只弹一次**。</para>
	/// </summary>
	public static void ToastModeBanner(bool daily)
	{
		Toast("当前模式：" + (daily ? "日常" : "高难"), 1000, daily ? 1 : 2);
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
