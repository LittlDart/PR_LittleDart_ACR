using LittleDart.Common.Mitigation;
using LittleDart.Gunbreaker.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbRampartOffGcd : GnbResolverBase
{
	private static readonly MitigationSkillDef? Def = GnbMitigationConfig.Find(GnbSkills.铁壁);

	protected override string ResolverDescription => "减伤：铁壁（稀缺大减 · 调度器）";

	protected override CheckResult CheckReal()
	{
		return GnbMitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return GnbMitigationResolverSupport.GetAction(Def);
	}
}
