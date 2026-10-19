using System;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;
using LittleDart.Paladin.Skill;

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

	
	
	
	
	
	
	
	
	internal static bool ReadyWithCanCastAoe(uint actionId)
	{
		if (ActionHelper.IsUnlocked(actionId) && PldHelper.IsReadyWithCanCastAoe(actionId))
		{
			return !ActionHelper.RecentlyUsed(actionId, 500);
		}
		return false;
	}

	
	
	
	
	
	
	
	
	internal static bool IsRequiescatCastable()
	{
		if (!PromeSettings.Instance.GetQt(PldQt.UseRequiescat))
		{
			return false;
		}
		if (!PldHelper.IsFofActive())
		{
			return false;
		}
		if (ReadyWithCanCastAoe(PldSkills.绝对统治))
		{
			return true;
		}
		return ReadyWithCanCastAoe(PldSkills.安魂祈祷技能);
	}

	public static bool ShouldUse()
	{
		
		
		
		
		
		if (!PldHelper.HasEnemyToFight())
		{
			return false;
		}
		if (PromeSettings.Instance.GetQt(PldQt.TwinDamageDirect))
		{
			return true;
		}
		double fofCooldownSeconds = PldHelper.GetFofCooldownSeconds();
		if (fofCooldownSeconds < (double)PldSettings.Instance.TwinDamageFofLockMin || fofCooldownSeconds > (double)PldSettings.Instance.TwinDamageFofLockMax)
		{
			return false;
		}
		if (ActionHelper.IsUnlocked(PldSkills.绝对统治) || ActionHelper.IsUnlocked(PldSkills.安魂祈祷技能))
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
				bool burstStarterUnlocked = ActionHelper.IsUnlocked(PldSkills.绝对统治) || ActionHelper.IsUnlocked(PldSkills.安魂祈祷技能);
				double burstStarterCooldownSeconds = PldHelper.GetBurstStarterCooldownSeconds();
				bool reqLockSatisfied = !burstStarterUnlocked || (burstStarterCooldownSeconds >= (double)PldSettings.Instance.TwinDamageReqLockMin && burstStarterCooldownSeconds <= (double)PldSettings.Instance.TwinDamageReqLockMax);
				PldHelper.LogTagged($"[LittleDart-PLD/双伤] shouldUse={shouldUse} | 锁1(fofCd={fofCooldownSeconds:F1}s)={fofLockSatisfied} 锁2(reqCd={burstStarterCooldownSeconds:F1}s unlocked={burstStarterUnlocked})={reqLockSatisfied} | 厄运流转 unlock={scornUnlock} ready={scornReady} | 偿赎剑 unlock={expiUnlock} ready={expiReady} | 起手技 ready={ReadyWithCanCast(PldHelper.GetBurstStarterSkill())} | fof={PldHelper.IsFofActive()} 安魂buff={PldHelper.HasAura(PldSkills.安魂祈祷)} 近战={PldHelper.IsInMeleeRange()}");
			}
		}
	}
}
