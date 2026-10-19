using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofConfiteorSoloGcd : PldResolverBase
{
	protected override string ResolverDescription => "80档战逃内：悔罪（悔罪连只有这一段）";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.IsFofInside())
		{
			return new CheckResult(false, "战逃外：交给单体悔罪那一套");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		
		if (!PldHelper.CanStartConfiteor())
		{
			return new CheckResult(false, "没有「悔罪预备」3019（安魂祈祷 7383 / 绝对统治 36921 才给，悔罪出手即消耗）");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.悔罪))
		{
			return new CheckResult(false, "悔罪未解锁（80 级后才有）");
		}
		if (PldHelper.IsRecentlyCast(PldSkills.悔罪))
		{
			return new CheckResult(false, "刚打过悔罪，等连击计数器刷新");
		}
		if (ConfiteorDelay.IsDelayed())
		{
			return new CheckResult(false, "延后悔罪开启中，悔罪往后挪 2 个 GCD");
		}
		if (!IsReadyAndInRange(PldSkills.悔罪))
		{
			return new CheckResult(false, "悔罪冷却中或不在射程内");
		}
		return new CheckResult(true, "80档战逃内：悔罪（只有这一段）");
	}

	public override PAction GetAction()
	{
		
		
		ConfiteorChain.NoteChainHeadDecision();
		return PldAction.Of(PldSkills.悔罪, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
