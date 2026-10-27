using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Paladin.Hotkey;

public static class PldHotkeys
{
	// ★ 顺序必须与 `PldHotkeyPanels.BuildMainPanel` 里 AddHotkey 的调用顺序**逐字一致**
	//   （2026-10-15 按玩家指定的新顺序重排；这里只用于 `/LittleDart-PLD help` 的提示文字）。
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
				return ActionTargetType.Self;
			}
			List<IBattleChara> party = PartyHelper.GetParty();
			for (int i = 1; i < party.Count && i < 8; i++)
			{
				if (party[i] != null && ((IGameObject)party[i]).EntityId == ((IGameObject)target).EntityId)
				{
					// 队伍下标 1..7 → PartyMember2..PartyMember8（枚举从 6 开始连续）
					return (ActionTargetType)((int)ActionTargetType.PartyMember2 + (i - 1));
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
}
