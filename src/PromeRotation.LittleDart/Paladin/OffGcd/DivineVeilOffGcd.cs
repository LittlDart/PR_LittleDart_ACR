using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class DivineVeilOffGcd : PldResolverBase
{
	private static readonly MitigationSkillDef? Def = MitigationConfig.Find(PldSkills.圣光幕帘);

	protected override string ResolverDescription => "圣光幕帘（AOE团减，调度器）";

	protected override CheckResult CheckReal()
	{
		return MitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return MitigationResolverSupport.GetAction(Def);
	}
}
