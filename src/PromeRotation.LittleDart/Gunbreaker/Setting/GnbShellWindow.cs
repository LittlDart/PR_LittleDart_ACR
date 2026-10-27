using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using LittleDart.Common.Ui;
using PromeRotation;
using PromeRotation.UI.Components;

namespace LittleDart.Gunbreaker.Setting;

public sealed class GnbShellWindow : LittleDartShellWindow
{
	public const string WindowId = "littledart_gnb_shell";

	private static GnbShellWindow? _instance;

	private static bool _registered;

	public static GnbShellWindow Instance => _instance ??= new GnbShellWindow();

	private GnbShellWindow()
		: base(WindowId, "LittleDart · 绝枪战士")
	{
	}

	protected override string JobLabel => "绝枪战士";

	protected override string JobShort => "GNB";

	protected override bool IsDailyMode => GnbHelper.IsDailyMode();

	protected override IReadOnlyList<string> PageNames => GnbSettingUi.ShellPageNames;

	
	protected override void DrawModeStrip()
	{
		GnbSettingUi.DrawModeSwitchOnly();
	}

	protected override void DrawPageContent(int pageIndex)
	{
		if (pageIndex >= GnbSettingUi.PageCount)
		{
			ThemeSection.Draw(GnbWindowSwitcher.SwitchTo);
			return;
		}
		GnbSettingUi.DrawPage(pageIndex);
	}

	public static void Register()
	{
		if (_registered)
		{
			return;
		}
		Plugin.Instance.WindowSystem.AddWindow((IWindow)(object)Instance);
		_registered = true;
		GnbHelper.Log("LittleDart 自建界面窗口已注册到 PR 窗口系统");
	}

	public static void Unregister()
	{
		if (!_registered)
		{
			return;
		}
		_registered = false;
		Instance.SaveLayoutNow();
		((Window)Instance).IsOpen = false;
		Plugin.Instance.WindowSystem.RemoveWindow((IWindow)(object)Instance);
		GnbHelper.Log("LittleDart 自建界面窗口已从 PR 窗口系统摘除");
	}
}
