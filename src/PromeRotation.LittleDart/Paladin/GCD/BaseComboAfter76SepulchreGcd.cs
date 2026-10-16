using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class BaseComboAfter76SepulchreGcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招(76级后)：葬送剑";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(36919u))
		{
			return new CheckResult(false, "葬送剑未解锁（76 级后才有）");
		}
		if (!PldHelper.HasAura(3828u))
		{
			return new CheckResult(false, "没有「葬送剑预备」");
		}
		if (!IsReadyAndInRange(36919u))
		{
			return new CheckResult(false, "葬送剑冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招(76级后)：葬送剑");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(36919u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
