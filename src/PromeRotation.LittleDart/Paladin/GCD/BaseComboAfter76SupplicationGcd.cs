using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class BaseComboAfter76SupplicationGcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招(76级后)：祈告剑";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(36918u))
		{
			return new CheckResult(false, "祈告剑未解锁（76 级后才有）");
		}
		if (!PldHelper.HasAura(3827u))
		{
			return new CheckResult(false, "没有「祈告剑预备」");
		}
		if (!IsReadyAndInRange(36918u))
		{
			return new CheckResult(false, "祈告剑冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招(76级后)：祈告剑");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(36918u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
