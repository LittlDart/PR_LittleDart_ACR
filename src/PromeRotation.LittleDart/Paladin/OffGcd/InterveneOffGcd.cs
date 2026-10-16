using System;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class InterveneOffGcd : PldResolverBase
{
	private const string LogTag = "[LittleDart-PLD/调停]";

	private const int DiagIntervalMs = 1000;

	private static long _lastDiagTime;

	protected override string ResolverDescription => "oGCD：调停（保留层数 / 安全距离 / 爆发内不保留）";

	private static void Diag(bool qtOn, bool delayFof, bool hasTarget, bool unlock, bool ready, float distanceToCircle, float keepDistance, bool passDistance, float rawCharges, int usableCharges, int keepCharges, bool burstExempt, bool passCharges, bool fofBuff, bool fofRecent, bool inBurst)
	{
		if (Diagnostics.Enabled)
		{
			long tickCount = Environment.TickCount64;
			if (tickCount - _lastDiagTime >= 1000)
			{
				_lastDiagTime = tickCount;
				bool passAll = (qtOn && !delayFof) & hasTarget & unlock & ready & passDistance & passCharges;
				string distanceText = (hasTarget ? $"{distanceToCircle:F1}" : "n/a");
				PldHelper.LogTagged($"{"[LittleDart-PLD/调停]"} pass={passAll} | qt={qtOn} 延后战逃={delayFof} | target={hasTarget} unlock={unlock} ready={ready} | 圈距={distanceText} 安全距离={keepDistance:F1} 距离闸={passDistance} | 原值={rawCharges:F2} 可用层={usableCharges} 保留={keepCharges} 层数闸={passCharges} | fofBuff={fofBuff} fofRecent={fofRecent} inBurst={inBurst} 爆发豁免={burstExempt}");
			}
		}
	}

	protected override CheckResult CheckReal()
	{
		if (PldHelper.IsDailyMode())
		{
			return new CheckResult(false, "日常模式不自动调停（本决策器高难专用）");
		}
		bool qt = PromeSettings.Instance.GetQt("自动调停");
		bool qt2 = PromeSettings.Instance.GetQt("延后战逃");
		bool hasValidTarget = PldHelper.HasValidTarget();
		bool interveneUnlocked = ActionHelper.IsUnlocked(16461u);
		float interveneKeepDistance = PldSettings.Instance.InterveneKeepDistance;
		float distanceToTargetCircle = PldHelper.GetDistanceToTargetCircle();
		bool passDistance = interveneKeepDistance <= 0f || distanceToTargetCircle <= interveneKeepDistance;
		float actionRange = default;
		bool passActionRange = !ActionHelper.TryGetActionRange(16461u, out actionRange) || actionRange <= 0f || distanceToTargetCircle <= actionRange;
		bool actionReady = (ActionHelper.CanCast(16461u, (IBattleChara)null) && !ActionHelper.RecentlyUsed(16461u, 500)) & passActionRange;
		int usableCharges = (int)MathF.Floor(ActionHelper.GetActionCharges(16461u));
		int interveneKeepCharges = PldSettings.Instance.InterveneKeepCharges;
		PldHelper.IsFofActive();
		PldHelper.IsFofRecentCast();
		bool inBurst = PldHelper.IsFofInside();
		bool burstExempt = (PldSettings.Instance.InterveneKeepInBurst & inBurst) || usableCharges > interveneKeepCharges;
		if (!qt)
		{
			return new CheckResult(false, "QT「自动调停」没开");
		}
		if (qt2)
		{
			return new CheckResult(false, "QT「延后战逃」开着，不自动调停");
		}
		if (!hasValidTarget)
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!interveneUnlocked)
		{
			return new CheckResult(false, "调停还没解锁");
		}
		if (!actionReady)
		{
			return new CheckResult(false, passActionRange ? "调停没冷却好 / 刚放过" : "离目标太远，调停够不着");
		}
		if (!passDistance)
		{
			return new CheckResult(false, $"到目标圈还剩 {distanceToTargetCircle:F1} 米，超过安全距离 {interveneKeepDistance:F1} 米");
		}
		if (!burstExempt)
		{
			return new CheckResult(false, $"可用层数 {usableCharges} 不够（要保留 {interveneKeepCharges} 层）");
		}
		return new CheckResult(true, "安全距离与保留层数都通过，交调停");
	}

	public override PAction GetAction()
	{
		DiagNow();
		return PldAction.Of(16461u, (ActionType)1);
	}

	private static void DiagNow()
	{
		bool qt = PromeSettings.Instance.GetQt("自动调停");
		bool qt2 = PromeSettings.Instance.GetQt("延后战逃");
		bool hasTarget = PldHelper.HasValidTarget();
		bool unlock = ActionHelper.IsUnlocked(16461u);
		float interveneKeepDistance = PldSettings.Instance.InterveneKeepDistance;
		float distanceToTargetCircle = PldHelper.GetDistanceToTargetCircle();
		bool passDistance = interveneKeepDistance <= 0f || distanceToTargetCircle <= interveneKeepDistance;
		float actionRange = default;
		bool passActionRange = !ActionHelper.TryGetActionRange(16461u, out actionRange) || actionRange <= 0f || distanceToTargetCircle <= actionRange;
		bool ready = (ActionHelper.CanCast(16461u, (IBattleChara)null) && !ActionHelper.RecentlyUsed(16461u, 500)) & passActionRange;
		float actionCharges = ActionHelper.GetActionCharges(16461u);
		int usableCharges = (int)MathF.Floor(actionCharges);
		int interveneKeepCharges = PldSettings.Instance.InterveneKeepCharges;
		bool fofBuff = PldHelper.IsFofActive();
		bool fofRecent = PldHelper.IsFofRecentCast();
		bool inBurst = PldHelper.IsFofInside();
		bool burstExempt = PldSettings.Instance.InterveneKeepInBurst & inBurst;
		bool passCharges = burstExempt || usableCharges > interveneKeepCharges;
		Diag(qt, qt2, hasTarget, unlock, ready, distanceToTargetCircle, interveneKeepDistance, passDistance, actionCharges, usableCharges, interveneKeepCharges, burstExempt, passCharges, fofBuff, fofRecent, inBurst);
	}
}
