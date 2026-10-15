using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// 100 级（含高难轴）·无情窗口内的「通用爆发击出口」。
///
/// <para>
/// 背景：100 级爆发击此前只有 <c>GnbWindowBurstStrikeGcd</c>（&lt;100 级）、
/// <c>GnbPotionBurstStrikeGcd</c>（药效）、<c>GnbOvercapBurstStrikeGcd</c>（腾位）
/// 三条条件式档。于是「无情内手里还有晶壤、但倍攻差 1 发 / 烈牙在 CD / 没音速破 /
/// 没血壤连」这一格会一路落到兜底的 <c>GnbBaseComboGcd</c>（基础三连），
/// 把晶壤留到无情窗口外。
/// </para>
///
/// <para>
/// 玩家口径（2026-10-14 追加）：<b>哪怕是高难的轴，无情里爆发击的优先级也要高过基础连。</b>
/// 所以本档注册在 <c>GnbBaseComboGcd</c> 之前；它排在所有功能技之后，
/// 只接「其它无情技都不成立」的那一格，不会抢功能技的格子。
/// </para>
/// </summary>
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
