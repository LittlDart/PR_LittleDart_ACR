using System;

namespace LittleDart.Common.Mitigation;

public sealed class MitigationSelfCareRule
{
	public MitigationSkillDef Def = new MitigationSkillDef();

	public Func<float, bool> Trigger = (float _) => false;

	public string Reason = string.Empty;
}
