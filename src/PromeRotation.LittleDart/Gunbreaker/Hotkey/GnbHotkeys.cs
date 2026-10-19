using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Gunbreaker.Hotkey;

public static class GnbHotkeys
{
	
	
	
	
	
	
	public static readonly string[] MainPanelNames = new string[15]
	{
		"刚玉", "铁壁", "伪装", "大星云", "亲疏自行", "挑衅", "退避", "弹道", "闪雷弹", "血仇",
		"极光", "冲刺", "超火流星", "LB", "清扫队列"
	};

	
	
	
	
	
	
	
	
	
	
	
	public static readonly string[] SupportPanelNames = new string[6]
	{
		"刚玉搭档", "极光搭档", "刚玉奶妈", "极光奶妈", "刚玉血量最低", "极光血量最低"
	};

	
	
	
	
	
	public static ActionTargetType TargetTypeOf(IBattleChara? target)
	{
		if (target != null)
		{
			IBattleChara me = Core.Me;
			if (me != null && target.EntityId == me.EntityId)
			{
				return ActionTargetType.Self;
			}
			List<IBattleChara> party = PartyHelper.GetParty();
			for (int i = 1; i < party.Count && i < 8; i++)
			{
				if (party[i] != null && party[i].EntityId == target.EntityId)
				{
					
					return (ActionTargetType)(6 + (i - 1));
				}
			}
			return ActionTargetType.Target;
		}
		return ActionTargetType.Target;
	}

	
	public static PAction OnTarget(uint actionId, ActionType type, IBattleChara target)
	{
		return new PAction(actionId, type, TargetTypeOf(target));
	}

	
	public static bool IsSupportPanelName(string name)
	{
		return Array.IndexOf(SupportPanelNames, name) >= 0;
	}

	
	public static IReadOnlyList<string> AllPanelNames()
	{
		List<string> all = new List<string>(MainPanelNames.Length + SupportPanelNames.Length);
		all.AddRange(MainPanelNames);
		all.AddRange(SupportPanelNames);
		return all;
	}
}
