using System;
using System.Collections.Generic;
using ECommons.Logging;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

public static class GnbHotkeyPanels
{
	private const int MainColumns = 5;

		private const int SupportColumns = 2;

	private const float DefaultButtonSize = 45f;

	private const float Spacing = 5f;

	private const string MainWindowId = "littledart_gnb_main";

	private const string SupportWindowId = "littledart_gnb_support";

	private static HotkeyPanel? _mainPanel;

	private static HotkeyPanel? _supportPanel;

	public static bool IsRegistered => _mainPanel != null && _supportPanel != null;

	public static void Register()
	{
		if (IsRegistered)
		{
			return;
		}
		try
		{
			_mainPanel = new HotkeyPanel(MainColumns, DefaultButtonSize, Spacing, "LittleDart · 绝枪", MainWindowId);
			BuildMainPanel(_mainPanel);
			_supportPanel = new HotkeyPanel(SupportColumns, DefaultButtonSize, Spacing, "LittleDart · 绝枪支援", SupportWindowId);
			BuildSupportPanel(_supportPanel);
			HotkeyManager.Instance.AddHotkeyPanel(_mainPanel);
			HotkeyManager.Instance.AddHotkeyPanel(_supportPanel);
			GnbHelper.Log($"热键面板已注册：主面板栏 {_mainPanel.Count} 个（5 列）、六宫格支援 {_supportPanel.Count} 个（2 列，按钮 {DefaultButtonSize:F0}px）");
		}
		catch (Exception exception)
		{
			PluginLog.Error($"[LittleDart-GNB/热键] 热键面板注册失败：{exception}");
			Unregister();
		}
	}

	public static void Unregister()
	{
		if (_mainPanel != null)
		{
			TryRemove(_mainPanel);
			_mainPanel = null;
		}
		if (_supportPanel != null)
		{
			TryRemove(_supportPanel);
			_supportPanel = null;
		}
		GnbHelper.Log("热键面板已从 PR 窗口系统摘除");
	}

	private static void TryRemove(HotkeyPanel panel)
	{
		try
		{
			HotkeyManager.Instance.RemoveHotkeyPanel(panel);
		}
		catch (Exception exception)
		{
			PluginLog.Error($"[LittleDart-GNB/热键] 摘除热键面板失败：{exception}");
		}
	}

		public static bool TryTrigger(string name)
	{
		HotkeyPanel[] panels = new HotkeyPanel[2] { _mainPanel, _supportPanel };
		foreach (HotkeyPanel panel in panels)
		{
			if (panel == null)
			{
				continue;
			}
			foreach (var (hotkeyName, hotkey) in panel.EnumerateHotkeys())
			{
				if (string.Equals(hotkeyName, name, StringComparison.Ordinal))
				{
					try
					{
						hotkey.OnClick();
					}
					catch (Exception exception)
					{
						PluginLog.Error($"[LittleDart-GNB/热键] 触发「{name}」时异常：{exception}");
					}
					return true;
				}
			}
		}
		return false;
	}

		public static IReadOnlyList<string> RegisteredNames()
	{
		List<string> names = new List<string>();
		HotkeyPanel[] panels = new HotkeyPanel[2] { _mainPanel, _supportPanel };
		foreach (HotkeyPanel panel in panels)
		{
			if (panel == null)
			{
				continue;
			}
			foreach (var (hotkeyName, _) in panel.EnumerateHotkeys())
			{
				names.Add(hotkeyName);
			}
		}
		return names;
	}

		private static void BuildMainPanel(HotkeyPanel panel)
	{
		panel.AddHotkey("刚玉", GnbAction.Self(GnbSkills.刚玉之心, ActionType.OffGcd));
		panel.AddHotkey("铁壁", GnbAction.Self(GnbSkills.铁壁, ActionType.OffGcd));
		panel.AddHotkey("伪装", GnbAction.Self(GnbSkills.伪装, ActionType.OffGcd));
		panel.AddHotkey("大星云", GnbAction.Self(GnbSkills.大星云, ActionType.OffGcd));
		panel.AddHotkey("亲疏自行", GnbAction.Self(GnbSkills.亲疏, ActionType.OffGcd));
		panel.AddHotkey("挑衅", GnbAction.Enemy(GnbSkills.挑衅, ActionType.OffGcd));
		panel.AddHotkey("退避", new GnbShirkToPartnerHotkey(), GnbSkills.退避, null);
		panel.AddHotkey("弹道", GnbAction.Enemy(GnbSkills.弹道, ActionType.OffGcd));
		panel.AddHotkey("闪雷弹", GnbAction.Enemy(GnbSkills.闪雷弹, ActionType.Gcd));
		panel.AddHotkey("血仇", GnbAction.Enemy(GnbSkills.雪仇, ActionType.OffGcd));
		panel.AddHotkey("极光", GnbAction.Self(GnbSkills.极光, ActionType.OffGcd));
		panel.AddHotkey("冲刺", GnbAction.Self(GnbSkills.冲刺, ActionType.OffGcd));
		panel.AddHotkey("超火流星", GnbAction.Self(GnbSkills.超火流星, ActionType.OffGcd));
		panel.AddHotkey("LB", new GnbLimitBreakHotkeyLogic(), GnbLimitBreakHotkeyLogic.IconActionId, null);
		panel.AddHotkey("清扫队列", new GnbClearQueueHotkey(), 0u, GnbHotkeyIcon.Resolve("ClearQueue.png"));
	}

		private static void BuildSupportPanel(HotkeyPanel panel)
	{
		AddSupport(panel, "刚玉搭档", new GnbHeartOfCorundumPartnerHotkey());
		AddSupport(panel, "极光搭档", new GnbAuroraPartnerHotkey());
		AddSupport(panel, "刚玉奶妈", new GnbHeartOfCorundumHealerHotkey());
		AddSupport(panel, "极光奶妈", new GnbAuroraHealerHotkey());
		AddSupport(panel, "刚玉血量最低", new GnbHeartOfCorundumLowestHpHotkey());
		AddSupport(panel, "极光血量最低", new GnbAuroraLowestHpHotkey());
	}

	private static void AddSupport(HotkeyPanel panel, string name, GnbSupportHotkeyBase logic)
	{
		panel.AddHotkey(name, logic, logic.IconActionId, logic.CustomIconPath);
	}
}
