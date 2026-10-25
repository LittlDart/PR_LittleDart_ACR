using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofRangedSpiritGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内远离：圣灵（强化）";

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
		if (!PldHelper.HasDivineMight())
		{
			return new CheckResult(false, "没有神圣魔法效果提高");
		}
		if (PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "目标在近战范围内（该走近战档）");
		}
		if (!PldHelper.PassesRangedHolySpiritGate())
		{
			return new CheckResult(false, "距离未达「圣灵释放阈值」");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内远离：圣灵（神圣魔法效果提高中）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.圣灵, ActionType.Gcd);
	}
}
