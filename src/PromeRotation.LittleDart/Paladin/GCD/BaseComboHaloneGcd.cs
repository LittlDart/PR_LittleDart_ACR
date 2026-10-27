using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class BaseComboHaloneGcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招(60级前)：战女神之怒";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (ActionHelper.IsUnlocked(PldSkills.王权剑))
		{
			return new CheckResult(false, "王权剑已解锁（60 级后），第 3 段交给王权剑");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.战女神之怒))
		{
			return new CheckResult(false, "战女神之怒未解锁");
		}
		if (!PldHelper.IsAtRoyalAuthorityStep())
		{
			return new CheckResult(false, "连击没走到「暴乱剑刚打完」");
		}
		if (!IsReadyAndInRange(PldSkills.战女神之怒))
		{
			return new CheckResult(false, "战女神之怒冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招(60级前)：战女神之怒");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.战女神之怒, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
