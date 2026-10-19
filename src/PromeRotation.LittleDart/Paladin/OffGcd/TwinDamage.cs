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

	/// <summary>
	/// 【AOE 轴专用】<see cref="ReadyWithCanCast"/> 的**目标无关**版（★ 2026-10-16）：
	/// AOE 成立时「在射程内」按「<c>PldHelper.GetCastRadius</c> 内最近的一只」算（不看当前选定目标），
	/// 否则与老函数逐字等价（只放宽、不收紧）。
	///
	/// <para>用法：厄运流转 23（自身中心 AOE ⇒ 半径 5 米）与偿赎剑 25747
	/// （Range = -1 ⇒ 半径 = 近战 3 米，且出手目标由 <c>PldAction.Aoe</c> 钉到最近那一只）。</para>
	/// </summary>
	internal static bool ReadyWithCanCastAoe(uint actionId)
	{
		if (ActionHelper.IsUnlocked(actionId) && PldHelper.IsReadyWithCanCastAoe(actionId))
		{
			return !ActionHelper.RecentlyUsed(actionId, 500);
		}
		return false;
	}

	/// <summary>
	/// 安魂祈祷 7383 / 绝对统治 36921 现在能不能交（两者强绑定）。
	///
	/// <para>★ 2026-10-16：两个 ready 判据都换成 AOE 版 —— 这两个技能**必须选一只敌人**
	/// （7383 的 Action 表 Range = -1、36921 是 25 米），原来那一版读的是「当前选定目标在不在射程内」
	/// ⇒ 玩家把目标清掉后，身边 5 米挤着 3 只怪也不会开爆发窗口（与绝枪那边的血壤同一个毛病）。
	/// AOE 不成立时 AOE 版与老函数逐字等价，所以单体轴一行没变。</para>
	/// </summary>
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
		// ★ 2026-10-16 玩家口径：AOE 成立 ⇒ 不看当前选定的目标，只要「眼前有能打的怪」；
		//   否则 = 老的 HasValidTarget + IsInMeleeRange（逐字等价）。
		//   厄运流转 23 是自身中心 AOE（Range = 0 / EffectRange = 5）⇒ 这一条把它从
		//   「玩家必须先选好目标」里解放出来；偿赎剑 25747 是 Range = -1（要选敌人）
		//   ⇒ 它自己那一格还有一道「挑得出最近的一只」的闸门（见 ExpiacionOffGcd）。
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
