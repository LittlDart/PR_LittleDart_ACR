using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Paladin.Hotkey;

public static class PldHotkeys
{
	public static readonly string[] MainPanelNames = new string[15]
	{
		"圣盾阵", "铁壁", "壁垒", "极致防御", "亲疏自行", "挑衅", "退避", "调停", "圣灵", "血仇",
		"武装戍卫", "冲刺", "神圣领域", "LB", "清扫队列"
	};

	public static ActionTargetType TargetTypeOf(IBattleChara? target)
	{
		if (target != null)
		{
			IBattleChara me = Core.Me;
			if (me != null && ((IGameObject)target).EntityId == ((IGameObject)me).EntityId)
			{
				return (ActionTargetType)0;
			}
			List<IBattleChara> party = PartyHelper.GetParty();
			for (int i = 1; i < party.Count && i < 8; i++)
			{
				if (party[i] != null && ((IGameObject)party[i]).EntityId == ((IGameObject)target).EntityId)
				{
					return (ActionTargetType)(6 + (i - 1));
				}
			}
			return (ActionTargetType)1;
		}
		return (ActionTargetType)1;
	}

	public static PAction OnTarget(uint actionId, ActionType type, IBattleChara target)
	{
		return new PAction(actionId, type, TargetTypeOf(target));
	}

	public static bool IsMainPanelName(string name)
	{
		return Array.IndexOf(MainPanelNames, name) >= 0;
	}
}
