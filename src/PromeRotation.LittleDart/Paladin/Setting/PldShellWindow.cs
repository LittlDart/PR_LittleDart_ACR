using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using LittleDart.Common.Ui;
using PromeRotation;
using PromeRotation.UI.Components;

namespace LittleDart.Paladin.Setting;

public sealed class PldShellWindow : LittleDartShellWindow
{
	public const string WindowId = "littledart_pld_shell";

	private static PldShellWindow? _instance;

	private static bool _registered;

	public static PldShellWindow Instance => _instance ??= new PldShellWindow();

	private PldShellWindow()
		: base(WindowId, "LittleDart · 骑士")
	{
	}

	protected override string JobLabel => "骑士";

	protected override string JobShort => "PLD";

	protected override bool IsDailyMode => PldHelper.IsDailyMode();

	protected override IReadOnlyList<string> PageNames => PldSettingUi.ShellPageNames;

	
	protected override void DrawModeStrip()
	{
		PldSettingUi.DrawModeSwitchOnly();
	}

	protected override void DrawPageContent(int pageIndex)
	{
		if (pageIndex >= PldSettingUi.PageCount)
		{
			ThemeSection.Draw(PldWindowSwitcher.SwitchTo);
			return;
		}
		PldSettingUi.DrawPage(pageIndex);
	}

	public static void Register()
	{
		if (_registered)
		{
			return;
		}
		Plugin.Instance.WindowSystem.AddWindow((IWindow)(object)Instance);
		_registered = true;
		PldHelper.Log("LittleDart 自建界面窗口已注册到 PR 窗口系统");
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
		PldHelper.Log("LittleDart 自建界面窗口已从 PR 窗口系统摘除");
	}
}
