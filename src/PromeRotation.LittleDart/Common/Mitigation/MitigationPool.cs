using System.Collections.Generic;

namespace LittleDart.Common.Mitigation;

public static class MitigationPool
{
	public static List<MitigationSkillDef> Build(IReadOnlyList<MitigationSkillDef> registry, IReadOnlyList<LevelSkillRow> levelTable, int level)
	{
		List<MitigationSkillDef> list = new List<MitigationSkillDef>(registry.Count);
		foreach (MitigationSkillDef item in registry)
		{
			if (!item.LevelAllowed(level))
			{
				continue;
			}
			MitigationSkillDef mitigationSkillDef = Clone(item);
			LevelSkillRow levelSkillRow = FindRow(levelTable, item.SkillId, level);
			if (levelSkillRow != null)
			{
				if (levelSkillRow.MitigationPercent >= 0.0)
				{
					mitigationSkillDef.MitigationPercent = levelSkillRow.MitigationPercent;
				}
				if (levelSkillRow.DurationSeconds >= 0.0)
				{
					mitigationSkillDef.DurationSeconds = levelSkillRow.DurationSeconds;
					mitigationSkillDef.DurationUpgradeLevel = 0;
				}
				if (levelSkillRow.CooldownSeconds >= 0.0)
				{
					mitigationSkillDef.CooldownSeconds = levelSkillRow.CooldownSeconds;
				}
			}
			list.Add(mitigationSkillDef);
		}
		return list;
	}

	public static double EffectiveCooldown(IReadOnlyList<LevelSkillRow> levelTable, uint skillId, int level)
	{
		return FindRow(levelTable, skillId, level)?.CooldownSeconds ?? (-1.0);
	}

	public static LevelSkillRow? FindRow(IReadOnlyList<LevelSkillRow> levelTable, uint skillId, int level)
	{
		LevelSkillRow result = null;
		foreach (LevelSkillRow item in levelTable)
		{
			if (item.SkillId == skillId && item.Covers(level))
			{
				result = item;
			}
		}
		return result;
	}

	public static MitigationSkillDef Clone(MitigationSkillDef d)
	{
		return new MitigationSkillDef
		{
			Name = d.Name,
			SkillId = d.SkillId,
			BuffId = d.BuffId,
			ShieldBuffId = d.ShieldBuffId,
			MinLevel = d.MinLevel,
			MaxLevel = d.MaxLevel,
			DurationSeconds = d.DurationSeconds,
			DurationSecondsUpgraded = d.DurationSecondsUpgraded,
			DurationUpgradeLevel = d.DurationUpgradeLevel,
			MitigationPercent = d.MitigationPercent,
			CooldownSeconds = d.CooldownSeconds,
			Category = d.Category,
			ResourceCost = d.ResourceCost,
			SelfTarget = d.SelfTarget,
			Priority = d.Priority,
			ReservedForBoss = d.ReservedForBoss,
			AoeOnly = d.AoeOnly,
			SingleBossForbidden = d.SingleBossForbidden,
			DeathSentenceForbidden = d.DeathSentenceForbidden,
			RequireAggroOnMeOver = d.RequireAggroOnMeOver,
			AoeEquivalentMitigation = d.AoeEquivalentMitigation
		};
	}
}
