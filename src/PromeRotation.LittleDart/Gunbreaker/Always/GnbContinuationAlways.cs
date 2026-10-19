using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.Always;

public class GnbContinuationAlways : GnbResolverBase
{
	private const double ContinuationSafetyMarginSeconds = 0.3;

	private const float MinGcdRemainHardFloor = 0.35f;

	protected override string ResolverDescription => "Always：续剑（追加技 · 无情让位 + 显式落地抢救）";

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		
		
		if (!GnbHelper.HasContinuationReady())
		{
			return new CheckResult(false, "当前没有可触发的续剑形态");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		
		
		
		
		string? noMercyYieldReason = GnbHelper.GetContinuationNoMercyYieldReason();
		if (noMercyYieldReason != null)
		{
			return new CheckResult(false, noMercyYieldReason);
		}
		float gcdRemain = GnbHelper.GetGcdRemainSeconds();
		bool expiring = IsContinuationPreparedExpiring();
		
		if (gcdRemain <= 0.05f)
		{
			if (GnbHelper.IsInLandingRescueWindow())
			{
				return new CheckResult(true, "Boss 刚落地抢救窗口：在首格 GCD 前抢先打出遗留续剑");
			}
			return new CheckResult(false, "GCD 已就绪（非刚落地抢救窗口）：让位给 GCD 先出手，避免卡 GCD");
		}
		
		if (gcdRemain <= MinGcdRemainHardFloor)
		{
			return new CheckResult(false, $"GCD 剩余窗口不足（{gcdRemain:F2}s ≤ {MinGcdRemainHardFloor:F2}s），避免卡 GCD");
		}
		if (!GnbHelper.IsNoMercyActive() && !expiring && gcdRemain < GnbHelper.ContinuationMinWeaveGcdRemain)
		{
			return new CheckResult(false, $"无情外常规织入需 GCD 剩余 ≥ {GnbHelper.ContinuationMinWeaveGcdRemain:F2}s（当前 {gcdRemain:F2}s）");
		}
		
		if (GnbHelper.IsLastNoMercyGcd())
		{
			return new CheckResult(true, "无情最后一个 GCD：续剑抢在前面");
		}
		if (GnbHelper.IsNoMercyActive() && !GnbHelper.IsDailyMode())
		{
			return new CheckResult(true, "高难无情窗口内：不后置，直接插续剑");
		}
		if (!PromeSettings.Instance.GetQt(GnbQt.DelayContinuation))
		{
			return new CheckResult(true, "续剑后置关着：能打就打");
		}
		if (GnbHelper.IsInGcdSecondHalf() || expiring)
		{
			return new CheckResult(true, "GCD 后半窗口或预备即将过期：插续剑");
		}
		return new CheckResult(false, "续剑后置：等 GCD 后半窗口");
	}

	public override PAction GetAction()
	{
		uint actionId = GnbHelper.GetContinuationReadyActionId();
		if (actionId == 0)
		{
			
			
			actionId = ActionHelper.GetAdjustedActionId(GnbSkills.续剑);
		}
		return GnbAction.Of(actionId, ActionType.Always);
	}

	private static bool IsContinuationPreparedExpiring()
	{
		if (!IsExpiring(GnbSkills.撕喉预备) && !IsExpiring(GnbSkills.裂膛预备) && !IsExpiring(GnbSkills.穿目预备) && !IsExpiring(GnbSkills.超高速预备))
		{
			return IsExpiring(GnbSkills.命运之印预备);
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
		return auraTimeLeft <= TimeUntilGcdSecondHalf() + ContinuationSafetyMarginSeconds;
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
		double remainSeconds = (double)gcdTotal / 2.0 - (double)gcdElapsed;
		return (remainSeconds > 0.0) ? remainSeconds : 0.0;
	}
}
