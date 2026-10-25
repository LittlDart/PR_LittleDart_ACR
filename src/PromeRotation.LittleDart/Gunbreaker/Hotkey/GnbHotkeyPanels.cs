using System;
using System.Collections.Generic;
using ECommons.Logging;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 绝枪的**热键面板**（2026-10-15 新增；此前绝枪一个热键都没有，见 docs\21 §3.1）。
///
/// <para><b>两个面板</b>：</para>
/// <list type="number">
/// <item><b>主面板</b>（<c>LittleDart · 绝枪</c>）：5 列 × 3 行 = 15 个按钮，
/// 顺序 = 玩家指定（见 <see cref="GnbHotkeys.MainPanelNames"/>），第 1 行第 1 个是「刚玉」。</item>
/// <item><b>支援面板</b>（<c>LittleDart · 绝枪支援</c>）：**2 列 × 3 行 = 六宫格**
/// —— 刚玉之心 / 极光 × 搭档坦克 / 治疗 / 血量最低，**刚玉在前极光在后**
/// （见 <see cref="BuildSupportPanel"/>）。对标骑士的「九宫格支援」，命名规范同款。</item>
/// </list>
///
/// <para><b>注册 / 注销的时机</b>：在 <c>GnbRotationEventHandler.OnEnterRotation</c> /
/// <c>OnExitRotation</c> 里成对调用（与骑士完全同构）。PR 会重载 ACR ⇒
/// **必须**成对，否则面板会越挂越多、且换职业后残留。</para>
///
/// <para><b>按钮尺寸</b>：固定 45px（PR <c>HotkeyPanel</c> 的默认值）。骑士那边读的是设置里的
/// 「搭档面板图标大小」，绝枪暂时没有这项设置 —— 要能调再补设置项，不要在这里读骑士的设置。</para>
/// </summary>
public static class GnbHotkeyPanels
{
	private const int MainColumns = 5;

	/// <summary>支援面板列数：**2 列 × 3 行 = 六宫格**（左列刚玉之心、右列极光）。</summary>
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

	/// <summary>按名字点一下某个按钮（<c>/LittleDart-GNB hk &lt;名字&gt;</c> 走这里）。两个面板都会找。</summary>
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

	/// <summary>面板上已注册的名字（面板没注册时返回空表）；主面板在前、支援面板在后。</summary>
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

	/// <summary>
	/// 建面板：★ **AddHotkey 的调用顺序 = 面板上从左到右、从上到下的顺序**，
	/// 与 <see cref="GnbHotkeys.MainPanelNames"/> 必须逐字一致。
	///
	/// <para>技能 ID 一律走 <see cref="GnbSkills"/> 常量、目标类型走 <c>GnbAction</c> 的三个工厂
	/// （<c>Self</c> / <c>Enemy</c>），不要在这里写裸数字。</para>
	///
	/// <para><b>三个逻辑按钮</b>（退避 / LB / 清扫队列）不是「一个固定技能的 action」，
	/// 所以用 <c>IHotkeyLogic</c> 重载：退避要点名另一个坦克、LB 的 ID 要运行时取、
	/// 清扫队列干脆不是技能。</para>
	/// </summary>
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

	/// <summary>
	/// 建**支援面板**（六宫格）：2 列 × 3 行，**刚玉在前、极光在后**（左列刚玉之心、右列极光），
	/// 三行依次是搭档坦克 / 治疗 / 血量最低的队友。
	///
	/// <para><b>行优先填充</b>（PR <c>HotkeyPanel</c> 的 tiling 行为）⇒ 顺序就是
	/// 搭档·刚玉 → 搭档·极光 → 奶妈·刚玉 → 奶妈·极光 → 残血·刚玉 → 残血·极光。</para>
	///
	/// <para>与骑士「九宫格支援」的关系：骑士是 3 技能（干预 / 深仁厚泽 / 保护）× 3 类目标 = 9 格；
	/// 绝枪只有两个技能能给队友（刚玉之心 / 极光）⇒ 2 × 3 = **6 格**。命名规范与骑士一致
	/// （<c>&lt;技能&gt;&lt;目标&gt;</c>，例如骑士的「干预搭档」↔ 这里的「刚玉搭档」）。</para>
	///
	/// <para>名字表在 <see cref="GnbHotkeys.SupportPanelNames"/> —— 改这里请**同时**改那边。</para>
	/// </summary>
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
