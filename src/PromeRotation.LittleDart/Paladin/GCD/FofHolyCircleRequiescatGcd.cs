using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofHolyCircleRequiescatGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体战逃内：圣环（安魂祈祷状态中）";

	protected override CheckResult CheckReal()
	{
		// ★★ 2026-10-16 玩家口径：AOE 轴**不看当前选定的目标**（圣环 16458 是自身中心 AOE，
		//   Action 表 Range = 0 / EffectRange = 5 / CanTargetSelf = 是）。
		if (!AoeQueue.ShouldUseAoeIgnoringTarget())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 自身 5 米内能打的怪不够）");
		}
		if (!PldHelper.IsFofInside())
		{
			return new CheckResult(false, "战逃外：交给普通群体圣环");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.圣环))
		{
			return new CheckResult(false, "圣环未解锁（72 级后才有）");
		}
		if (!PldHelper.HasAura(PldSkills.安魂祈祷))
		{
			return new CheckResult(false, "没有「安魂祈祷」，用不上瞬发圣环");
		}
		if (PldHelper.IsRecentlyCast(PldSkills.圣环))
		{
			return new CheckResult(false, "刚打过圣环，等安魂层数刷新");
		}
		if (!IsReadyAndInRange(PldSkills.圣环))
		{
			return new CheckResult(false, "圣环冷却中或身前没有能打的怪");
		}
		return new CheckResult(true, "群体战逃内：圣环（消耗安魂层数）");
	}

	public override PAction GetAction()
	{
		// 圣环 16458 是自身中心（Range = 0）⇒ 不需要给「打哪只」，保持原样。
		return PldAction.Of(PldSkills.圣环, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		// ★ 2026-10-16：换成 AOE 版（AOE 成立时按 5 米内最近的一只判射程）。
		return PldHelper.IsReadyAndInRangeForNextGcdAoe(actionId);
	}
}
