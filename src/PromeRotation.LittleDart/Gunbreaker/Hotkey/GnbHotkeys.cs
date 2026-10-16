using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 绝枪热键面板的「名字表 + 目标翻译」。
///
/// <para><b>名字表的作用</b>：<see cref="MainPanelNames"/> 是 <c>/LittleDart-GNB hk</c> 与
/// <c>/LittleDart-GNB help</c> 的提示文字来源（面板本身在 <see cref="GnbHotkeyPanels"/> 里建）。
/// 两份顺序一旦不一致，提示与实际按钮就会对不上 —— 改顺序请**同时**改两处。</para>
///
/// <para><b>目标翻译</b>：面板上大多数按钮是「固定目标」（自己 / 当前敌方），
/// 但「退避」这类技能必须点名**另一个坦克**，而 PR 的 <see cref="PAction"/> 只认
/// <see cref="ActionTargetType"/> 枚举（没有「实体」这一档）⇒ 只能把队伍里的实体位置
/// 翻译回 <c>PartyMember2..8</c>（写法与骑士 <c>PldHotkeys.TargetTypeOf</c> 逐字同构）。</para>
/// </summary>
public static class GnbHotkeys
{
	/// <summary>
	/// 主面板 15 个按钮的注册顺序（面板按**行优先**填充，5 列 ⇒ 3 行）。
	/// ★ 2026-10-15 按玩家指定的新顺序建表：
	/// 刚玉 → 铁壁 → 伪装 → 大星云 → 亲疏自行 → 挑衅 → 退避 → 弹道 → 闪雷弹 → 血仇 →
	/// 极光 → 冲刺 → 超火流星 → LB → 清扫队列。
	/// </summary>
	public static readonly string[] MainPanelNames = new string[15]
	{
		"刚玉", "铁壁", "伪装", "大星云", "亲疏自行", "挑衅", "退避", "弹道", "闪雷弹", "血仇",
		"极光", "冲刺", "超火流星", "LB", "清扫队列"
	};

	/// <summary>
	/// 支援面板（**六宫格**）6 个按钮的注册顺序。
	/// ★ 面板 = **2 列 × 3 行**、**刚玉在前极光在后**（行优先填充）：
	/// <code>
	///   刚玉搭档        极光搭档
	///   刚玉奶妈        极光奶妈
	///   刚玉血量最低    极光血量最低
	/// </code>
	/// 命名规范与骑士「九宫格支援」一致（<c>&lt;技能&gt;&lt;目标&gt;</c>，如骑士的「干预搭档」）。
	/// 与 <see cref="GnbHotkeyPanels"/> 的 <c>BuildSupportPanel</c> 必须**逐字一致**（老坑，见 docs\25 第 2 节）。
	/// </summary>
	public static readonly string[] SupportPanelNames = new string[6]
	{
		"刚玉搭档", "极光搭档", "刚玉奶妈", "极光奶妈", "刚玉血量最低", "极光血量最低"
	};

	/// <summary>
	/// 把一个**队伍里的实体**翻译成 PR 的目标枚举：自己 = <c>Self</c>、
	/// 队友 = <c>PartyMember2..8</c>（列表第 0 位是自己，第 1 位 = PartyMember2）、
	/// 其余（敌人 / 不在队伍里）= <c>Target</c>。
	/// </summary>
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
					// PartyMember2 = 6，依次 +1（与 SDK 枚举的定义顺序一致）。
					return (ActionTargetType)(6 + (i - 1));
				}
			}
			return ActionTargetType.Target;
		}
		return ActionTargetType.Target;
	}

	/// <summary>按「技能 + 实体目标」造一个 <see cref="PAction"/>（目标枚举由 <see cref="TargetTypeOf"/> 推出来）。</summary>
	public static PAction OnTarget(uint actionId, ActionType type, IBattleChara target)
	{
		return new PAction(actionId, type, TargetTypeOf(target));
	}

	/// <summary>这个名字是不是主面板上的按钮名。</summary>
	public static bool IsMainPanelName(string name)
	{
		return Array.IndexOf(MainPanelNames, name) >= 0;
	}

	/// <summary>这个名字是不是支援面板（六宫格）上的按钮名。</summary>
	public static bool IsSupportPanelName(string name)
	{
		return Array.IndexOf(SupportPanelNames, name) >= 0;
	}

	/// <summary>两个面板的全部按钮名（主面板在前、支援面板在后）—— <c>help</c> 与兜底提示用。</summary>
	public static IReadOnlyList<string> AllPanelNames()
	{
		List<string> all = new List<string>(MainPanelNames.Length + SupportPanelNames.Length);
		all.AddRange(MainPanelNames);
		all.AddRange(SupportPanelNames);
		return all;
	}
}
