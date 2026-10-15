using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbOvercapBurstStrikeGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：爆发击（腾位：迅连斩要溢出了）";

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
		if (GnbHelper.GetLastComboSpellId() != 16139)
		{
			return new CheckResult(false, "不在「残暴弹 → 迅连斩」这一格");
		}
		if (!ActionHelper.IsUnlocked(16145u))
		{
			return new CheckResult(false, "迅连斩还没解锁（26 级），这一格没有 +1 晶壤");
		}
		if (!GnbHelper.IsAmmoAtCap())
		{
			return new CheckResult(false, $"晶壤没到上限（{GnbHelper.GetAmmo()}/{GnbHelper.GetAmmoCap()}），迅连斩的 +1 不会浪费");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"晶壤已满（{GnbHelper.GetAmmo()}/{GnbHelper.GetAmmoCap()}）：先倒一发腾位置再打迅连斩");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16162u, (ActionType)0);
	}
}
