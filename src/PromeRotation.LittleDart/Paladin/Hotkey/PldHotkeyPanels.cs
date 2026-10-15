using System;
using System.Collections.Generic;
using ECommons.Logging;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.UI.HotKey;

namespace LittleDart.Paladin.Hotkey;

public static class PldHotkeyPanels
{
	private const int MainColumns = 5;

	private const int SupportColumns = 3;

	private const float DefaultButtonSize = 45f;

	private const float Spacing = 5f;

	private const string MainWindowId = "littledart_pld_main";

	private const string SupportWindowId = "littledart_pld_support";

	private static HotkeyPanel? _mainPanel;

	private static HotkeyPanel? _supportPanel;

	public static bool IsRegistered
	{
		get
		{
			if (_mainPanel == null)
			{
				return _supportPanel != null;
			}
			return true;
		}
	}

	public static void Register()
	{
		if (IsRegistered)
		{
			return;
		}
		try
		{
			PldSettings instance = PldSettings.Instance;
			float configuredIconSize = instance?.PartnerPanelIconSize.X ?? 45f;
			float buttonSize = (float.IsFinite(configuredIconSize) ? Math.Clamp(configuredIconSize, 24f, 96f) : 45f);
			bool supportPanelOpen = instance?.PartnerPanelShow ?? true;
			_mainPanel = new HotkeyPanel(5, buttonSize, 5f, "LittleDart · 骑士", "littledart_pld_main");
			BuildMainPanel(_mainPanel);
			_supportPanel = new HotkeyPanel(3, buttonSize, 5f, "LittleDart · 九宫格支援", "littledart_pld_support");
			BuildSupportPanel(_supportPanel);
			_supportPanel.IsOpen = supportPanelOpen;
			HotkeyManager.Instance.AddHotkeyPanel(_mainPanel);
			HotkeyManager.Instance.AddHotkeyPanel(_supportPanel);
			PldHelper.Log($"热键面板已注册：主面板栏 {_mainPanel.Count} 个、九宫格支援 {_supportPanel.Count} 个（按钮 {buttonSize:F0}px，支援面板{(supportPanelOpen ? "显示" : "隐藏")}）");
		}
		catch (Exception exception)
		{
			PluginLog.Error($"[LittleDart-PLD/热键] 热键面板注册失败：{exception}");
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
		PldHelper.Log("热键面板已从 PR 窗口系统摘除");
	}

	private static void TryRemove(HotkeyPanel panel)
	{
		try
		{
			HotkeyManager.Instance.RemoveHotkeyPanel(panel);
		}
		catch (Exception exception)
		{
			PluginLog.Error($"[LittleDart-PLD/热键] 摘除热键面板失败：{exception}");
		}
	}

	public static bool TryTrigger(string name)
	{
		HotkeyPanel[] panels = new HotkeyPanel[2] { _mainPanel, _supportPanel };
		foreach (HotkeyPanel hotkeyPanel in panels)
		{
			if (hotkeyPanel == null)
			{
				continue;
			}
			foreach (var (a, hotkey) in hotkeyPanel.EnumerateHotkeys())
			{
				if (string.Equals(a, name, StringComparison.Ordinal))
				{
					try
					{
						hotkey.OnClick();
					}
					catch (Exception exception)
					{
						PluginLog.Error($"[LittleDart-PLD/热键] 触发「{name}」时异常：{exception}");
					}
					return true;
				}
			}
		}
		return false;
	}

	public static IReadOnlyList<string> RegisteredNames()
	{
		List<string> list = new List<string>();
		HotkeyPanel[] panels = new HotkeyPanel[2] { _mainPanel, _supportPanel };
		foreach (HotkeyPanel hotkeyPanel in panels)
		{
			if (hotkeyPanel == null)
			{
				continue;
			}
			foreach (var hotkeyEntry in hotkeyPanel.EnumerateHotkeys())
			{
				string item = hotkeyEntry.Item1;
				list.Add(item);
			}
		}
		return list;
	}

	private static void BuildMainPanel(HotkeyPanel panel)
	{
		panel.AddHotkey("圣盾阵", PldAction.Self(25746u, (ActionType)1));
		panel.AddHotkey("铁壁", PldAction.Self(7531u, (ActionType)1));
		panel.AddHotkey("壁垒", PldAction.Self(22u, (ActionType)1));
		panel.AddHotkey("极致防御", PldAction.Self(36920u, (ActionType)1));
		panel.AddHotkey("亲疏自行", PldAction.Self(7548u, (ActionType)1));
		panel.AddHotkey("挑衅", PldAction.Enemy(7533u, (ActionType)1));
		panel.AddHotkey("退避", (IHotkeyLogic)(object)new ShirkToPartnerHotkey(), 7537u, (string)null);
		panel.AddHotkey("调停", PldAction.Enemy(16461u, (ActionType)1));
		panel.AddHotkey("圣灵", PldAction.Enemy(7384u, (ActionType)0));
		panel.AddHotkey("血仇", PldAction.Enemy(7535u, (ActionType)1));
		panel.AddHotkey("武装戍卫", PldAction.Self(7385u, (ActionType)1));
		panel.AddHotkey("冲刺", PldAction.Self(3u, (ActionType)1));
		panel.AddHotkey("神圣领域", PldAction.Self(30u, (ActionType)1));
		panel.AddHotkey("LB", (IHotkeyLogic)(object)new PldLimitBreakHotkeyLogic(), 31399u, (string)null);
		panel.AddHotkey("清扫队列", (IHotkeyLogic)(object)new ClearQueueHotkey(), 0u, PldHotkeyIcon.Resolve("ClearQueue.png"));
	}

	private static void BuildSupportPanel(HotkeyPanel panel)
	{
		AddSupport(panel, "干预搭档", new InterventionPartnerHotkey());
		AddSupport(panel, "深仁厚泽搭档", new ClemencyPartnerHotkey());
		AddSupport(panel, "保护搭档", new CoverPartnerHotkey());
		AddSupport(panel, "干预奶妈", new InterventionHealerHotkey());
		AddSupport(panel, "深仁厚泽奶妈", new ClemencyHealerHotkey());
		AddSupport(panel, "保护奶妈", new CoverHealerHotkey());
		AddSupport(panel, "干预血量最低", new InterventionLowestHpSupportHotkey());
		AddSupport(panel, "深仁厚泽血量最低", new ClemencyLowestHpHotkey());
		AddSupport(panel, "保护血量最低", new CoverLowestHpHotkey());
	}

	private static void AddSupport(HotkeyPanel panel, string name, PldSupportHotkeyBase logic)
	{
		panel.AddHotkey(name, (IHotkeyLogic)(object)logic, logic.IconActionId, logic.CustomIconPath);
	}
}
