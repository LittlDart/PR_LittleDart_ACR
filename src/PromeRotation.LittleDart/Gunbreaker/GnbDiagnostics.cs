using System;
using System.Collections.Concurrent;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common;
using LittleDart.Gunbreaker.GCD;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;

namespace LittleDart.Gunbreaker;

public static class GnbDiagnostics
{
	private static readonly ConcurrentDictionary<string, long> LastHitLogTicks = new ConcurrentDictionary<string, long>();

	private const long HitLogThrottleMs = 1000L;

	private static long _lastComboLogTime;

	public static bool Enabled { get; set; }

	public static void OnHit(string resolver, string message)
	{
		if (Enabled && ShouldLogHit(resolver))
		{
			if (resolver.Contains("利刃斩") || resolver.Contains("残暴弹") || resolver.Contains("迅连斩") || resolver.Contains("烈牙") || resolver.Contains("爆发击") || resolver.Contains("倍攻") || resolver.Contains("狮心"))
			{
				GnbHelper.LogTagged($"[LittleDart-GNB/Hit] {resolver} → {message} | combo={GnbHelper.GetLastComboSpellId()} comboLeft={GnbHelper.GetComboTimeLeftSeconds():F2}s ammo={GnbHelper.GetAmmo()} step={GnbHelper.GetAmmoComboStep()} " + NoMercyFields() + " " + TimeFields());
			}
			else
			{
				GnbHelper.LogTagged($"[LittleDart-GNB/Hit] {resolver} → {message} | {NoMercyFields()} {TimeFields()}");
			}
		}
	}

	private static string TimeFields()
	{
		return $"gcdRemain={AbilityGate.Current.GcdRemain:F3} scope={AbilityGate.InDecisionScope}";
	}

	private static string GnashingFangCooldownField()
	{
		return $"烈牙CD={ActionHelper.GetActionCooldown(16146u):F2}";
	}

	private static bool ShouldLogHit(string resolver)
	{
		long tickCount = Environment.TickCount64;
		if (LastHitLogTicks.TryGetValue(resolver, out var value) && tickCount - value < 1000)
		{
			return false;
		}
		LastHitLogTicks[resolver] = tickCount;
		return true;
	}

	private static string NoMercyFields()
	{
		return $"nmBuff={GnbHelper.IsNoMercyActive()} nmRecent={GnbHelper.IsNoMercyRecentCast()} nmInside={GnbHelper.IsNoMercyInside()}";
	}

	private static string Remain(uint auraId)
	{
		if (!GnbHelper.HasAura(auraId))
		{
			return "无";
		}
		if (GnbHelper.HasAura(auraId, 25000))
		{
			return "≥25s";
		}
		if (GnbHelper.HasAura(auraId, 15000))
		{
			return "≥15s";
		}
		if (GnbHelper.HasAura(auraId, 5000))
		{
			return "≥5s";
		}
		return "<5s";
	}

	private static string ChainBuffFields()
	{
		return $"无情={Remain(1831u)} 血壤={Remain(5051u)} 音速破={Remain(3886u)}";
	}

	public static void TickComboSnapshot()
	{
		if (Enabled)
		{
			long tickCount = Environment.TickCount64;
			if (tickCount - _lastComboLogTime >= 500)
			{
				_lastComboLogTime = tickCount;
				GnbHelper.LogTagged(
					$"[LittleDart-GNB/Combo] 等级={(byte)((Core.Me != null) ? ((ICharacter)Core.Me).Level : 0)} "
					+ $"ammo={GnbHelper.GetAmmo()}/{GnbHelper.GetAmmoCap()} "
					+ $"step={GnbHelper.GetAmmoComboStep()} "
					+ $"烈牙充能={GnbHelper.GetGnashingFangCharges():F2} "
					+ GnashingFangCooldownField()
					+ " "
					+ $"combo={GnbHelper.GetLastComboSpellId()} "
					+ $"left={GnbHelper.GetComboTimeLeftSeconds():F2}s "
					+ NoMercyFields()
					+ $" nmPrep={GnbHelper.IsNoMercyPrepWindow()} "
					+ $"狮心压着={GnbHelper.IsLionHeartHeld()} "
					+ $"药效={GnbHelper.HasPotionBuff()} "
					+ $"起手已跑={PromeSettings.Instance.OpenerHasBeenExecuted} "
					+ $"轴={GnbHelper.HasTimelineRunning()} "
					+ $"起手队列={ActionQueueManager.HasActionsInGcdQueue()} "
					+ "相位="
					+ GnbPlanGcd.Engine.Status
					+ " "
					+ $"相位步={GnbPlanGcd.Engine.CurrentIndex}"
					+ " | "
					+ ChainBuffFields()
					+ " | "

					+ "形态=" + GnbHelper.GetTargetProfile()
					+ $"（{TargetClassifier.EngageRadius:F0}m 内 {GnbHelper.GetNearbyEnemyCount((int)TargetClassifier.EngageRadius)} 只"
					+ (GnbHelper.TryGetTargetTimeToDeath(out double ttk) ? $"，预计 {ttk:F1}s 死" : "，TTK 读不出")
					+ "）");
			}
		}
	}
}
