using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using PromeRotation.UI.Components;

namespace LittleDart.Common.Ui;

public static class UiTheme
{
	
	
	
	private static string _cachedThemeId = string.Empty;

	private static UiThemeDefinition? _cachedTheme;

	private static UiThemeDefinition Current
	{
		get
		{
			string id = InterfaceSettings.SelectedThemeId;
			UiThemeDefinition? cached = _cachedTheme;
			if (cached != null && string.Equals(_cachedThemeId, id, StringComparison.Ordinal))
			{
				return cached;
			}
			cached = ThemeCatalog.Resolve(id);
			_cachedTheme = cached;
			_cachedThemeId = id;
			return cached;
		}
	}

	private static UiPalette? Palette => Current.Palette;

	private static UiExtra? Extra => Current.Extra;

	
	public static string CurrentThemeName => Current.Name;

	
	public static UiStyle Style => Current.Style;

	
	public static bool Chamfered => Style == UiStyle.Chamfered;

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static float UiScale
	{
		get
		{
			
			float scale = ImGuiHelpers.GlobalScale;
			if (float.IsNaN(scale) || scale < 0.1f)
			{
				return 1f;
			}
			return MathF.Min(scale, 4f);
		}
	}

	
	public static float Px(float designPx) => designPx * UiScale;

	
	public static Vector2 Px(Vector2 designPx) => designPx * UiScale;

	
	
	
	
	public static float Cut => Chamfered ? Px(10f) : 0f;

	
	public static float CutSmall => Chamfered ? Px(5f) : 0f;

	
	public static float CutLarge => Chamfered ? Px(14f) : 0f;

	
	public static Vector4 Layout => Palette?.Layout ?? PromeUiTheme.Layout;

	public static Vector4 Surface => Palette?.Surface ?? PromeUiTheme.Surface;

	public static Vector4 SurfaceRaised => Palette?.SurfaceRaised ?? PromeUiTheme.SurfaceRaised;

	public static Vector4 SurfaceHover => Palette?.SurfaceHover ?? PromeUiTheme.SurfaceHover;

	
	public static Vector4 Border => Palette?.Border ?? PromeUiTheme.Border;

	public static Vector4 BorderStrong => Palette?.BorderStrong ?? PromeUiTheme.BorderStrong;

	public static Vector4 Text => Palette?.Text ?? PromeUiTheme.Text;

	public static Vector4 TextMuted => Palette?.TextMuted ?? PromeUiTheme.TextMuted;

	
	public static Vector4 Accent => Palette?.Accent ?? PromeUiTheme.Accent;

	public static Vector4 AccentStrong => Palette?.AccentStrong ?? PromeUiTheme.AccentStrong;

	public static Vector4 Success => Palette?.Success ?? PromeUiTheme.Success;

	public static Vector4 Warning => Palette?.Warning ?? PromeUiTheme.Warning;

	public static Vector4 Danger => Palette?.Danger ?? PromeUiTheme.Danger;

	
	
	public static Vector4 Heading => Extra?.Heading ?? PromeUiTheme.Mix(Danger, Text, 0.18f);

	
	public static Vector4 Micro => Extra is { } extra ? extra.Dim : PromeUiTheme.WithAlpha(TextMuted, 0.75f);

	
	public static Vector4 Well => Extra?.Well ?? PromeUiTheme.Mix(Accent, Layout, 0.86f);

	
	public static Vector4 Dim => Extra?.Dim ?? PromeUiTheme.Mix(Border, Layout, 0.30f);

	
	public static Vector4 Ink => Extra?.Ink ?? PromeUiTheme.WithAlpha(Layout, 1f);

	
	public static Vector4 Glow => PromeUiTheme.WithAlpha(Accent, 0.40f);

	
	public static Vector4 GlowSoft => PromeUiTheme.WithAlpha(Accent, 0.16f);

	
	public static Vector4 Hairline => PromeUiTheme.WithAlpha(Border, 0.85f);

	
	public static float WindowRounding => Chamfered ? 0f : PromeUiTheme.WindowRounding;

	public static float PanelRounding => Chamfered ? 0f : PromeUiTheme.PanelRounding;

	public static float ControlRounding => Chamfered ? 0f : PromeUiTheme.ControlRounding;

	
	
	
	
	public static Vector2 WindowPadding => Px(Chamfered ? new Vector2(12f, 10f) : PromeUiTheme.PanelPadding);

	
	
	public static Vector4 TitleBarBackground => PromeUiTheme.Mix(Surface, Accent, 0.07f);

	
	public static Vector4 CardBackground => Palette?.CardBackground ?? PromeUiTheme.Mix(Surface, SurfaceRaised, 0.55f);

	
	public static Vector4 StatusText => PromeUiTheme.Mix(TextMuted, Text, 0.25f);

	
	public static Vector4 Mix(Vector4 from, Vector4 to, float amount)
	{
		return PromeUiTheme.Mix(from, to, amount);
	}

	public static Vector4 Alpha(Vector4 color, float alpha)
	{
		return PromeUiTheme.WithAlpha(color, alpha);
	}

	
	public static uint ToU32(Vector4 color)
	{
		byte r = (byte)(Math.Clamp(color.X, 0f, 1f) * 255f);
		byte g = (byte)(Math.Clamp(color.Y, 0f, 1f) * 255f);
		byte b = (byte)(Math.Clamp(color.Z, 0f, 1f) * 255f);
		return (uint)(((byte)(Math.Clamp(color.W, 0f, 1f) * 255f) << 24) | (b << 16) | (g << 8) | r);
	}

	
	
	
	
	public static UiShellStyleScope PushShellStyle()
	{
		int colorCount = 0;
		int varCount = 0;

		void Color(ImGuiCol col, Vector4 value)
		{
			ImGui.PushStyleColor(col, value);
			colorCount++;
		}

		void Var(ImGuiStyleVar var, float value)
		{
			ImGui.PushStyleVar(var, value);
			varCount++;
		}

		void Var2(ImGuiStyleVar var, Vector2 value)
		{
			ImGui.PushStyleVar(var, value);
			varCount++;
		}

		
		
		
		
		bool colorsFromProme = Palette is null && !Chamfered && !PromeUiTheme.IsOriginal;
		if (!colorsFromProme)
		{
			Color(ImGuiCol.WindowBg, Layout);
			Color(ImGuiCol.ChildBg, Surface);
			Color(ImGuiCol.PopupBg, SurfaceRaised);
			Color(ImGuiCol.Border, Border);
			Color(ImGuiCol.BorderShadow, new Vector4(0f, 0f, 0f, 0f));
			Color(ImGuiCol.FrameBg, Chamfered ? PromeUiTheme.WithAlpha(CardBackground, 0.9f) : SurfaceRaised);
			Color(ImGuiCol.FrameBgHovered, Chamfered ? PromeUiTheme.Mix(CardBackground, Accent, 0.10f) : SurfaceHover);
			Color(ImGuiCol.FrameBgActive, Chamfered ? PromeUiTheme.Mix(CardBackground, Accent, 0.18f) : SurfaceHover);
			Color(ImGuiCol.Button, SurfaceRaised);
			Color(ImGuiCol.ButtonHovered, SurfaceHover);
			Color(ImGuiCol.ButtonActive, SurfaceHover);
			Color(ImGuiCol.Text, Text);
			Color(ImGuiCol.TextDisabled, TextMuted);
			Color(ImGuiCol.TextSelectedBg, PromeUiTheme.WithAlpha(Accent, Chamfered ? 0.28f : 0.35f));
			Color(ImGuiCol.Separator, Chamfered ? Hairline : BorderStrong);
			Color(ImGuiCol.CheckMark, Accent);
			Color(ImGuiCol.SliderGrab, Accent);
			Color(ImGuiCol.SliderGrabActive, AccentStrong);
			Color(ImGuiCol.Header, Chamfered ? PromeUiTheme.Mix(Well, Accent, 0.10f) : SurfaceRaised);
			Color(ImGuiCol.HeaderHovered, Chamfered ? PromeUiTheme.Mix(Well, Accent, 0.18f) : SurfaceHover);
			Color(ImGuiCol.HeaderActive, Chamfered ? PromeUiTheme.Mix(Well, Accent, 0.26f) : SurfaceHover);
			Color(ImGuiCol.ScrollbarBg, Layout);
			Color(ImGuiCol.ScrollbarGrab, Border);
			Color(ImGuiCol.ScrollbarGrabHovered, BorderStrong);
			Color(ImGuiCol.ScrollbarGrabActive, Accent);
		}

		
		
		Var(ImGuiStyleVar.WindowRounding, WindowRounding);
		Var(ImGuiStyleVar.ChildRounding, Chamfered ? 0f : PanelRounding);
		Var(ImGuiStyleVar.FrameRounding, Chamfered ? 0f : ControlRounding);
		Var2(ImGuiStyleVar.WindowPadding, WindowPadding);

		if (Chamfered)
		{
			
			
			Var(ImGuiStyleVar.PopupRounding, 0f);
			Var(ImGuiStyleVar.ScrollbarRounding, 0f);
			Var(ImGuiStyleVar.GrabRounding, 0f);
			Var(ImGuiStyleVar.TabRounding, 0f);
		}

		return new UiShellStyleScope(colorCount, varCount);
	}

	
	
	
	
	
	
	
	
	
	public static PromeUiStyleScope? PushWindowBase(float backgroundAlpha)
	{
		return Chamfered ? null : PromeUiTheme.PushWindowStyle(backgroundAlpha);
	}
}

public readonly struct UiShellStyleScope : IDisposable
{
	private readonly int _colorCount;

	private readonly int _varCount;

	internal UiShellStyleScope(int colorCount, int varCount)
	{
		_colorCount = colorCount;
		_varCount = varCount;
	}

	public void Dispose()
	{
		if (_varCount > 0)
		{
			ImGui.PopStyleVar(_varCount);
		}
		if (_colorCount > 0)
		{
			ImGui.PopStyleColor(_colorCount);
		}
	}
}
