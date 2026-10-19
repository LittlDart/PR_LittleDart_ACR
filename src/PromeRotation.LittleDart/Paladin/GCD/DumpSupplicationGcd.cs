using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class DumpSupplicationGcd : PldResolverBase
{
	protected override string ResolverDescription => "倾泻资源：祈告剑";

	protected override CheckResult CheckReal()
	{
		if (!DumpQueue.IsEnabled())
		{
			return new CheckResult(false, "倾泻资源未开启 / 没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.祈告剑))
		{
			return new CheckResult(false, "祈告剑尚未解锁");
		}
		if (PldHelper.GetAtonementChainStep() != 2)
		{
			return new CheckResult(false, "没有祈告剑预备（赎罪连不在第 2 段）");
		}
		return new CheckResult(true, "倾泻资源：打祈告剑（第 2 档）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.祈告剑, ActionType.Gcd);
	}
}
