using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbPreNoMercyBurstStrikeGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：爆发击（100 级以下 · 无情前那一格）";

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16162u))
		{
			return new CheckResult(false, "爆发击还没解锁（30 级）");
		}
		if (GnbHelper.IsLevelAtLeast100())
		{
			return new CheckResult(false, "100 级及以上走高难那一套（本条不参与）");
		}
		if (GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, "群体连招中：爆发击（单体）不打，交给群体轴");
		}
		if (GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "在无情窗口内（交给窗口内那一档）");
		}
		if (!GnbHelper.IsNoMercyReady())
		{
			return new CheckResult(false, $"无情还没转好（{GnbHelper.GetNoMercyCooldownSeconds():F1}s）—— 这一格是「无情的前一个 gcd」");
		}
		if (GnbHelper.GetAmmo() <= 0)
		{
			return new CheckResult(false, "没有晶壤");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"无情前那一格：倒一发爆发击（晶壤 {GnbHelper.GetAmmo()}）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16162u, (ActionType)0);
	}
}
