using System;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;

namespace LittleDart.Paladin.Mitigation;

public static class PldMitigationProfile
{
	public static readonly MitigationJobProfile Instance = Build();

	private static MitigationJobProfile Build()
	{
		return new MitigationJobProfile
		{
			JobName = "骑士",
			LogPrefix = "[LittleDart-PLD/Mit]",
			CommandHint = "/LittleDart-PLD mit",
			Registry = MitigationConfig.Registry,
			LevelTable = LevelMitigationProfile.Table,
			ReprisalSkillId = 7535u,
			RefreshThresholds = RefreshThresholds,
			CanRun = () => PldSettings.Instance != null,
			IsDailyMode = PldHelper.IsDailyMode,
			AutoMitigationQt = () => PromeSettings.Instance.GetQt("自动减伤"),
			StopAttackQt = () => PromeSettings.Instance.GetQt("停手"),
			DailyModeQt = () => PromeSettings.Instance.GetQt("日常模式"),
			Me = () => Core.Me,
			CurrentTarget = PldHelper.GetCurrentTarget,
			HasValidTarget = PldHelper.HasValidTarget,
			HasTankStance = PldHelper.HasTankStance,
			HasSelfAura = (uint id) => PldHelper.HasAura(id),
			TargetAuraTimeLeft = PldHelper.GetTargetAuraTimeLeft,
			IsRecentlyCast = (uint id, int ms) => PldHelper.IsRecentlyCast(id, ms),
			IsReadyWithCanCast = IsReadyWithCanCast,
			HasResourceFor = (MitigationSkillDef def) => PldHelper.GetOath() >= def.ResourceCost,
			IsEnemy = (IBattleChara chara) => BattleCharaExtensions.IsEnemy(chara),
			NearbyEnemyCount = (float radius) => PldHelper.GetNearbyEnemyCount((int)Math.Round(radius)),
			DistanceTo = (IBattleChara chara) => PldHelper.GetDistanceTo(chara),
			DistanceToTarget = PldHelper.GetDistanceToTarget,
			DiagnosticsEnabled = () => Diagnostics.Enabled,
			Log = PldHelper.LogTagged,
			BuildAction = (MitigationSkillDef def) => (!def.SelfTarget) ? PldAction.Enemy(def.SkillId, (ActionType)1) : PldAction.Self(def.SkillId, (ActionType)1)
		};
	}

	private static bool IsReadyWithCanCast(MitigationSkillDef def)
	{
		if (!def.SelfTarget)
		{
			return PldHelper.IsReadyWithCanCast(def.SkillId);
		}
		return ActionHelper.CanCast(def.SkillId, (IBattleChara)null);
	}

	private static void RefreshThresholds()
	{
		MitigationThresholds thresholds = Instance.Thresholds;
		PldSettings instance = PldSettings.Instance;
		if (instance != null)
		{
			thresholds.RequireTankStance = instance.MitigationRequireTankStance;
			thresholds.DamageWindowMs = instance.MitigationDamageWindowMs;
			thresholds.EnemyCountRadius = instance.MitigationEnemyCountRadius;
			thresholds.WaveEndDebounceMs = instance.MitigationWaveEndDebounceMs;
			thresholds.DefaultNextPullSeconds = instance.MitigationDefaultNextPullSeconds;
			thresholds.Phase1EndMs = instance.MitigationPhase1EndMs;
			thresholds.Phase2StartMs = instance.MitigationPhase2StartMs;
			thresholds.ReprisalMinEnemiesIn5m = instance.MitigationReprisalMinEnemiesIn5m;
			thresholds.AoeWarnMs = instance.MitigationAoeWarnMs;
			thresholds.ArmsLengthAggroOver = instance.MitigationArmsLengthAggroOver;
			thresholds.TargetDeathSkipSeconds = instance.MitigationTargetDeathSkipSeconds;
			thresholds.TargetDeathSkipCooldownSeconds = instance.MitigationTargetDeathSkipCooldownSeconds;
			thresholds.ReserveMinCooldownSeconds = instance.MitigationReserveMinCooldownSeconds;
			thresholds.MultiTargetEnemies = instance.MitigationMultiTargetEnemies;
			thresholds.StressEnemyWeight = instance.MitigationStressEnemyWeight;
			thresholds.StressDamageWeight = instance.MitigationStressDamageWeight;
			thresholds.StressMidThreshold = instance.MitigationStressMidThreshold;
			thresholds.StressHighThreshold = instance.MitigationStressHighThreshold;
			thresholds.InvulnThresholdPercent = instance.MitigationInvulnThresholdPercent;
			thresholds.ScarceBigThresholdPercent = instance.MitigationScarceBigThresholdPercent;
			thresholds.BigThresholdPercent = instance.MitigationBigThresholdPercent;
			thresholds.InvulnHpPercent = instance.MitigationInvulnHpPercent;
		}
	}
}
