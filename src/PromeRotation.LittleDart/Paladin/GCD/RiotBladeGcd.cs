using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class RiotBladeGcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招：暴乱剑";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(15u))
		{
			return new CheckResult(false, "暴乱剑未解锁");
		}
		// ★ 与先锋剑那一格成对：计时走完后不能再接暴乱剑（否则先锋剑被 id 挡、这里被计时挡 = 双向封死）。
		if (!PldHelper.IsComboAlive() || PldHelper.GetLastComboSpellId() != 9)
		{
			return new CheckResult(false, "连击不对：上一段不是先锋剑（或连击计时已走完）");
		}
		if (!IsReadyAndInRange(15u))
		{
			return new CheckResult(false, "暴乱剑冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招：暴乱剑（连击第 2 段）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(15u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
