using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class FofHolyCircleRequiescatGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体战逃内：圣环（安魂祈祷状态中）";

	protected override CheckResult CheckReal()
	{
		if (!AoeQueue.ShouldUseAoe())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 没有目标 / 敌人数量不够）");
		}
		if (!PldHelper.IsFofInside())
		{
			return new CheckResult(false, "战逃外：交给普通群体圣环");
		}
		if (!ActionHelper.IsUnlocked(16458u))
		{
			return new CheckResult(false, "圣环未解锁（72 级后才有）");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "目标不在近战范围内");
		}
		if (!PldHelper.HasAura(1368u))
		{
			return new CheckResult(false, "没有「安魂祈祷」，用不上瞬发圣环");
		}
		if (PldHelper.IsRecentlyCast(16458u))
		{
			return new CheckResult(false, "刚打过圣环，等安魂层数刷新");
		}
		if (!IsReadyAndInRange(16458u))
		{
			return new CheckResult(false, "圣环冷却中或不在射程内");
		}
		return new CheckResult(true, "群体战逃内：圣环（消耗安魂层数）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(16458u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
