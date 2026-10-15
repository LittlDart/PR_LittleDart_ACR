using System;
using System.Collections.Generic;
using LittleDart.Paladin.Hotkey;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;

namespace LittleDart.Paladin.Skill;

public static class PldCommand
{
	public const string MainCommand = "/LittleDart-PLD";

	public static void OnCommand(string command, string args)
	{
		args = args.Trim();
		if (string.IsNullOrEmpty(args))
		{
			PrintHelp();
			return;
		}
		string[] commandParts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		switch (commandParts[0].ToLowerInvariant())
		{
		case "qt":
			if (commandParts.Length < 2)
			{
				PldFeedback.Fail("用法：/LittleDart-PLD qt <开关名>");
			}
			else
			{
				ToggleQt(commandParts[1]);
			}
			break;
		case "hk":
			if (commandParts.Length < 2)
			{
				PldFeedback.Fail("用法：/LittleDart-PLD hk <热键名>");
			}
			else
			{
				TriggerHotkey(commandParts[1]);
			}
			break;
		case "mode":
			SetMode((commandParts.Length >= 2) ? commandParts[1] : null);
			break;
		case "mit":
			PldFeedback.Info(MitigationScheduler.Instance.DescribeStatus());
			break;
		case "autopull":
			AutoPull((commandParts.Length >= 2) ? commandParts[1] : null, (commandParts.Length >= 3) ? commandParts[2] : null);
			break;
		case "clear":
			PldHelper.ClearHighPriorityQueues();
			break;
		case "help":
			PrintHelp();
			break;
		default:
			PldFeedback.Fail($"未知参数：{commandParts[0]}，输入 {"/LittleDart-PLD"} help 查看用法");
			break;
		}
	}

	public static void ToggleQt(string qtName)
	{
		if (!PldQt.All.ContainsKey(qtName))
		{
			PldFeedback.Fail($"QT 开关不存在：{qtName}，输入 {"/LittleDart-PLD"} help 查看可用开关");
		}
		else
		{
			bool qt = PromeSettings.Instance.GetQt(qtName);
			PromeSettings.Instance.SetQt(qtName, !qt);
			PldFeedback.Info("已" + (qt ? "关闭" : "开启") + "：" + qtName);
		}
	}

	public static void TriggerHotkey(string hotkeyName)
	{
		if (!PldHotkeyPanels.TryTrigger(hotkeyName))
		{
			PldFeedback.Fail($"热键不存在或面板未注册：{hotkeyName}，输入 {"/LittleDart-PLD"} help 查看可用热键");
		}
	}

	public static void AutoPull(string? value, string? selectorArg)
	{
		if (string.Equals(value, "selector", StringComparison.OrdinalIgnoreCase))
		{
			if (string.IsNullOrWhiteSpace(selectorArg))
			{
				PldFeedback.Info("可选的目标选择器模式：" + string.Join(" / ", PldAutoPull.ModeLabels) + "（也可用枚举名，例如 Closest）");
				return;
			}
			if (!PldAutoPull.TryParseSelectorMode(selectorArg, out var mode))
			{
				PldFeedback.Fail("不认识的目标选择器模式：" + selectorArg);
				return;
			}
			PldAutoPull.SetSelectorMode(mode);
			PldFeedback.Info(PldAutoPull.Describe());
		}
		else if ((value == "0" || value == "1") ? true : false)
		{
			PldAutoPull.SetEnabled(value == "1");
			PldFeedback.Info(PldAutoPull.Describe());
		}
		else
		{
			PldFeedback.Info(PldAutoPull.Describe());
			PldFeedback.Info("用法：/LittleDart-PLD autopull 0|1 | /LittleDart-PLD autopull selector <模式>");
		}
	}

	public static void SetMode(string? value)
	{
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			PldFeedback.Fail("设置未初始化");
		}
		else if ((value == "0" || value == "1") ? true : false)
		{
			instance.Mode = ((value == "1") ? 1 : 0);
			instance.Save();
			if (!PldRotationEventHandler.ApplyModePreset())
			{
				PldHelper.ToastModeBanner(instance.Mode == 1);
			}
			PldFeedback.Info("已切换到" + ((instance.Mode == 1) ? "日常" : "高难") + "模式");
		}
		else
		{
			PldFeedback.Info($"当前模式：{((instance.Mode == 1) ? "日常" : "高难")}（Mode={instance.Mode}）。用法：{"/LittleDart-PLD"} mode 0（高难）| {"/LittleDart-PLD"} mode 1（日常）");
		}
	}

	public static void PrintHelp()
	{
		PldFeedback.Info("========== LittleDart 骑士 ACR 指令 ==========");
		PldFeedback.Info("/LittleDart-PLD qt <开关名>    —— 切换某个 QT 开关");
		PldFeedback.Info("/LittleDart-PLD hk <热键名>  —— 触发某个热键（等同于点面板按钮）");
		PldFeedback.Info("/LittleDart-PLD mode <0|1>     —— 切换高难/日常模式（日常才启用自动减伤）");
		PldFeedback.Info("/LittleDart-PLD autopull 0|1   —— 开/关自动拉怪（PR 的 AutoPull + 目标选择器一起动）");
		PldFeedback.Info("/LittleDart-PLD autopull selector <模式> —— 单独设目标选择器模式");
		PldFeedback.Info("/LittleDart-PLD mit            —— 打印减伤调度器状态（排查为什么不调度）");
		PldFeedback.Info("/LittleDart-PLD clear          —— 清扫技能队列（时间轴插的技能卡住时用）");
		PldFeedback.Info("/LittleDart-PLD help           —— 显示这份帮助");
		PldFeedback.Info("---------------------------------------------");
		PldFeedback.Info("可用 QT 开关：" + string.Join("、", PldQt.All.Keys));
		PldFeedback.Info("可用热键：" + string.Join("、", AvailableHotkeyNames()));
		PldFeedback.Info("=============================================");
	}

	private static IReadOnlyList<string> AvailableHotkeyNames()
	{
		IReadOnlyList<string> readOnlyList = PldHotkeyPanels.RegisteredNames();
		if (readOnlyList.Count <= 0)
		{
			return PldHotkeys.MainPanelNames;
		}
		return readOnlyList;
	}
}
