using System;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Paladin.OffGcd;

internal static class TwinDamage
{
	private static long _lastDiagTime;

	internal static bool ReadyWithCanCast(uint actionId)
	{
		if (ActionHelper.IsUnlocked(actionId) && PldHelper.IsReadyWithCanCast(actionId))
		{
			return !ActionHelper.RecentlyUsed(actionId, 500);
		}
		return false;
	}

	internal static bool IsRequiescatCastable()
	{
		if (!PromeSettings.Instance.GetQt("绝对统治"))
		{
			return false;
		}
		if (!PldHelper.IsFofActive())
		{
			return false;
		}
		if (ReadyWithCanCast(36921u))
		{
			return true;
		}
		return ReadyWithCanCast(7383u);
	}

	public static bool ShouldUse()
	{
		if (!PldHelper.HasValidTarget())
		{
			return false;
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return false;
		}
		if (PromeSettings.Instance.GetQt("双伤直出"))
		{
			return true;
		}
		double fofCooldownSeconds = PldHelper.GetFofCooldownSeconds();
		if (fofCooldownSeconds < (double)PldSettings.Instance.TwinDamageFofLockMin || fofCooldownSeconds > (double)PldSettings.Instance.TwinDamageFofLockMax)
		{
			return false;
		}
		if (ActionHelper.IsUnlocked(36921u) || ActionHelper.IsUnlocked(7383u))
		{
			double burstStarterCooldownSeconds = PldHelper.GetBurstStarterCooldownSeconds();
			if (burstStarterCooldownSeconds < (double)PldSettings.Instance.TwinDamageReqLockMin || burstStarterCooldownSeconds > (double)PldSettings.Instance.TwinDamageReqLockMax)
			{
				return false;
			}
		}
		return true;
	}

	internal static void Diag(bool shouldUse, bool scornUnlock, bool scornReady, bool expiUnlock, bool expiReady)
	{
		if (Diagnostics.Enabled)
		{
			long tickCount = Environment.TickCount64;
			if (tickCount - _lastDiagTime >= 1000)
			{
				_lastDiagTime = tickCount;
				double fofCooldownSeconds = PldHelper.GetFofCooldownSeconds();
				bool fofLockSatisfied = fofCooldownSeconds >= (double)PldSettings.Instance.TwinDamageFofLockMin && fofCooldownSeconds <= (double)PldSettings.Instance.TwinDamageFofLockMax;
				bool burstStarterUnlocked = ActionHelper.IsUnlocked(36921u) || ActionHelper.IsUnlocked(7383u);
				double burstStarterCooldownSeconds = PldHelper.GetBurstStarterCooldownSeconds();
				bool reqLockSatisfied = !burstStarterUnlocked || (burstStarterCooldownSeconds >= (double)PldSettings.Instance.TwinDamageReqLockMin && burstStarterCooldownSeconds <= (double)PldSettings.Instance.TwinDamageReqLockMax);
				PldHelper.LogTagged($"[LittleDart-PLD/双伤] shouldUse={shouldUse} | 锁1(fofCd={fofCooldownSeconds:F1}s)={fofLockSatisfied} 锁2(reqCd={burstStarterCooldownSeconds:F1}s unlocked={burstStarterUnlocked})={reqLockSatisfied} | 厄运流转 unlock={scornUnlock} ready={scornReady} | 偿赎剑 unlock={expiUnlock} ready={expiReady} | 起手技 ready={ReadyWithCanCast(PldHelper.GetBurstStarterSkill())} | fof={PldHelper.IsFofActive()} 安魂buff={PldHelper.HasAura(1368u)} 近战={PldHelper.IsInMeleeRange()}");
			}
		}
	}
}
