using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbNoMercyBurstStrikeGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：爆发击（100 级 · 无情内通用）";

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
		if (!GnbHelper.IsLevelAtLeast100())
		{
			return new CheckResult(false, "100 级以下：交给那两档爆发击（无情前 / 无情内）");
		}
		if (GnbHelper.ShouldUseAoeCombo() || GnbHelper.ShouldUseAoe(GnbBaseComboGcd.CurrentAoeThreshold))
		{
			return new CheckResult(false, "多目标：让位给 AOE 连");
		}
		if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "不在无情窗口内");
		}
		if (GnbHelper.GetAmmo() <= 0)
		{
			return new CheckResult(false, "没有晶壤");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"无情内倒爆发击（晶壤 {GnbHelper.GetAmmo()}）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16162u, (ActionType)0);
	}
}
