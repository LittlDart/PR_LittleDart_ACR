using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofSpiritRequiescatGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内：圣灵（安魂祈祷状态中）";

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
		if (!ActionHelper.IsUnlocked(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵尚未解锁");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.安魂祈祷技能) && !ActionHelper.IsUnlocked(PldSkills.绝对统治))
		{
			return new CheckResult(false, "尚未学会安魂祈祷 / 绝对统治（68 级前）");
		}
		if (!PldHelper.HasAura(PldSkills.安魂祈祷))
		{
			return new CheckResult(false, "没有安魂祈祷状态");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "距离太远（不在近战范围）");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内：圣灵（安魂祈祷状态中）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.圣灵, ActionType.Gcd);
	}
}
