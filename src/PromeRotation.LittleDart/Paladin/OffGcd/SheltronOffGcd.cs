using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class SheltronOffGcd : PldResolverBase
{
	private static readonly MitigationSkillDef? Def = MitigationConfig.Find(3542u);

	protected override string ResolverDescription => "盾阵（短减，圣盾阵的低级形态）（调度器）";

	protected override CheckResult CheckReal()
	{
		return MitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return MitigationResolverSupport.GetAction(Def);
	}
}
