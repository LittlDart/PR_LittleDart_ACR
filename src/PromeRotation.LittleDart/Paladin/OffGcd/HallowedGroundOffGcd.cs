using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class HallowedGroundOffGcd : PldResolverBase
{
	private static readonly MitigationSkillDef? Def = MitigationConfig.Find(PldSkills.神圣领域);

	protected override string ResolverDescription => "神圣领域（无敌）（调度器）";

	protected override CheckResult CheckReal()
	{
		return MitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return MitigationResolverSupport.GetAction(Def);
	}
}
