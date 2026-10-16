using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public abstract class GnbAoeComboResolverBase : GnbResolverBase
{
	protected static CheckResult AoeCommonCheck()
	{
		return GnbHelper.CheckAoeCluster(GnbHelper.AoeComboThreshold);
	}
}
