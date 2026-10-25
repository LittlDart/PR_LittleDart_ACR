using System;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class InterveneOffGcd : PldResolverBase
{
	private const string LogTag = "[LittleDart-PLD/调停]";

	private const int DiagIntervalMs = 1000;

	
	
	private const int RecentlyUsedWindowMs = 500;

	
	
	private const float ChargesFloorEpsilon = 0.001f;

	
	
	private const int InterveneMaxCharges = 2;

	private static long _lastDiagTime;

	protected override string ResolverDescription => "oGCD：调停（爆发内不保留 / 充能层数 / 安全距离）";

	
	
	
	
	private sealed class Reading
	{
		
		public bool DailyMode;
		public bool Qt;
		public bool DelayFof;
		public bool HasTarget;
		public bool Unlocked;
		public bool Occupied;
		public bool InRange;
		public bool DistanceOk;
		public bool RecentlyUsed;

		
		public float Charges;
		public int UsableCharges;
		public int MaxCharges;
		public int ConfiguredKeep;
		public bool FofActive;
		public bool KeepInBurst;

		
		public float Distance;
		public float KeepDistance;
		public float ActionRange;

		
		public bool HasCharge => Charges >= 1f;

		
		public bool NoKeepInBurst => KeepInBurst & FofActive;

		
		public int EffectiveKeep => NoKeepInBurst ? 0 : ConfiguredKeep;

		
		public bool PassCharges => UsableCharges > EffectiveKeep;

		public bool PassAll => !DailyMode && Qt && !DelayFof && HasTarget && Unlocked && !Occupied && !RecentlyUsed && InRange && DistanceOk && HasCharge && PassCharges;

		public string SuccessMessage => NoKeepInBurst ? "爆发内不保留：有层就放" : "安全距离与保留层数都通过，交调停";

		public string Message
		{
			get
			{
				if (DailyMode)
				{
					return "日常模式不自动调停（本决策器高难专用）";
				}
				if (!Qt)
				{
					return "QT「自动调停」没开";
				}
				if (DelayFof)
				{
					return "QT「延后战逃」开着，不自动调停";
				}
				if (!HasTarget)
				{
					return "没有有效目标";
				}
				if (!Unlocked)
				{
					return "调停还没解锁";
				}
				if (Occupied)
				{
					return "玩家被占用（过场 / 载入 / 骑乘 / 水中 / 眩晕等），这帧不交";
				}
				if (RecentlyUsed)
				{
					return $"刚放过调停（{RecentlyUsedWindowMs}ms 内）";
				}
				if (!InRange)
				{
					return $"离目标 {Distance:F1} 米，超出调停射程 {ActionRange:F1} 米";
				}
				if (!DistanceOk)
				{
					return $"到目标圈还剩 {Distance:F1} 米，超过安全距离 {KeepDistance:F1} 米";
				}
				if (!HasCharge)
				{
					return $"调停 0 层可用（{MaxCharges} 层充能中）";
				}
				return $"可用层数 {UsableCharges} 不够（保留 {EffectiveKeep} 层；爆发内不保留={(NoKeepInBurst ? "生效" : "未生效")}）";
			}
		}
	}

	
	
	
	
	private static Reading Read()
	{
		PldSettings settings = PldSettings.Instance;
		Reading reading = new Reading
		{
			DailyMode = PldHelper.IsDailyMode(),
			Qt = PromeSettings.Instance.GetQt(PldQt.UseIntervene),
			DelayFof = PromeSettings.Instance.GetQt(PldQt.DelayFof),
			HasTarget = PldHelper.HasValidTarget(),
			Unlocked = ActionHelper.IsUnlocked(PldSkills.调停),
			
			
			
			Occupied = GameData.IsPlayerOccupied(),
			Charges = ActionHelper.GetActionCharges(PldSkills.调停),
			MaxCharges = Math.Max(1, ActionHelper.GetMaxCharges(PldSkills.调停)),
			ConfiguredKeep = settings.InterveneKeepCharges,
			KeepDistance = settings.InterveneKeepDistance,
			RecentlyUsed = ActionHelper.RecentlyUsed(PldSkills.调停, RecentlyUsedWindowMs),
			FofActive = PldHelper.IsFofInside(),
			KeepInBurst = settings.InterveneKeepInBurst
		};
		reading.UsableCharges = (int)MathF.Floor(reading.Charges + ChargesFloorEpsilon);
		
		
		
		reading.ConfiguredKeep = Math.Clamp(reading.ConfiguredKeep, 0, InterveneMaxCharges);
		reading.Distance = PldHelper.GetDistanceToTargetCircle();
		reading.ActionRange = default;
		bool rangeKnown = ActionHelper.TryGetActionRange(PldSkills.调停, out reading.ActionRange) && reading.ActionRange > 0f;
		reading.InRange = !rangeKnown || reading.Distance <= reading.ActionRange;
		reading.DistanceOk = reading.KeepDistance <= 0f || reading.Distance <= reading.KeepDistance;
		return reading;
	}

	protected override CheckResult CheckReal()
	{
		Reading reading = Read();
		return new CheckResult(reading.PassAll, reading.PassAll ? reading.SuccessMessage : reading.Message);
	}

	public override PAction GetAction()
	{
		Diag(Read());
		return PldAction.Of(PldSkills.调停, ActionType.OffGcd);
	}

	
	private static void Diag(Reading reading)
	{
		if (!Diagnostics.Enabled)
		{
			return;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount - _lastDiagTime < DiagIntervalMs)
		{
			return;
		}
		_lastDiagTime = tickCount;
		string distanceText = reading.HasTarget ? $"{reading.Distance:F1}" : "n/a";
		PldHelper.LogTagged($"{LogTag} pass={reading.PassAll} | qt={reading.Qt} 延后战逃={reading.DelayFof} 日常={reading.DailyMode} | target={reading.HasTarget} unlock={reading.Unlocked} 占用={reading.Occupied} 刚放过={reading.RecentlyUsed} | 圈距={distanceText} 安全距离={reading.KeepDistance:F1} 距离闸={reading.DistanceOk} | 充能原值={reading.Charges:F2}/{reading.MaxCharges} 可用层={reading.UsableCharges} 保留={reading.ConfiguredKeep} 生效保留={reading.EffectiveKeep} 层数闸={reading.PassCharges} | fof={reading.FofActive} 爆发内不保留={reading.KeepInBurst} 生效={reading.NoKeepInBurst} | 结论={reading.Message}");
	}
}
