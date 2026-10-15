using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbLionHeartGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：狮心连（崛起之心/支配之心/终结之心）";

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16164u))
		{
			return new CheckResult(false, "血壤还没解锁（76 级）");
		}
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(16164u);
		if (adjustedActionId == 16164)
		{
			return new CheckResult(false, "血壤没有被替换成狮心连");
		}
		if (GnbHelper.IsLionHeartHeld())
		{
			return new CheckResult(false, $"压着等无情窗口（心有灵狮还剩 {GnbHelper.GetAuraTimeLeft(3840u):F1}s，无情 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
		}
		if (!PromeSettings.Instance.GetQt("先打血壤连") && !GnbHelper.ShouldUseAoeCombo() && GnbHelper.IsNoMercyInside() && GnbHelper.IsGnashingAvailable() && GnbHelper.GetAuraTimeLeft(3840u) > 4.0 * (double)GnbHelper.GetGcdSeconds())
		{
			return new CheckResult(false, "「先打血壤连」关着：窗口里让位给子弹连");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"狮心连（adjusted={adjustedActionId}）");
	}

	public override PAction GetAction()
	{
		return ActionHelper.GetAdjustedActionId(16164u) switch
		{
			36938u => GnbAction.Of(36938u, (ActionType)0), 
			36939u => GnbAction.Of(36939u, (ActionType)0), 
			_ => GnbAction.Of(36937u, (ActionType)0), 
		};
	}
}
