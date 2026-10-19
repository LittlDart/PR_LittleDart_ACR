using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofConfiteorGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内：悔罪（悔罪连·链头）";

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
		
		if (!PldHelper.CanStartConfiteor())
		{
			return new CheckResult(false, "没有悔罪的入场券（Lv96+ 要「悔罪预备」3019）");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.悔罪))
		{
			return new CheckResult(false, "悔罪尚未解锁");
		}
		if (PldHelper.IsRecentlyCast(PldSkills.悔罪))
		{
			return new CheckResult(false, "悔罪刚刚才打过（等 buff 刷新）");
		}
		if (ConfiteorDelay.IsDelayed())
		{
			return new CheckResult(false, "延后悔罪：还要等 2 个 GCD");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.悔罪))
		{
			return new CheckResult(false, "悔罪不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内：打悔罪（悔罪连链头）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.悔罪, ActionType.Gcd);
	}
}
