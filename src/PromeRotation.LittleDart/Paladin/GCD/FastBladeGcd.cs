using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class FastBladeGcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招：先锋剑（兜底起手）";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(9u))
		{
			return new CheckResult(false, "先锋剑未解锁");
		}
		uint lastComboSpellId = PldHelper.GetLastComboSpellId();
		if (lastComboSpellId == 9 || lastComboSpellId == 15)
		{
			return new CheckResult(false, "连击进行中，让位给暴乱剑 / 王权剑");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "目标不在近战范围内，交给远离档");
		}
		return new CheckResult(true, "基础连招：先锋剑（兜底起手）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(9u, (ActionType)0);
	}
}
