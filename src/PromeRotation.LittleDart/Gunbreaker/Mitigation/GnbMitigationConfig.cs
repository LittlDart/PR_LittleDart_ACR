using System.Collections.Generic;
using LittleDart.Common.Mitigation;
using LittleDart.Gunbreaker.Skill;

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
			SkillId = GnbSkills.铁壁,
			BuffId = GnbSkills.铁壁buff,
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
			SkillId = GnbSkills.伪装,
			BuffId = GnbSkills.伪装buff,
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
			SkillId = GnbSkills.星云,
			BuffId = GnbSkills.星云buff,
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
			SkillId = GnbSkills.大星云,
			BuffId = GnbSkills.大星云buff,
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
			SkillId = GnbSkills.石之心,
			BuffId = GnbSkills.石之心buff,
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
			SkillId = GnbSkills.刚玉之心,
			BuffId = GnbSkills.刚玉之心buff,
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
			SkillId = GnbSkills.雪仇,
			BuffId = GnbSkills.雪仇buff,
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
			SkillId = GnbSkills.光之心,
			BuffId = GnbSkills.光之心buff,
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
			SkillId = GnbSkills.亲疏,
			BuffId = GnbSkills.亲疏buff,
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
			SkillId = GnbSkills.超火流星,
			BuffId = GnbSkills.超火流星buff,
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
			SkillId = GnbSkills.极光,
			BuffId = GnbSkills.极光buff,
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
			SkillId = GnbSkills.伪装,
			MitigationPercent = 10.0,
			DurationSeconds = 20.0,
			CooldownSeconds = 90.0
		},
		new LevelSkillRow
		{
			MinLevel = 8,
			MaxLevel = 0,
			SkillId = GnbSkills.铁壁,
			MitigationPercent = 20.0,
			DurationSeconds = 20.0,
			CooldownSeconds = 90.0
		},
		new LevelSkillRow
		{
			MinLevel = 22,
			MaxLevel = 0,
			SkillId = GnbSkills.雪仇,
			MitigationPercent = 10.0,
			DurationSeconds = 10.0,
			CooldownSeconds = 60.0
		},
		new LevelSkillRow
		{
			MinLevel = 32,
			MaxLevel = 0,
			SkillId = GnbSkills.亲疏,
			MitigationPercent = 16.0,
			DurationSeconds = 12.5,
			CooldownSeconds = 120.0
		},
		new LevelSkillRow
		{
			MinLevel = 38,
			MaxLevel = 91,
			SkillId = GnbSkills.星云,
			MitigationPercent = 30.0,
			DurationSeconds = 15.0,
			CooldownSeconds = 120.0
		},
		new LevelSkillRow
		{
			MinLevel = 50,
			MaxLevel = 0,
			SkillId = GnbSkills.超火流星,
			MitigationPercent = 100.0,
			DurationSeconds = 10.0,
			CooldownSeconds = 360.0
		},
		new LevelSkillRow
		{
			MinLevel = 64,
			MaxLevel = 0,
			SkillId = GnbSkills.光之心,
			MitigationPercent = 10.0,
			DurationSeconds = 15.0,
			CooldownSeconds = 90.0
		},
		new LevelSkillRow
		{
			MinLevel = 68,
			MaxLevel = 81,
			SkillId = GnbSkills.石之心,
			MitigationPercent = 15.0,
			DurationSeconds = 7.0,
			CooldownSeconds = 25.0
		},
		new LevelSkillRow
		{
			MinLevel = 82,
			MaxLevel = 0,
			SkillId = GnbSkills.刚玉之心,
			MitigationPercent = 15.0,
			DurationSeconds = 8.0,
			CooldownSeconds = 25.0
		},
		new LevelSkillRow
		{
			MinLevel = 92,
			MaxLevel = 0,
			SkillId = GnbSkills.大星云,
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
