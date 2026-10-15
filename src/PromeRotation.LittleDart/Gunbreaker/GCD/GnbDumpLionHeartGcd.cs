using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbDumpLionHeartGcd : GnbDumpResolverBase
{
	protected override string ResolverDescription => "倾泻资源·无情内：崛起之心连";

	protected override CheckResult CheckReal()
	{
		CheckResult commonCheck = GnbDumpResolverBase.DumpCommonCheck();
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "不在无情窗口内");
		}
		if (!ActionHelper.IsUnlocked(16164u))
		{
			return new CheckResult(false, "血壤还没解锁（76 级）");
		}
		if (ActionHelper.GetAdjustedActionId(16164u) == 16164)
		{
			return new CheckResult(false, "血壤没有被替换成狮心连");
		}
		return new CheckResult(true, "倾泻资源·无情内：打狮心连");
	}

	public override PAction GetAction()
	{
		return ActionHelper.GetAdjustedActionId(16164u) switch
		{
			36938u => GnbAction.Of(36938u, (ActionType)0),
			36939u => GnbAction.Of(36939u, (ActionType)0),
			_ => GnbAction.Of(36937u, (ActionType)0),
		};
	}
}
