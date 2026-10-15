using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public abstract class GnbDumpResolverBase : GnbResolverBase
{
	protected static CheckResult DumpCommonCheck()
	{
		if (!GnbHelper.IsDumpResource())
		{
			return new CheckResult(false, "倾泻资源未开启");
		}
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}

		if (GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, "群体连招中：倾泻资源让位给群体轴（不在群怪里倒单体）");
		}
		return new CheckResult(true, "倾泻资源");
	}
}
