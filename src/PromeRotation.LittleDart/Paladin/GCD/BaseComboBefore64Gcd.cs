using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class BaseComboBefore64Gcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招(64级前)：王权剑";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.王权剑))
		{
			return new CheckResult(false, "王权剑未解锁");
		}
		if (PldHelper.IsRecentlyCast(PldSkills.王权剑))
		{
			return new CheckResult(false, "刚打过王权剑，等连击计数器刷新");
		}
		if (ActionHelper.IsUnlocked(PldSkills.圣灵))
		{
			return new CheckResult(false, "已经 64 级（圣灵已解锁），本档让位给 64 级后那一档");
		}
		if (!PldHelper.IsAtRoyalAuthorityStep())
		{
			return new CheckResult(false, "连击没走到「暴乱剑刚打完」");
		}
		if (!IsReadyAndInRange(PldSkills.王权剑))
		{
			return new CheckResult(false, "王权剑冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招(64级前)：王权剑");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.王权剑, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
