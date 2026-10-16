using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using PromeRotation;

namespace LittleDart.Gunbreaker.Setting;

public sealed class GnbSettingWindow : Window
{
	private static GnbSettingWindow? _instance;

	private static bool _registered;

	public static GnbSettingWindow Instance => _instance ?? (_instance = new GnbSettingWindow());

	private GnbSettingWindow()
		: base("LittleDart 绝枪战士设置###gnb_settings_window")
	{
		WindowSizeConstraints value = new WindowSizeConstraints();
		value.MinimumSize = new Vector2(420f, 320f);
		value.MaximumSize = new Vector2(1600f, 1600f);
		((Window)this).SizeConstraints = value;
		((Window)this).Size = new Vector2(560f, 640f);
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).RespectCloseHotkey = true;
	}

	public override void Draw()
	{
		GnbSettingUi.DrawWindowBody();
	}

	public static void Register()
	{
		if (!_registered)
		{
			Plugin.Instance.WindowSystem.AddWindow((IWindow)(object)Instance);
			_registered = true;
			GnbHelper.Log("独立设置窗口已注册到 PR 窗口系统");
		}
	}

	public static void Unregister()
	{
		if (_registered)
		{
			_registered = false;
			((Window)Instance).IsOpen = false;
			Plugin.Instance.WindowSystem.RemoveWindow((IWindow)(object)Instance);
			GnbHelper.Log("独立设置窗口已从 PR 窗口系统摘除");
		}
	}

	public static void ToggleWindow()
	{
		Register();
		((Window)Instance).Toggle();
	}
}
