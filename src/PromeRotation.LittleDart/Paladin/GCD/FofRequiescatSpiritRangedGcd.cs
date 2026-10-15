using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class FofRequiescatSpiritRangedGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内远离：圣灵（安魂祈祷状态中）";

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
		if (!ActionHelper.IsUnlocked(7383u) && !ActionHelper.IsUnlocked(36921u))
		{
			return new CheckResult(false, "尚未学会安魂祈祷 / 绝对统治（68 级前）");
		}
		if (!ActionHelper.IsUnlocked(7384u))
		{
			return new CheckResult(false, "圣灵尚未解锁");
		}
		if (!PldHelper.IsTargetFar())
		{
			return new CheckResult(false, "目标在近战范围内（该走近战档）");
		}
		if (!PldHelper.PassesRangedHolySpiritGate())
		{
			return new CheckResult(false, "距离未达「圣灵释放阈值」");
		}
		if (!PldHelper.HasAura(1368u))
		{
			return new CheckResult(false, "没有安魂祈祷状态");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(7384u))
		{
			return new CheckResult(false, "圣灵不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内远离：圣灵（安魂祈祷状态中）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(7384u, (ActionType)0);
	}
}
