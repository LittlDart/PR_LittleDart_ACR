using System.Collections.Generic;
using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation.Calc;

namespace LittleDart.Paladin.Mitigation;

public static class MitigationConfig
{
	public const int DailyModeValue = 1;

	public const int DefaultNextPullSeconds = 45;

	public const double MinPullIntervalSeconds = 3.0;

	public const double MaxPullIntervalSeconds = 180.0;

	public const int IntervalHistoryCount = 3;

	public const float EnemyCountRadius = 10f;

	public const int EnemySampleIntervalMs = 250;

	public const int HpSampleIntervalMs = 200;

	public const int DamageWindowMs = 2500;

	public const int Phase1EndMs = 1000;

	public const int Phase2StartMs = 2500;

	public const int WaveEndDebounceMs = 3000;

	public const int ReentryGuardMs = 1500;

	public const int EvalThrottleMs = 80;

	public const int OathCost = 50;

	public const float StressEnemyWeight = 1f;

	public const float StressDamageWeight = 2f;

	public const float StressMidThreshold = 4f;

	public const float StressHighThreshold = 8f;

	public const float InvulnThresholdPercent = 70f;

	public const int ThresholdPercentMin = 10;

	public const int ThresholdPercentMax = 35;

	public const int ScarceBigThresholdPercent = 25;

	public const int BigThresholdPercent = 35;

	public const int InvulnHpPercent = 25;

	public static readonly ReserveRequirement ReserveLow = MitigationDefaults.ReserveLow;

	public static readonly ReserveRequirement ReserveMid = MitigationDefaults.ReserveMid;

	public static readonly ReserveRequirement ReserveHigh = MitigationDefaults.ReserveHigh;

	public const int ReprisalMinEnemiesIn5m = 3;

	public const int AoeWarnMs = 1500;

	public const int ArmsLengthAggroOnMeOver = 3;

	public const int ReserveMinCooldownSeconds = 20;

	public const int MultiTargetEnemies = 3;

	public const double TargetDeathSkipSeconds = 10.0;

	public const double TargetDeathSkipCooldownSeconds = 60.0;

	public const double ClemencyDeathSeconds = 5.0;

	public static readonly MitigationSkillDef[] Registry = new MitigationSkillDef[10]
	{
		new MitigationSkillDef
		{
			Name = "铁壁",
			SkillId = 7531u,
			BuffId = 1191u,
			MinLevel = 8,
			MaxLevel = 0,
			DurationSeconds = 20.0,
			MitigationPercent = 20.0,
			CooldownSeconds = 90.0,
			Category = MitigationCategory.ScarceBig,
			Priority = 0,
			ReservedForBoss = true
		},
		new MitigationSkillDef
		{
			Name = "壁垒",
			SkillId = 22u,
			BuffId = 77u,
			MinLevel = 52,
			MaxLevel = 0,
			DurationSeconds = 10.0,
			MitigationPercent = 16.0,
			CooldownSeconds = 90.0,
			Category = MitigationCategory.ScarceBig,
			Priority = 1,
			ReservedForBoss = true
		},
		new MitigationSkillDef
		{
			Name = "预警",
			SkillId = 17u,
			BuffId = 74u,
			MinLevel = 38,
			MaxLevel = 91,
			DurationSeconds = 15.0,
			MitigationPercent = 30.0,
			CooldownSeconds = 120.0,
			Category = MitigationCategory.FrequentBig,
			Priority = 1,
			ReservedForBoss = false
		},
		new MitigationSkillDef
		{
			Name = "极致防御",
			SkillId = 36920u,
			BuffId = 3829u,
			MinLevel = 92,
			MaxLevel = 0,
			DurationSeconds = 15.0,
			MitigationPercent = 40.0,
			CooldownSeconds = 120.0,
			Category = MitigationCategory.FrequentBig,
			Priority = 0,
			ReservedForBoss = false
		},
		new MitigationSkillDef
		{
			Name = "盾阵",
			SkillId = 3542u,
			BuffId = 1856u,
			ShieldBuffId = 728u,
			MinLevel = 35,
			MaxLevel = 81,
			DurationSeconds = 4.0,
			MitigationPercent = 15.0,
			CooldownSeconds = 5.0,
			Category = MitigationCategory.Short,
			ResourceCost = 50,
			Priority = 1,
			DurationUpgradeLevel = 74,
			DurationSecondsUpgraded = 6.0
		},
		new MitigationSkillDef
		{
			Name = "圣盾阵",
			SkillId = 25746u,
			BuffId = 2674u,
			ShieldBuffId = 3026u,
			MinLevel = 82,
			MaxLevel = 0,
			DurationSeconds = 8.0,
			MitigationPercent = 15.0,
			CooldownSeconds = 5.0,
			Category = MitigationCategory.Short,
			ResourceCost = 50,
			Priority = 0
		},
		new MitigationSkillDef
		{
			Name = "血仇",
			SkillId = 7535u,
			BuffId = 1193u,
			MinLevel = 22,
			MaxLevel = 0,
			DurationSeconds = 10.0,
			MitigationPercent = 10.0,
			CooldownSeconds = 60.0,
			Category = MitigationCategory.Team,
			SelfTarget = false,
			Priority = 0,
			AoeEquivalentMitigation = 10.0
		},
		new MitigationSkillDef
		{
			Name = "圣光幕帘",
			SkillId = 3540u,
			BuffId = 727u,
			MinLevel = 56,
			MaxLevel = 0,
			DurationSeconds = 30.0,
			MitigationPercent = 0.0,
			CooldownSeconds = 90.0,
			Category = MitigationCategory.Team,
			Priority = 1,
			AoeOnly = true,
			AoeEquivalentMitigation = 15.0
		},
		new MitigationSkillDef
		{
			Name = "亲疏自行",
			SkillId = 7548u,
			BuffId = 1209u,
			MinLevel = 32,
			MaxLevel = 0,
			DurationSeconds = 12.5,
			MitigationPercent = 16.0,
			CooldownSeconds = 120.0,
			Category = MitigationCategory.Personal,
			Priority = 0,
			SingleBossForbidden = true,
			DeathSentenceForbidden = true,
			RequireAggroOnMeOver = 3
		},
		new MitigationSkillDef
		{
			Name = "神圣领域",
			SkillId = 30u,
			BuffId = 82u,
			MinLevel = 50,
			MaxLevel = 0,
			DurationSeconds = 10.0,
			MitigationPercent = 100.0,
			CooldownSeconds = 420.0,
			Category = MitigationCategory.Invuln,
			Priority = 0,
			ReservedForBoss = true
		}
	};

	public static List<MitigationSkillDef> PoolForLevel(int level)
	{
		return MitigationPool.Build(Registry, LevelMitigationProfile.Table, level);
	}

	public static MitigationSkillDef? Find(uint skillId)
	{
		MitigationSkillDef[] registry = Registry;
		foreach (MitigationSkillDef mitigationSkillDef in registry)
		{
			if (mitigationSkillDef.SkillId == skillId)
			{
				return mitigationSkillDef;
			}
		}
		return null;
	}

	public static ReserveRequirement MinReserve(PressureBand band)
	{
		return MitigationDefaults.MinReserve(band);
	}
}
