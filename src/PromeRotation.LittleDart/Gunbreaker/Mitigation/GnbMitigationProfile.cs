using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common.Mitigation;
using LittleDart.Gunbreaker.Setting;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;

namespace LittleDart.Gunbreaker.Mitigation;

public static class GnbMitigationProfile
{
	public static readonly MitigationJobProfile Instance = Build();

	private static MitigationJobProfile Build()
	{
		return new MitigationJobProfile
		{
			JobName = "绝枪战士",
			LogPrefix = "[LittleDart-GNB/Mit]",
			CommandHint = "/LittleDart-GNB mit",
			Registry = GnbMitigationConfig.Registry,
			LevelTable = GnbMitigationConfig.LevelTable,
			ReprisalSkillId = GnbSkills.雪仇,
			RefreshThresholds = RefreshThresholds,
			CanRun = () => GnbSettings.Instance != null,
			IsDailyMode = GnbHelper.IsDailyMode,
			AutoMitigationQt = () => PromeSettings.Instance.GetQt(GnbQt.UseAutoMitigation),
			StopAttackQt = () => PromeSettings.Instance.GetQt(GnbQt.StopAttack),
			DailyModeQt = () => PromeSettings.Instance.GetQt(GnbQt.DailyMode),
			Me = () => Core.Me,
			CurrentTarget = GnbHelper.GetCurrentTarget,
			HasValidTarget = GnbHelper.HasValidTarget,
			HasTankStance = GnbHelper.HasTankStance,
			HasSelfAura = (uint id) => GnbHelper.HasAura(id),
			TargetAuraTimeLeft = GnbHelper.GetTargetAuraTimeLeft,
			IsRecentlyCast = (uint id, int ms) => GnbHelper.IsRecentlyCast(id, ms),
			IsReadyWithCanCast = IsReadyWithCanCast,
			HasResourceFor = (MitigationSkillDef _) => true,
			IsEnemy = (IBattleChara chara) => BattleCharaExtensions.IsEnemy(chara),
			NearbyEnemyCount = (float radius) => GnbHelper.GetNearbyEnemyCount((int)Math.Round(radius)),
			DistanceTo = (IBattleChara chara) => GnbHelper.GetDistanceTo(chara),
			DistanceToTarget = GnbHelper.GetDistanceToTarget,
			DiagnosticsEnabled = () => GnbDiagnostics.Enabled,
			IsLocalPlayerMoving = () => GnbHelper.IsMoving(),
			InBossRoom = MitigationBossRoom.Probe,
			TargetSurvivesLongerThan = GnbHelper.TargetSurvivesLongerThan,
			Log = GnbHelper.LogTagged,
			SelfCareRules = BuildSelfCareRules(),
			BuildAction = (MitigationSkillDef def) => (!def.SelfTarget) ? GnbAction.Enemy(def.SkillId, (ActionType)1) : GnbAction.Self(def.SkillId, (ActionType)1)
		};
	}

	private static IReadOnlyList<MitigationSelfCareRule> BuildSelfCareRules()
	{
		List<MitigationSelfCareRule> list = new List<MitigationSelfCareRule>(2);
		MitigationSkillDef? corundum = GnbMitigationConfig.Find(GnbSkills.刚玉之心);
		if (corundum != null)
		{
			list.Add(new MitigationSelfCareRule
			{
				Def = corundum,
				Reason = $"自保·血量<{GnbMitigationConfig.SelfCareCorundumHpPercent}% 对自己交刚玉之心",
				Trigger = (float selfHp) => selfHp * 100f < (float)GnbMitigationConfig.SelfCareCorundumHpPercent
			});
		}
		MitigationSkillDef? aurora = GnbMitigationConfig.Find(GnbSkills.极光);
		if (aurora != null)
		{
			list.Add(new MitigationSelfCareRule
			{
				Def = aurora,
				Reason = $"自保·血量<{GnbMitigationConfig.SelfCareAuroraHpPercent}% 且身上没有极光 对自己交极光",
				Trigger = (float selfHp) => selfHp * 100f < (float)GnbMitigationConfig.SelfCareAuroraHpPercent && !GnbHelper.HasAura(GnbSkills.极光buff)
			});
		}
		return list;
	}

	private static bool IsReadyWithCanCast(MitigationSkillDef def)
	{
		if (!def.SelfTarget)
		{
			return GnbHelper.IsReadyWithCanCast(def.SkillId);
		}
		return ActionHelper.CanCast(def.SkillId, (IBattleChara)null);
	}

	private static void RefreshThresholds()
	{
		MitigationThresholds thresholds = Instance.Thresholds;
		GnbSettings instance = GnbSettings.Instance;
		if (instance != null)
		{
			thresholds.RequireTankStance = instance.MitigationRequireTankStance;
			thresholds.DamageWindowMs = instance.MitigationDamageWindowMs;
			thresholds.EnemyCountRadius = instance.MitigationEnemyCountRadius;
			thresholds.WaveEndDebounceMs = instance.MitigationWaveEndDebounceMs;
			thresholds.ReprisalMinEnemiesIn5m = instance.MitigationReprisalMinEnemiesIn5m;
			thresholds.TargetDeathSkipSeconds = instance.MitigationTargetDeathSkipSeconds;
			thresholds.TargetDeathSkipCooldownSeconds = instance.MitigationTargetDeathSkipCooldownSeconds;
			thresholds.InvulnHpPercent = instance.MitigationInvulnHpPercent;
		}
	}
}
