using LittleDart.Common.Mitigation;
using LittleDart.Gunbreaker.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbCamouflageOffGcd : GnbResolverBase
{
	private static readonly MitigationSkillDef? Def = GnbMitigationConfig.Find(GnbSkills.伪装);

	protected override string ResolverDescription => "减伤：伪装（稀缺大减 · 调度器）";

	protected override CheckResult CheckReal()
	{
		return GnbMitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return GnbMitigationResolverSupport.GetAction(Def);
	}
}
