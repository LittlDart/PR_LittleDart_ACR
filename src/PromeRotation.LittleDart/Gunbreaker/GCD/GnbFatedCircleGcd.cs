using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbFatedCircleGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：命运之环（单体轴占位 · 见 GnbAoeComboGcd）";

	protected override CheckResult CheckReal()
	{
		return GnbResolverBase.SkipNotImplemented();
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16163u, (ActionType)0);
	}
}
