using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class DumpAtonementGcd : PldResolverBase
{
	protected override string ResolverDescription => "倾泻资源：赎罪剑";

	protected override CheckResult CheckReal()
	{
		if (!DumpQueue.IsEnabled())
		{
			return new CheckResult(false, "倾泻资源未开启 / 没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16460u))
		{
			return new CheckResult(false, "赎罪剑尚未解锁");
		}
		if (PldHelper.GetAtonementChainStep() != 1)
		{
			return new CheckResult(false, "没有赎罪剑预备（赎罪连不在第 1 段）");
		}
		return new CheckResult(true, "倾泻资源：打赎罪剑（第 1 档）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(16460u, (ActionType)0);
	}
}
