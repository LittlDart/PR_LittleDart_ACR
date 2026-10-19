namespace LittleDart.Common.Mitigation;

public readonly struct MitigationDecision(uint skillId, MitigationCategory category, string reason)
{
	public readonly uint SkillId = skillId;

	public readonly MitigationCategory Category = category;

	public readonly string Reason = reason;
}
