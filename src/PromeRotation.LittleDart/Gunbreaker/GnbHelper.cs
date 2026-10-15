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

	public const int GcdReentryGuardMs = 1500;

	private static DateTime _trajectoryPulledAtUtc = DateTime.MinValue;

	public const double OpenerBridgeWindowSeconds = 30.0;

	private static bool _openerBridgeConsumed;

	public const double OpenerPlanWindowSeconds = 40.0;

	private const double NoMercyReadyEpsilonSeconds = 0.1;

	public const double NoMercyTtkSeconds = 15.0;

	private const int TtkSampleWindowMs = 3000;

	private const int TtkSampleIntervalMs = 250;

	public const int NoMercyPrepGcdCount = 3;

	private const int BloodfestPrepGcdCount = 3;

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
			return BattleCharaExtensions.HasStatus(me, 1833u);
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
		if (GetLastComboSpellId() == 16139)
		{
			return GetComboTimeLeftSeconds() > 0.0;
		}
		return false;
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
		double cooldownSeconds = SkillExtensions.GetActionCooldown(16146u);
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
		return SkillExtensions.GetActionCooldown(16138u);
	}

	public static double GetBloodfestCooldownSeconds()
	{
		return SkillExtensions.GetActionCooldown(16164u);
	}

	public static bool IsNoMercyActive()
	{
		return HasAura(1831u);
	}

	public static bool IsNoMercyRecentCast()
	{
		return IsRecentlyCast(16138u);
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
		return PromeSettings.Instance.GetQt("倾泻资源");
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

	public static bool IsNoMercyReady()
	{
		return GetNoMercyCooldownSeconds() <= 0.1;
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
		if (IsLevelAtLeast(CartridgeTraitLevel) && HasAura(5051u))
		{
			return BloodfestAmmoCap;
		}
		return GetNormalAmmoCap();
	}

	public static bool IsAmmoAtCap()
	{
		return GetAmmo() >= GetAmmoCap();
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
		double auraTimeLeft = GetAuraTimeLeft(3840u);
		if (auraTimeLeft <= 0.0)
		{
			return false;
		}
		return auraTimeLeft > GetNoMercyCooldownSeconds() + (double)(2f * GetGcdSeconds());
	}

	public static bool IsPotionAmmoDumpEnabled()
	{
		if (PromeSettings.Instance.GetQt("爆发药内卸晶壤"))
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
		return HasAura(3886u);
	}

	public static int BloodfestWaitGcdCount()
	{
		return 3;
	}

	public static bool DelayBloodfestUntilNoMercy()
	{
		return PromeSettings.Instance.GetQt("延后血壤");
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
		double auraTimeLeft = GetAuraTimeLeft(1831u);
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
		if (ActionHelper.TryGetActionRange(16137u, out actionRange) && actionRange > 0f)
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

	public static bool IsReadyWithCanCastOrCharged(uint actionId)
	{
		if (IsReadyWithCanCast(actionId))
		{
			return true;
		}
		if (ActionHelper.GetMaxCharges(actionId) <= 1)
		{
			return false;
		}
		if (Core.Me == null || GameData.IsPlayerOccupied())
		{
			return false;
		}
		if (!ActionHelper.IsUnlocked(actionId))
		{
			return false;
		}
		if (SkillExtensions.GetActionCharges(actionId) < 1f)
		{
			return false;
		}
		return IsInCastRange(actionId);
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
		ActionQueueManager.Enqueue(GnbAction.Self(on ? 16142u : 32068u, (ActionType)1), false);
	}

	public static bool IsDailyMode()
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance != null && instance.Mode == 1)
		{
			return true;
		}
		return PromeSettings.Instance.GetQt("日常模式");
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
	}

	public static void Log(string message)
	{
		LogTagged("[LittleDart-GNB] " + message);
	}

	public static void LogTagged(string taggedMessage)
	{
		PluginLog.Information(taggedMessage);
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
