using LittleDart.Common.Mitigation;
using LittleDart.Gunbreaker.Mitigation;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

internal static class GnbMitigationResolverSupport
{
	private static MitigationEngine Engine => GnbMitigationScheduler.Instance.Engine;

	private static MitigationJobProfile Profile => GnbMitigationProfile.Instance;

	public static CheckResult Check(MitigationSkillDef? def)
	{
		return MitigationResolverSupport.Check(Engine, Profile, def);
	}

	public static PAction GetAction(MitigationSkillDef? def)
	{
		return MitigationResolverSupport.GetAction(Engine, Profile, def);
	}
}
