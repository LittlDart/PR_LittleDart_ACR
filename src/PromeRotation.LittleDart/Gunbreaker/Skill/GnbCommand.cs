using System;
using System.Collections.Generic;
using LittleDart.Gunbreaker.Hotkey;
using LittleDart.Gunbreaker.Mitigation;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;

namespace LittleDart.Gunbreaker.Skill;

public static class GnbCommand
{
	public const string MainCommand = "/LittleDart-GNB";

	public static void OnCommand(string command, string args)
	{
		string[] commandParts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (commandParts.Length == 0)
		{
			PrintHelp();
			return;
		}
		switch (commandParts[0].ToLowerInvariant())
		{
		case "qt":
			if (commandParts.Length < 2)
			{
				GnbFeedback.Fail("用法：/LittleDart-GNB qt <开关名>");
			}
			else
			{
				ToggleQt(commandParts[1]);
			}
			break;
		case "mode":
			SetMode((commandParts.Length >= 2) ? commandParts[1] : null);
			break;
		case "hk":
			if (commandParts.Length < 2)
			{
				GnbFeedback.Fail("用法：/LittleDart-GNB hk <热键名>");
			}
			else
			{
				TriggerHotkey(commandParts[1]);
			}
			break;
		case "mit":
			GnbFeedback.Info(GnbMitigationScheduler.Instance.DescribeStatus());
			break;
		case "autopull":
			AutoPull((commandParts.Length >= 2) ? commandParts[1] : null, (commandParts.Length >= 3) ? commandParts[2] : null);
			break;
		case "clear":
			// ★ 2026-10-19「骑士同款」：与 PldCommand 一样走 Helper 那一个实现 ——
			//   清队列 + 日志 + 游戏内提示都在 GnbHelper.ClearHighPriorityQueues 里（热键点的是同一份）。
			GnbHelper.ClearHighPriorityQueues();
			GnbFeedback.Info("已清空技能队列");
			break;
		case "help":
			PrintHelp();
			break;
		default:
			GnbFeedback.Fail($"未知参数：{commandParts[0]}，输入 {"/LittleDart-GNB"} help 查看用法");
			break;
		}
	}

	public static void ToggleQt(string qtName)
	{
		if (!GnbQt.All.ContainsKey(qtName))
		{
			GnbFeedback.Fail($"QT 开关不存在：{qtName}，输入 {"/LittleDart-GNB"} help 查看可用开关");
		}
		else
		{
			bool qt = PromeSettings.Instance.GetQt(qtName);
			PromeSettings.Instance.SetQt(qtName, !qt);
			GnbFeedback.Info("已" + (qt ? "关闭" : "开启") + "：" + qtName);
		}
	}

	/// <summary>触发热键面板上的某个按钮（<c>/LittleDart-GNB hk &lt;名字&gt;</c>）。</summary>
	public static void TriggerHotkey(string hotkeyName)
	{
		if (!GnbHotkeyPanels.TryTrigger(hotkeyName))
		{
			GnbFeedback.Fail($"热键不存在或面板未注册：{hotkeyName}，输入 {"/LittleDart-GNB"} help 查看可用热键");
		}
	}

	public static void AutoPull(string? value, string? selectorArg)
	{
		if (string.Equals(value, "selector", StringComparison.OrdinalIgnoreCase))
		{
			if (string.IsNullOrWhiteSpace(selectorArg))
			{
				GnbFeedback.Info("可选的目标选择器模式：" + string.Join(" / ", GnbAutoPull.ModeLabels) + "（也可用枚举名，例如 Closest）");
				return;
			}
			if (!GnbAutoPull.TryParseSelectorMode(selectorArg, out var mode))
			{
				GnbFeedback.Fail("不认识的目标选择器模式：" + selectorArg);
				return;
			}
			GnbAutoPull.SetSelectorMode(mode);
			GnbFeedback.Info(GnbAutoPull.Describe());
		}
		else if ((value == "0" || value == "1") ? true : false)
		{
			GnbAutoPull.SetEnabled(value == "1");
			GnbFeedback.Info(GnbAutoPull.Describe());
		}
		else
		{
			GnbFeedback.Info(GnbAutoPull.Describe());
			GnbFeedback.Info("用法：/LittleDart-GNB autopull 0|1 | /LittleDart-GNB autopull selector <模式>");
		}
	}

	public static void SetMode(string? value)
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance == null)
		{
			GnbFeedback.Fail("设置未初始化");
		}
		else if ((value == "0" || value == "1") ? true : false)
		{
			instance.Mode = ((value == "1") ? 1 : 0);
			instance.Save();
			// ★ 2026-10-19「骑士同款切模式提示」：与 PldCommand.SetMode 同一条管道 ——
			//   ApplyModePreset 内部弹游戏内提示；它返回 false（玩家关了「切换模式时自动调整 QT」）时这里补一条。
			if (!GnbRotationEventHandler.ApplyModePreset())
			{
				GnbHelper.ToastModeBanner(instance.Mode == 1);
			}
			GnbFeedback.Info("已切换到" + ((instance.Mode == 1) ? "日常" : "高难") + "模式");
		}
		else
		{
			GnbFeedback.Info("当前模式：" + ((instance.Mode == 1) ? "日常" : "高难"));
			GnbFeedback.Info("用法：/LittleDart-GNB mode 0|1（0 = 高难，1 = 日常）");
		}
	}

	public static void PrintHelp()
	{
		GnbFeedback.Info("========== LittleDart 绝枪战士 ACR 指令 ==========");
		GnbFeedback.Info("/LittleDart-GNB qt <开关名>   —— 反转开关");
		GnbFeedback.Info("/LittleDart-GNB hk <热键名>   —— 触发某个热键（等同于点面板按钮）");
		GnbFeedback.Info("/LittleDart-GNB mode 0|1      —— 高难 / 日常");
		GnbFeedback.Info("/LittleDart-GNB mit            —— 打印减伤调度器状态（排查为什么不调度）");
		GnbFeedback.Info("/LittleDart-GNB autopull 0|1   —— 开关自动拉怪（目标选择器一起摆正）");
		GnbFeedback.Info("/LittleDart-GNB autopull selector <模式>  —— 单独设目标选择器模式");
		GnbFeedback.Info("/LittleDart-GNB clear          —— 清空技能队列");
		GnbFeedback.Info("/LittleDart-GNB help           —— 本帮助");
		GnbFeedback.Info("---- 可用开关 ----");
		foreach (string key in GnbQt.All.Keys)
		{
			GnbFeedback.Info($"  {key} = {PromeSettings.Instance.GetQt(key)}");
		}
		GnbFeedback.Info("---- 可用热键 ----");
		GnbFeedback.Info("  " + string.Join("、", AvailableHotkeyNames()));
	}

	/// <summary>面板上已注册的名字；面板还没注册（ACR 没加载完）时退回静态名字表（主面板 + 支援面板）。</summary>
	private static IReadOnlyList<string> AvailableHotkeyNames()
	{
		IReadOnlyList<string> registered = GnbHotkeyPanels.RegisteredNames();
		if (registered.Count <= 0)
		{
			return GnbHotkeys.AllPanelNames();
		}
		return registered;
	}
}
