using System;
using System.Collections.Concurrent;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common;
using PromeRotation.Core;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin;

public static class Diagnostics
{
	private static readonly ConcurrentDictionary<string, long> LastHitLogTicks = new ConcurrentDictionary<string, long>();

	private const long HitLogThrottleMs = 1000L;

	private static long _lastComboLogTime;

	public static bool Enabled { get; set; }

	public static void OnHit(string resolver, string message)
	{
		if (Enabled && ShouldLogHit(resolver))
		{
			if (resolver.Contains("王权剑") || resolver.Contains("暴乱剑") || resolver.Contains("圣灵") || resolver.Contains("先锋剑") || resolver.Contains("赎罪剑") || resolver.Contains("葬送剑") || resolver.Contains("祈告剑"))
			{
				PldHelper.LogTagged($"[LittleDart-PLD/Hit] {resolver} → {message} | combo={PldHelper.GetLastComboSpellId()} comboLeft={PldHelper.GetComboTimeLeftSeconds():F2}s divineMight={PldHelper.HasDivineMight()} atonementReady={PldHelper.HasAura(PldSkills.赎罪剑预备)} " + FofFields() + " " + TimeFields());
			}
			else
			{
				PldHelper.LogTagged($"[LittleDart-PLD/Hit] {resolver} → {message} | {FofFields()} {TimeFields()}");
			}
		}
	}

	private static string TimeFields()
	{
		return $"gcdRemain={AbilityGate.Current.GcdRemain:F3} scope={AbilityGate.InDecisionScope}";
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

	private static string FofFields()
	{
		return $"fofBuff={PldHelper.IsFofActive()} fofRecent={PldHelper.IsFofRecentCast()} fofInside={PldHelper.IsFofInside()}";
	}

	public static void TickComboSnapshot()
	{
		if (Enabled)
		{
			long tickCount = Environment.TickCount64;
			if (tickCount - _lastComboLogTime >= 500)
			{
				_lastComboLogTime = tickCount;
				PldHelper.LogTagged(
					$"[LittleDart-PLD/Combo] 等级={(byte)((Core.Me != null) ? ((ICharacter)Core.Me).Level : 0)} "
					+ $"combo={PldHelper.GetLastComboSpellId()} "
					+ $"left={PldHelper.GetComboTimeLeftSeconds():F2}s "
					+ $"divineMight={PldHelper.HasDivineMight()} "
					+ FofFields() + " | " + ChainBuffFields());
			}
		}
	}

	private static string Remain(uint auraId)
	{
		if (!PldHelper.HasAura(auraId))
		{
			return "无";
		}
		if (PldHelper.HasAura(auraId, 25000))
		{
			return "≥25s";
		}
		if (PldHelper.HasAura(auraId, 15000))
		{
			return "≥15s";
		}
		if (PldHelper.HasAura(auraId, 5000))
		{
			return "≥5s";
		}
		return "<5s";
	}

	private static string ChainBuffFields()
	{
		return $"安魂={Remain(PldSkills.安魂祈祷)} 安魂层={PldHelper.GetAuraStack(PldSkills.安魂祈祷)} 沥血预备={Remain(PldSkills.沥血剑预备)} 赎罪预备={Remain(PldSkills.赎罪剑预备)} 祈告预备={Remain(PldSkills.祈告剑预备)} 葬送预备={Remain(PldSkills.葬送剑预备)}";
	}
}
