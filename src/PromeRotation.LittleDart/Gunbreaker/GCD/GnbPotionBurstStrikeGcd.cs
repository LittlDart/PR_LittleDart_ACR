using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbPotionBurstStrikeGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：爆发击（爆发药内卸晶壤）";

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.IsPotionAmmoDumpEnabled())
		{
			return new CheckResult(false, "QT「爆发药内卸晶壤」关着，或身上没有药效");
		}
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16162u))
		{
			return new CheckResult(false, "爆发击还没解锁（30 级）");
		}
		if (GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, "群体连招中：爆发击（单体）不打，交给群体轴");
		}
		if (GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "在无情窗口内（交给窗口内的技能）");
		}
		if (!GnbHelper.IsLevelAtLeast100())
		{
			return new CheckResult(false, "100 级以下：交给那两档爆发击（无情前 / 腾位）");
		}
		if (GnbHelper.IsNoMercyPrepWindow())
		{
			return new CheckResult(false, "已进入无情前的准备窗口：晶壤留给窗口里的倍攻");
		}
		if (GnbHelper.GetAmmo() <= 0)
		{
			return new CheckResult(false, "没有晶壤");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"药效内倒爆发击（晶壤 {GnbHelper.GetAmmo()}）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16162u, (ActionType)0);
	}
}
