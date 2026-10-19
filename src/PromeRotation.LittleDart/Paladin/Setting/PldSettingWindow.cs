using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using PromeRotation;
using PromeRotation.UI.Components;

namespace LittleDart.Paladin.Setting;

public sealed class PldSettingWindow : Window
{
	private static PldSettingWindow? _instance;

	private static bool _registered;

	/// <summary>
	/// 主题作用域。docs\39 §6 批次 E：PR **不会**替 ACR 自己的独立窗口 push 主题
	/// （<c>Plugin.WindowSystem</c> 就是 Dalamud 原生 WindowSystem，没有包装层；
	/// 只有 PR 自己的 <c>Window</c> 子类在 <c>PreDraw()</c> 里 push）。
	/// </summary>
	/// <remarks>
	/// 写在 <c>PreDraw/PostDraw</c> 而不是 <c>Draw()</c> 里 <c>using</c> 的原因：
	/// <c>ImGuiCol.WindowBg/ChildBg</c> 必须在 <c>ImGui.Begin()</c> **之前** push 才能改窗口底色。
	/// 参数用 <c>1f</c>（普通设置窗，不是战斗浮层，不要半透明）。
	/// 构造里 += 了什么就在这里对称清理：PreDraw 造 scope ⇒ PostDraw Dispose 并置 null。
	/// <para>⚠️ 面板路径（<c>IRotation.DrawSettings()</c>）已被 PR 自动 push，那里不要再 push。</para>
	/// </remarks>
	private PromeUiStyleScope? _styleScope;

	public static PldSettingWindow Instance => _instance ?? (_instance = new PldSettingWindow());

	private PldSettingWindow()
		: base("LittleDart · 骑士设置###pld_settings_window")
	{
		WindowSizeConstraints value = new WindowSizeConstraints();
		value.MinimumSize = new Vector2(420f, 320f);
		value.MaximumSize = new Vector2(1600f, 1600f);
		((Window)this).SizeConstraints = value;
		((Window)this).Size = new Vector2(560f, 640f);
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).RespectCloseHotkey = true;
	}

	public override void PreDraw()
	{
		_styleScope?.Dispose();
		_styleScope = PromeUiTheme.PushWindowStyle(1f);
		base.PreDraw();
	}

	public override void PostDraw()
	{
		_styleScope?.Dispose();
		_styleScope = null;
		base.PostDraw();
	}

	public override void Draw()
	{
		PldSettingUi.DrawWindowBody();
	}

	public static void Register()
	{
		if (!_registered)
		{
			Plugin.Instance.WindowSystem.AddWindow((IWindow)(object)Instance);
			_registered = true;
			PldHelper.Log("独立设置窗口已注册到 PR 窗口系统");
		}
	}

	public static void Unregister()
	{
		if (_registered)
		{
			_registered = false;
			((Window)Instance).IsOpen = false;
			Plugin.Instance.WindowSystem.RemoveWindow((IWindow)(object)Instance);
			PldHelper.Log("独立设置窗口已从 PR 窗口系统摘除");
		}
	}

	public static void ToggleWindow()
	{
		Register();
		((Window)Instance).Toggle();
	}
}
