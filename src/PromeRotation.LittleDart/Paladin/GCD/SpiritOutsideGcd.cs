using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class SpiritOutsideGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃外：强化圣灵（留到 buff 将被覆盖）";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.IsFofOutside())
		{
			return new CheckResult(false, "战逃内：交给战逃内的强化圣灵决策器");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵未解锁（64 级后才有）");
		}
		if (!PldHelper.HasDivineMight())
		{
			return new CheckResult(false, "没有「神圣魔法效果提高」");
		}
		if (!PldHelper.IsAtRoyalAuthorityStep())
		{
			return new CheckResult(false, "连击没走到「暴乱剑刚打完」，强化圣灵先留着");
		}
		if (!IsReadyAndInRange(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵冷却中或不在射程内");
		}
		return new CheckResult(true, "战逃外：强化圣灵（连击马上要打王权剑，先用掉）");
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
