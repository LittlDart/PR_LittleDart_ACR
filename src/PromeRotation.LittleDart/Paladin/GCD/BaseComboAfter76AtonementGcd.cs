using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class BaseComboAfter76AtonementGcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招(76级后)：赎罪剑";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.IsFofOutside())
		{
			return new CheckResult(false, "战逃内：交给战逃内的赎罪剑决策器");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16460u))
		{
			return new CheckResult(false, "赎罪剑未解锁（76 级后才有）");
		}
		if (!PldHelper.HasAura(1902u))
		{
			return new CheckResult(false, "没有「赎罪剑预备」");
		}
		if (PldHelper.IsRecentlyCast(16460u))
		{
			return new CheckResult(false, "刚打过赎罪剑，等连击计数器刷新");
		}
		if (!PldHelper.IsAtRoyalAuthorityStep())
		{
			return new CheckResult(false, "连击没走到「暴乱剑刚打完」");
		}
		if (!IsReadyAndInRange(16460u))
		{
			return new CheckResult(false, "赎罪剑冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招(76级后)：赎罪剑");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(16460u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
