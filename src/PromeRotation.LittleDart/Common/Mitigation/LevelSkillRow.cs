namespace LittleDart.Common.Mitigation;

public sealed class LevelSkillRow
{
	public int MinLevel;

	public int MaxLevel;

	public uint SkillId;

	public double MitigationPercent = -1.0;

	public double DurationSeconds = -1.0;

	public double CooldownSeconds = -1.0;

	public bool Covers(int level)
	{
		if (level >= MinLevel)
		{
			if (MaxLevel > 0)
			{
				return level <= MaxLevel;
			}
			return true;
		}
		return false;
	}
}
