using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class DumpSepulchreGcd : PldResolverBase
{
	protected override string ResolverDescription => "倾泻资源：葬送剑";

	protected override CheckResult CheckReal()
	{
		if (!DumpQueue.IsEnabled())
		{
			return new CheckResult(false, "倾泻资源未开启 / 没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(36919u))
		{
			return new CheckResult(false, "葬送剑尚未解锁");
		}
		if (PldHelper.GetAtonementChainStep() != 3)
		{
			return new CheckResult(false, "没有葬送剑预备（赎罪连不在第 3 段）");
		}
		return new CheckResult(true, "倾泻资源：打葬送剑（第 3 档）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(36919u, (ActionType)0);
	}
}
