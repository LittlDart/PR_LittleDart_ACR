using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofRoyalAuthorityGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内：王权剑";

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
		if (!ActionHelper.IsUnlocked(PldSkills.王权剑))
		{
			return new CheckResult(false, "王权剑尚未解锁");
		}
		if (!PldHelper.IsAtRoyalAuthorityStep())
		{
			return new CheckResult(false, "连击不在「暴乱剑刚打完」这一步");
		}
		if (PldHelper.IsRecentlyCast(PldSkills.王权剑))
		{
			return new CheckResult(false, "王权剑刚刚才打过（等连击计数器刷新）");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "距离太远（不在近战范围）");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.王权剑))
		{
			return new CheckResult(false, "王权剑不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内：王权剑（连击收尾）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.王权剑, ActionType.Gcd);
	}
}
