using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class FofAoeConfiteorGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体战逃内：悔罪（悔罪连·链头）";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.IsFofInside())
		{
			return new CheckResult(false, "战逃外：交给单体悔罪那一套");
		}
		if (!AoeQueue.ShouldUseAoe())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 没有目标 / 敌人数量不够）");
		}
		if (!PldHelper.IsFofActive())
		{
			return new CheckResult(false, "战逃反应 buff 还没生效（只在读取盲区里靠本地记录兜底）");
		}
		if (!PldHelper.HasAura(1368u))
		{
			return new CheckResult(false, "没有「安魂祈祷」");
		}
		if (!ActionHelper.IsUnlocked(16459u))
		{
			return new CheckResult(false, "悔罪未解锁（80 级后才有）");
		}
		if (PldHelper.IsRecentlyCast(16459u))
		{
			return new CheckResult(false, "刚打过悔罪，等连击计数器刷新");
		}
		if (!IsReadyAndInRange(16459u))
		{
			return new CheckResult(false, "悔罪冷却中或不在射程内");
		}
		return new CheckResult(true, "群体战逃内：悔罪（悔罪连链头）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(16459u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
