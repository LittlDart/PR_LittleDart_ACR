using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofGoringBladeGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内：沥血剑";

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
		if (!ActionHelper.IsUnlocked(PldSkills.沥血剑))
		{
			return new CheckResult(false, "沥血剑尚未解锁");
		}
		if (!PldHelper.HasAura(PldSkills.沥血剑预备))
		{
			return new CheckResult(false, "没有沥血剑预备");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.沥血剑))
		{
			return new CheckResult(false, "沥血剑不在可施放状态（冷却中或被占用）");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "距离太远（不在近战范围）");
		}
		return new CheckResult(true, "战逃内：打沥血剑（顺序写死第 1）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.沥血剑, ActionType.Gcd);
	}
}
