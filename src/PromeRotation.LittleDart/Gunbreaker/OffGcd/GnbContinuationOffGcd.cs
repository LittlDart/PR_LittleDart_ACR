using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbContinuationOffGcd : GnbResolverBase
{
	private const double ContinuationSafetyMarginSeconds = 0.3;

	protected override string ResolverDescription => "oGCD：续剑（追加技）";

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16155u))
		{
			return new CheckResult(false, "续剑还没解锁（70 级）");
		}
		if (ActionHelper.GetAdjustedActionId(16155u) == 16155)
		{
			return new CheckResult(false, "当前没有可用的续剑形态");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		if (GnbHelper.IsNoMercyInside() && PromeSettings.Instance.GetQt("续剑进无情"))
		{
			return new CheckResult(true, "续剑进无情：窗口内不后置，直接打");
		}
		if (!PromeSettings.Instance.GetQt("续剑后置"))
		{
			return new CheckResult(true, "续剑后置关着：能打就打");
		}
		if (GnbHelper.IsLastNoMercyGcd())
		{
			return new CheckResult(true, "无情最后一个 GCD：续剑抢在前面");
		}
		if (GnbHelper.IsInGcdSecondHalf() || IsContinuationPreparedExpiring())
		{
			return new CheckResult(true, "GCD 后半窗口：插续剑");
		}
		return new CheckResult(false, "续剑后置：等 GCD 后半窗口");
	}

	public override PAction GetAction()
	{
		return ActionHelper.GetAdjustedActionId(16155u) switch
		{
			16156u => GnbAction.Of(16156u, (ActionType)1), 
			16157u => GnbAction.Of(16157u, (ActionType)1), 
			16158u => GnbAction.Of(16158u, (ActionType)1), 
			36936u => GnbAction.Of(36936u, (ActionType)1), 
			_ => GnbAction.Of(25759u, (ActionType)1), 
		};
	}

	private static bool IsContinuationPreparedExpiring()
	{
		if (!IsExpiring(1842u) && !IsExpiring(1843u) && !IsExpiring(1844u) && !IsExpiring(2686u))
		{
			return IsExpiring(3839u);
		}
		return true;
	}

	private static bool IsExpiring(uint statusId)
	{
		if (!GnbHelper.HasAura(statusId))
		{
			return false;
		}
		double auraTimeLeft = GnbHelper.GetAuraTimeLeft(statusId);
		if (auraTimeLeft <= 0.0)
		{
			return false;
		}
		return auraTimeLeft <= TimeUntilGcdSecondHalf() + 0.3;
	}

	private static double TimeUntilGcdSecondHalf()
	{
		float gcdTotal = ActionHelper.GetGcdTotal();
		if (gcdTotal <= 0f)
		{
			return 0.0;
		}
		float gcdElapsed = ActionHelper.GetGcdElapsed();
		if (gcdElapsed <= 0f)
		{
			return 0.0;
		}
		double remainMs = (double)gcdTotal / 2.0 - (double)gcdElapsed;
		if (!(remainMs > 0.0))
		{
			return 0.0;
		}
		return remainMs;
	}
}
