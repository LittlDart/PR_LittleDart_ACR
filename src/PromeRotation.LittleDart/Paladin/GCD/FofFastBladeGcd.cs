using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

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
		if (!ActionHelper.IsUnlocked(PldSkills.先锋剑))
		{
			return new CheckResult(false, "先锋剑尚未解锁");
		}
		uint lastComboSpellId = PldHelper.GetLastComboSpellId();
		
		if (PldHelper.IsComboAlive() && (lastComboSpellId == PldSkills.先锋剑 || lastComboSpellId == PldSkills.暴乱剑))
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
		return PldAction.Of(PldSkills.先锋剑, ActionType.Gcd);
	}
}
