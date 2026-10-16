using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class BaseComboAfter64Gcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招(64级后)：圣灵";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(7384u))
		{
			return new CheckResult(false, "圣灵未解锁（64 级后才有）");
		}
		if (!PldHelper.CanUseEmpoweredHolySpirit())
		{
			return new CheckResult(false, "没有「神圣魔法效果提高」，或连击没走到王权剑那一步");
		}
		if (!PldHelper.IsAtRoyalAuthorityStep())
		{
			return new CheckResult(false, "连击没走到「暴乱剑刚打完」，王权剑不在连击内");
		}
		if (!IsReadyAndInRange(7384u))
		{
			return new CheckResult(false, "圣灵冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招(64级后)：强化圣灵");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(7384u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
