using System.Collections.Generic;
using LittleDart.Common.Mitigation;

namespace LittleDart.Paladin.Mitigation.Calc;

public static class LevelMitigationProfile
{
	public static readonly LevelSkillRow[] Table = new LevelSkillRow[11]
	{
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
			MinLevel = 35,
			MaxLevel = 73,
			SkillId = 3542u,
			MitigationPercent = 15.0,
			DurationSeconds = 4.0,
			CooldownSeconds = 5.0
		},
		new LevelSkillRow
		{
			MinLevel = 74,
			MaxLevel = 81,
			SkillId = 3542u,
			MitigationPercent = 15.0,
			DurationSeconds = 6.0,
			CooldownSeconds = 5.0
		},
		new LevelSkillRow
		{
			MinLevel = 38,
			MaxLevel = 91,
			SkillId = 17u,
			MitigationPercent = 30.0,
			DurationSeconds = 15.0,
			CooldownSeconds = 120.0
		},
		new LevelSkillRow
		{
			MinLevel = 50,
			MaxLevel = 0,
			SkillId = 30u,
			MitigationPercent = 100.0,
			DurationSeconds = 10.0,
			CooldownSeconds = 420.0
		},
		new LevelSkillRow
		{
			MinLevel = 52,
			MaxLevel = 0,
			SkillId = 22u,
			MitigationPercent = 16.0,
			DurationSeconds = 10.0,
			CooldownSeconds = 90.0
		},
		new LevelSkillRow
		{
			MinLevel = 56,
			MaxLevel = 0,
			SkillId = 3540u,
			MitigationPercent = 0.0,
			DurationSeconds = 30.0,
			CooldownSeconds = 90.0
		},
		new LevelSkillRow
		{
			MinLevel = 82,
			MaxLevel = 0,
			SkillId = 25746u,
			MitigationPercent = 15.0,
			DurationSeconds = 8.0,
			CooldownSeconds = 5.0
		},
		new LevelSkillRow
		{
			MinLevel = 92,
			MaxLevel = 0,
			SkillId = 36920u,
			MitigationPercent = 40.0,
			DurationSeconds = 15.0,
			CooldownSeconds = 120.0
		}
	};

	public static List<MitigationSkillDef> BuildPool(int level)
	{
		return MitigationPool.Build(MitigationConfig.Registry, Table, level);
	}

	public static double EffectiveCooldown(uint skillId, int level)
	{
		return MitigationPool.EffectiveCooldown(Table, skillId, level);
	}

	public static LevelSkillRow? FindRow(uint skillId, int level)
	{
		return MitigationPool.FindRow(Table, skillId, level);
	}
}
