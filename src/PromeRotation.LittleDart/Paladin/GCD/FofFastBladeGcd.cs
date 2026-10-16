using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class FofFastBladeGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内：先锋剑";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.IsFofInside())
		{
			return new CheckResult(false, "不在战逃反应内");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(9u))
		{
			return new CheckResult(false, "先锋剑尚未解锁");
		}
		uint lastComboSpellId = PldHelper.GetLastComboSpellId();
		// ★ 同 FastBladeGcd：只有计时还在走才算「连击进行中」（王权剑/暴乱剑那两格用的是同一个 IsComboAlive 口径）。
		if (PldHelper.IsComboAlive() && (lastComboSpellId == 9 || lastComboSpellId == 15))
		{
			return new CheckResult(false, "连击进行中，让位给暴乱剑 / 王权剑");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "距离太远（不在近战范围）");
		}
		return new CheckResult(true, "战逃内：先锋剑（连击起手 / 兜底）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(9u, (ActionType)0);
	}
}
