using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

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
		if (!ActionHelper.IsUnlocked(PldSkills.暴乱剑))
		{
			return new CheckResult(false, "暴乱剑未解锁");
		}
		
		if (!PldHelper.IsComboAlive() || PldHelper.GetLastComboSpellId() != PldSkills.先锋剑)
		{
			return new CheckResult(false, "连击不对：上一段不是先锋剑（或连击计时已走完）");
		}
		if (!IsReadyAndInRange(PldSkills.暴乱剑))
		{
			return new CheckResult(false, "暴乱剑冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招：暴乱剑（连击第 2 段）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.暴乱剑, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
