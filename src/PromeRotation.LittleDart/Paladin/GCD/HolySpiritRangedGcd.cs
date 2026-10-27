using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class HolySpiritRangedGcd : PldResolverBase
{
	protected override string ResolverDescription => "远离：强化圣灵";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵未解锁（64 级后才有）");
		}
		if (!PldHelper.IsTargetFar())
		{
			return new CheckResult(false, "目标在近战范围内，交给近战基础连招");
		}
		if (!PldHelper.PassesRangedHolySpiritGate())
		{
			return new CheckResult(false, "没到「圣灵释放阈值」");
		}
		if (!PldHelper.HasDivineMight())
		{
			return new CheckResult(false, "没有「神圣魔法效果提高」");
		}
		if (!IsReadyAndInRange(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵冷却中或不在射程内");
		}
		return new CheckResult(true, "远离：强化圣灵（瞬发）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.圣灵, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
