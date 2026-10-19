using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class ReprisalOffGcd : PldResolverBase
{
	private static readonly MitigationSkillDef? Def = MitigationConfig.Find(PldSkills.血仇);

	protected override string ResolverDescription => "血仇 / 雪仇（团队减，对敌目标）（调度器）";

	protected override CheckResult CheckReal()
	{
		return MitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return MitigationResolverSupport.GetAction(Def);
	}
}
