using System.Collections.Generic;
using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation.Calc;

namespace LittleDart.Paladin.Mitigation;

public static class MitigationConfig
{

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
			Category = MitigationCategory.Rampart,
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
			Category = MitigationCategory.Minor,
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
			Category = MitigationCategory.Major,
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
			Category = MitigationCategory.Major,
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
			Category = MitigationCategory.Flavor,
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
			Category = MitigationCategory.Flavor,
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

}
