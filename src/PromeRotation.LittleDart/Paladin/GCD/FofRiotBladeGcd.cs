using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofRiotBladeGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内：暴乱剑";

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
		if (!ActionHelper.IsUnlocked(PldSkills.暴乱剑))
		{
			return new CheckResult(false, "暴乱剑尚未解锁");
		}
		
		if (!PldHelper.IsComboAlive() || PldHelper.GetLastComboSpellId() != PldSkills.先锋剑)
		{
			return new CheckResult(false, "连击不在「先锋剑刚打完」这一步（或连击计时已走完）");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "距离太远（不在近战范围）");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.暴乱剑))
		{
			return new CheckResult(false, "暴乱剑不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内：暴乱剑（连击第 2 段）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.暴乱剑, ActionType.Gcd);
	}
}
