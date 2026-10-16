namespace LittleDart.Common.Mitigation;

public sealed class MitigationSkillDef
{
	public string Name = string.Empty;

	public uint SkillId;

	public uint BuffId;

	public uint ShieldBuffId;

	public int MinLevel;

	public int MaxLevel;

	public double DurationSeconds;

	public double DurationSecondsUpgraded;

	public int DurationUpgradeLevel;

	public double MitigationPercent;

	public double CooldownSeconds;

	public MitigationCategory Category;

	public int ResourceCost;

	public bool SelfTarget = true;

	public int Priority;

	public bool ReservedForBoss;

	public bool AoeOnly;

	public bool SingleBossForbidden;

	public bool DeathSentenceForbidden;

	public int RequireAggroOnMeOver;

	public double AoeEquivalentMitigation;

	public string CategoryName => Category switch
	{
		MitigationCategory.Flavor => "特色减", 
		MitigationCategory.Rampart => "铁壁", 
		MitigationCategory.Minor => "小减", 
		MitigationCategory.Major => "大减", 
		MitigationCategory.Team => "团队减", 
		MitigationCategory.Personal => "个人减", 
		MitigationCategory.Invuln => "无敌", 
		MitigationCategory.SelfCare => "自保", 
		_ => "未知", 
	};

	public bool LevelAllowed(int level)
	{
		if (level < MinLevel)
		{
			return false;
		}
		if (MaxLevel > 0)
		{
			return level <= MaxLevel;
		}
		return true;
	}

	public double EffectiveDurationSeconds(int level)
	{
		if (DurationUpgradeLevel > 0 && level >= DurationUpgradeLevel)
		{
			return DurationSecondsUpgraded;
		}
		return DurationSeconds;
	}
}
