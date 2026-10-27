using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Extensions;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.Always;

/// <summary>
/// Always：续剑（16155 → 撕喉/裂膛/穿目/超高速/命运之印）——
/// <list type="bullet">
/// <item>显式落地抢救驱动（P1-2）：当 <c>gcdRemain &lt;= 0.05s</c> 时，仅在 <see cref="GnbHelper.IsInLandingRescueWindow"/>（Boss 刚落地重获目标窗口）内放行抢救续剑；常规战斗 GCD 边界一律让位给 GCD 先出手，避免 Always 在 GCD 边界硬插吞 GCD；</item>
/// <item>无情优先让位：当《无情》可在本 GCD 窗口内放出（<see cref="GnbHelper.WillNoMercyBeReadyInCurrentGcd"/>）时，续剑主动让位给无情；</item>
/// <item>防吞无情增伤：刚按下《无情》1000ms 内但身上无情 Buff（1831）尚未生效时暂缓续剑；</item>
/// <item>保留高难无情内直放、日常模式遵守「续剑后置」（P3）及 5 种预备状态即将过期的抢救逻辑。</item>
/// </list>
/// </summary>
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
		// 「能不能打续剑」的唯一判定在 GnbHelper.GetContinuationReadyActionId（docs\18 S1）；
		// 血壤那一格也调同一份，用来给续剑让位。
		if (!GnbHelper.HasContinuationReady())
		{
			return new CheckResult(false, "当前没有可触发的续剑形态");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		// 1/2. 解除无情与续剑互锁：无情本 GCD 内可开时续剑让位；无情刚按下但 buff 尚未生效的
		//      1000ms 防吞窗口内暂缓续剑。★ 判定只此一处（docs\18 S1）：
		//      GnbHelper.GetContinuationNoMercyYieldReason —— 血壤的让位闸门调的是同一份
		//      （2026-10-19 方案 A 收窄血壤让位，见 docs\44）。
		string? noMercyYieldReason = GnbHelper.GetContinuationNoMercyYieldReason();
		if (noMercyYieldReason != null)
		{
			return new CheckResult(false, noMercyYieldReason);
		}
		float gcdRemain = GnbHelper.GetGcdRemainSeconds();
		bool expiring = IsContinuationPreparedExpiring();
		// 3. P1-2 修复：GCD 已转好（gcdRemain <= 0.05s）时，仅在「Boss 刚落地重获目标」抢救窗口内放行，避免平时 GCD 边界吞 GCD
		if (gcdRemain <= 0.05f)
		{
			if (GnbHelper.IsInLandingRescueWindow())
			{
				return new CheckResult(true, "Boss 刚落地抢救窗口：在首格 GCD 前抢先打出遗留续剑");
			}
			return new CheckResult(false, "GCD 已就绪（非刚落地抢救窗口）：让位给 GCD 先出手，避免卡 GCD");
		}
		// 4. GCD 正在转时，确保足够后摇窗口（紧急过期/无情内底线 0.35s，常规织入底线 0.60s）
		if (gcdRemain <= MinGcdRemainHardFloor)
		{
			return new CheckResult(false, $"GCD 剩余窗口不足（{gcdRemain:F2}s ≤ {MinGcdRemainHardFloor:F2}s），避免卡 GCD");
		}
		if (!GnbHelper.IsNoMercyActive() && !expiring && gcdRemain < GnbHelper.ContinuationMinWeaveGcdRemain)
		{
			return new CheckResult(false, $"无情外常规织入需 GCD 剩余 ≥ {GnbHelper.ContinuationMinWeaveGcdRemain:F2}s（当前 {gcdRemain:F2}s）");
		}
		// 5. P3 修复：高难模式无情窗口内直放；日常模式仍尊重「续剑后置」（尾格或过期除外）
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
			// Check 刚成功、同一调用栈内状态不会漂移；万一漂移就回退到「调整后 ID」，
			// 与改动前的行为逐字一致（绝不返回 0 号技能）。
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
