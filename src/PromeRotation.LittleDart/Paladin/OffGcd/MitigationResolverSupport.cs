using LittleDart.Common.Mitigation;
using LittleDart.Paladin.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

internal static class MitigationResolverSupport
{
	private static MitigationEngine Engine => MitigationScheduler.Instance.Engine;

	private static MitigationJobProfile Profile => PldMitigationProfile.Instance;

	public static CheckResult Check(MitigationSkillDef? def)
	{
		return LittleDart.Common.Mitigation.MitigationResolverSupport.Check(Engine, Profile, def);
	}

	public static PAction GetAction(MitigationSkillDef? def)
	{
		return LittleDart.Common.Mitigation.MitigationResolverSupport.GetAction(Engine, Profile, def);
	}
}
