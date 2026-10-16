using System.Collections.Generic;
using LittleDart.Common.Mitigation;

namespace LittleDart.Gunbreaker.Mitigation;

public static class GnbMitigationConfig
{
	public const int SelfCareCorundumHpPercent = 50;

	public const int SelfCareAuroraHpPercent = 70;

	public static readonly MitigationSkillDef[] Registry = new MitigationSkillDef[11]
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
			Category = MitigationCategory.Rampart,
			Priority = 0,
			ReservedForBoss = true
		},
		new MitigationSkillDef
		{
			Name = "伪装",
			SkillId = 16140u,
			BuffId = 1832u,
			MinLevel = 6,
			MaxLevel = 0,
			DurationSeconds = 20.0,
			MitigationPercent = 10.0,
			CooldownSeconds = 90.0,
			Category = MitigationCategory.Minor,
			Priority = 1,
			ReservedForBoss = true
		},
		new MitigationSkillDef
		{
			Name = "星云",
			SkillId = 16148u,
			BuffId = 1834u,
			MinLevel = 38,
			MaxLevel = 91,
			DurationSeconds = 15.0,
			MitigationPercent = 30.0,
			CooldownSeconds = 120.0,
			Category = MitigationCategory.Major,
			Priority = 1
		},
		new MitigationSkillDef
		{
			Name = "大星云",
			SkillId = 36935u,
			BuffId = 3838u,
			MinLevel = 92,
			MaxLevel = 0,
			DurationSeconds = 15.0,
			MitigationPercent = 40.0,
			CooldownSeconds = 120.0,
			Category = MitigationCategory.Major,
			Priority = 0
		},
		new MitigationSkillDef
		{
			Name = "石之心",
			SkillId = 16161u,
			BuffId = 1840u,
			MinLevel = 68,
			MaxLevel = 81,
			DurationSeconds = 7.0,
			MitigationPercent = 15.0,
			CooldownSeconds = 25.0,
			Category = MitigationCategory.Flavor,
			Priority = 1
		},
		new MitigationSkillDef
		{
			Name = "刚玉之心",
			SkillId = 25758u,
			BuffId = 2683u,
			MinLevel = 82,
			MaxLevel = 0,
			DurationSeconds = 8.0,
			MitigationPercent = 15.0,
			CooldownSeconds = 25.0,
			Category = MitigationCategory.Flavor,
			Priority = 0
		},
		new MitigationSkillDef
		{
			Name = "雪仇",
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
			Name = "光之心",
			SkillId = 16160u,
			BuffId = 1839u,
			MinLevel = 64,
			MaxLevel = 0,
			DurationSeconds = 15.0,
			MitigationPercent = 10.0,
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
			Name = "超火流星",
			SkillId = 16152u,
			BuffId = 1836u,
			MinLevel = 50,
			MaxLevel = 0,
			DurationSeconds = 10.0,
			MitigationPercent = 100.0,
			CooldownSeconds = 360.0,
			Category = MitigationCategory.Invuln,
			Priority = 0,
			ReservedForBoss = true
		},
		new MitigationSkillDef
		{
			Name = "极光",
			SkillId = 16151u,
			BuffId = 1835u,
			MinLevel = 45,
			MaxLevel = 0,
			DurationSeconds = 18.0,
			MitigationPercent = 0.0,
			CooldownSeconds = 60.0,
			Category = MitigationCategory.SelfCare,
			Priority = 0
		}
	};

	public static readonly LevelSkillRow[] LevelTable = new LevelSkillRow[10]
	{
		new LevelSkillRow
		{
			MinLevel = 6,
			MaxLevel = 0,
			SkillId = 16140u,
			MitigationPercent = 10.0,
			DurationSeconds = 20.0,
			CooldownSeconds = 90.0
		},
		new LevelSkillRow
		{
			MinLevel = 8,
			MaxLevel = 0,
			SkillId = 7531u,
			MitigationPercent = 20.0,
			DurationSeconds = 20.0,
			CooldownSeconds = 90.0
		},
		new LevelSkillRow
		{
			MinLevel = 22,
			MaxLevel = 0,
			SkillId = 7535u,
			MitigationPercent = 10.0,
			DurationSeconds = 10.0,
			CooldownSeconds = 60.0
		},
		new LevelSkillRow
		{
			MinLevel = 32,
			MaxLevel = 0,
			SkillId = 7548u,
			MitigationPercent = 16.0,
			DurationSeconds = 12.5,
			CooldownSeconds = 120.0
		},
		new LevelSkillRow
		{
			MinLevel = 38,
			MaxLevel = 91,
			SkillId = 16148u,
			MitigationPercent = 30.0,
			DurationSeconds = 15.0,
			CooldownSeconds = 120.0
		},
		new LevelSkillRow
		{
			MinLevel = 50,
			MaxLevel = 0,
			SkillId = 16152u,
			MitigationPercent = 100.0,
			DurationSeconds = 10.0,
			CooldownSeconds = 360.0
		},
		new LevelSkillRow
		{
			MinLevel = 64,
			MaxLevel = 0,
			SkillId = 16160u,
			MitigationPercent = 10.0,
			DurationSeconds = 15.0,
			CooldownSeconds = 90.0
		},
		new LevelSkillRow
		{
			MinLevel = 68,
			MaxLevel = 81,
			SkillId = 16161u,
			MitigationPercent = 15.0,
			DurationSeconds = 7.0,
			CooldownSeconds = 25.0
		},
		new LevelSkillRow
		{
			MinLevel = 82,
			MaxLevel = 0,
			SkillId = 25758u,
			MitigationPercent = 15.0,
			DurationSeconds = 8.0,
			CooldownSeconds = 25.0
		},
		new LevelSkillRow
		{
			MinLevel = 92,
			MaxLevel = 0,
			SkillId = 36935u,
			MitigationPercent = 40.0,
			DurationSeconds = 15.0,
			CooldownSeconds = 120.0
		}
	};

	public static List<MitigationSkillDef> PoolForLevel(int level)
	{
		return MitigationPool.Build(Registry, LevelTable, level);
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
}
