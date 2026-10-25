using System.Collections.Generic;
using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Mitigation;

public static class MitigationConfig
{

	public static readonly MitigationSkillDef[] Registry = new MitigationSkillDef[10]
	{
		new MitigationSkillDef
		{
			Name = "铁壁",
			SkillId = PldSkills.铁壁,
			BuffId = PldSkills.铁壁buff,
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
			SkillId = PldSkills.壁垒,
			BuffId = PldSkills.壁垒buff,
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
			SkillId = PldSkills.预警,
			BuffId = PldSkills.预警buff,
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
			SkillId = PldSkills.极致防御,
			BuffId = PldSkills.极致防御buff,
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
			SkillId = PldSkills.盾阵,
			BuffId = PldSkills.盾阵buff,
			ShieldBuffId = PldSkills.盾阵格挡buff,
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
			SkillId = PldSkills.圣盾阵,
			BuffId = PldSkills.圣盾阵buff,
			ShieldBuffId = PldSkills.圣盾阵护盾buff,
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
			SkillId = PldSkills.血仇,
			BuffId = PldSkills.血仇buff,
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
			SkillId = PldSkills.圣光幕帘,
			BuffId = PldSkills.圣光幕帘buff,
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
			SkillId = PldSkills.亲疏,
			BuffId = PldSkills.亲疏buff,
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
			SkillId = PldSkills.神圣领域,
			BuffId = PldSkills.神圣领域buff,
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
