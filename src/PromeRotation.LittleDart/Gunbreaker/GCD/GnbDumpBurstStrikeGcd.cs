using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbDumpBurstStrikeGcd : GnbDumpResolverBase
{
	protected override string ResolverDescription => "倾泻资源·无情外：爆发击";

	protected override CheckResult CheckReal()
	{
		CheckResult commonCheck = GnbDumpResolverBase.DumpCommonCheck();
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "在无情窗口内");
		}
		if (!ActionHelper.IsUnlocked(16162u))
		{
			return new CheckResult(false, "爆发击还没解锁（30 级）");
		}
		if (!GnbHelper.IsDumpAmmoAvailable())
		{
			return new CheckResult(false, "没有晶壤可倒");
		}
		return new CheckResult(true, "倾泻资源·无情外：倒爆发击");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16162u, (ActionType)0);
	}
}
