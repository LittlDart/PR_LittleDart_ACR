using LittleDart.Common.Mitigation;
using LittleDart.Gunbreaker.Mitigation;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbAuroraOffGcd : GnbResolverBase
{
	private static readonly MitigationSkillDef? Def = GnbMitigationConfig.Find(GnbSkills.极光);

	protected override string ResolverDescription => "自保：极光（血量<70% 且身上没有极光 · 对自己 · 调度器）";

	protected override CheckResult CheckReal()
	{
		return GnbMitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return GnbMitigationResolverSupport.GetAction(Def);
	}
}
