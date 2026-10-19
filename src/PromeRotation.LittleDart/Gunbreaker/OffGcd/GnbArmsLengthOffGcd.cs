using LittleDart.Common.Mitigation;
using LittleDart.Gunbreaker.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbArmsLengthOffGcd : GnbResolverBase
{
	private static readonly MitigationSkillDef? Def = GnbMitigationConfig.Find(GnbSkills.亲疏);

	protected override string ResolverDescription => "减伤：亲疏自行（个人减 · 调度器）";

	protected override CheckResult CheckReal()
	{
		return GnbMitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return GnbMitigationResolverSupport.GetAction(Def);
	}
}
