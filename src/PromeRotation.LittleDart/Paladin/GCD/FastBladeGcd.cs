using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

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
		if (!ActionHelper.IsUnlocked(PldSkills.先锋剑))
		{
			return new CheckResult(false, "先锋剑未解锁");
		}
		uint lastComboSpellId = PldHelper.GetLastComboSpellId();
		// ★ 只有「连击计时还在走」才算连击进行中（与 IsAtRoyalAuthorityStep 同一份口径）：
		//   计时走完后 id 可能还挂着，那时先锋剑才是正确的一格；否则这一格与暴乱剑/王权剑会双向封死。
		if (PldHelper.IsComboAlive() && (lastComboSpellId == PldSkills.先锋剑 || lastComboSpellId == PldSkills.暴乱剑))
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
		return PldAction.Of(PldSkills.先锋剑, ActionType.Gcd);
	}
}
