using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class RoyalAuthorityGcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招：王权剑";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(3539u))
		{
			return new CheckResult(false, "王权剑未解锁（60 级前由战女神之怒负责）");
		}
		if (!PldHelper.IsAtRoyalAuthorityStep())
		{
			return new CheckResult(false, "连击没走到「暴乱剑刚打完」");
		}
		if (PldHelper.IsRecentlyCast(3539u))
		{
			return new CheckResult(false, "刚打过王权剑，等连击计数器刷新");
		}
		if (!IsReadyAndInRange(3539u))
		{
			return new CheckResult(false, "王权剑冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招：王权剑（连击第 3 段）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(3539u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
