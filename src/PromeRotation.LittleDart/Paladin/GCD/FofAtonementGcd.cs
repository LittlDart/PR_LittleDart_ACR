using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofAtonementGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内：赎罪剑（只判预备 buff）";

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
		if (!ActionHelper.IsUnlocked(PldSkills.赎罪剑))
		{
			return new CheckResult(false, "赎罪剑尚未解锁");
		}
		if (!PldHelper.HasAura(PldSkills.赎罪剑预备))
		{
			return new CheckResult(false, "没有赎罪剑预备");
		}
		if (PldHelper.IsRecentlyCast(PldSkills.赎罪剑))
		{
			return new CheckResult(false, "赎罪剑刚刚才打过（等 buff 刷新）");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "距离太远（不在近战范围）");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.赎罪剑))
		{
			return new CheckResult(false, "赎罪剑不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内：赎罪剑（只判赎罪剑预备）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.赎罪剑, ActionType.Gcd);
	}
}
