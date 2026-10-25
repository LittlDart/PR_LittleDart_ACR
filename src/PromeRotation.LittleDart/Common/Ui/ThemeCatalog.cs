using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LittleDart.Common.Ui;

public enum UiStyle
{
	
	Rounded = 0,

	
	Chamfered = 1,
}

public readonly record struct UiPalette(
	Vector4 Layout,
	Vector4 Surface,
	Vector4 SurfaceRaised,
	Vector4 SurfaceHover,
	Vector4 CardBackground,
	Vector4 Border,
	Vector4 BorderStrong,
	Vector4 Text,
	Vector4 TextMuted,
	Vector4 Accent,
	Vector4 AccentStrong,
	Vector4 Success,
	Vector4 Warning,
	Vector4 Danger);

public readonly record struct UiExtra(
	Vector4 Heading,
	Vector4 Well,
	Vector4 Dim,
	Vector4 Ink);

public sealed record UiThemeDefinition(
	string Id,
	string Name,
	UiPalette? Palette,
	UiStyle Style = UiStyle.Rounded,
	UiExtra? Extra = null);

public static class ThemeCatalog
{
	
	public const string DefaultId = "default";

	
	public const string CyberdeckId = "cyberdeck";

	private static readonly List<UiThemeDefinition> Themes = new List<UiThemeDefinition>
	{
		new UiThemeDefinition(
			DefaultId,
			"跟随系统",
			null),

		
		
		
		
		
		new UiThemeDefinition(
			CyberdeckId,
			"赛博风格",
			new UiPalette(
				
				Layout: Hex(0x0A0608, 0.90f),
				Surface: Hex(0x140709, 0.92f),
				SurfaceRaised: Hex(0x1E080A, 0.75f),
				SurfaceHover: Hex(0x2A0B0C, 0.92f),
				CardBackground: Hex(0x160809, 0.92f),
				
				Border: Hex(0x7A1814),
				BorderStrong: Hex(0xFF3828),
				
				Text: Hex(0xFFFFFF),
				TextMuted: Hex(0x8A241E),
				
				Accent: Hex(0x00F0FF),
				AccentStrong: Hex(0x00C2D6),
				
				Success: Hex(0x35E0A0),
				Warning: Hex(0xFFB020),
				Danger: Hex(0xFF3828)),
			UiStyle.Chamfered,
			new UiExtra(
				Heading: Hex(0xFF4636),
				Well: Hex(0x062830),
				Dim: Hex(0x4A1210),
				Ink: Hex(0x0A0608)))
	};

	
	public static IReadOnlyList<UiThemeDefinition> All => Themes;

	
	public static UiThemeDefinition Resolve(string? id)
	{
		return Themes.FirstOrDefault((UiThemeDefinition theme) => string.Equals(theme.Id, id, StringComparison.Ordinal)) ?? Themes[0];
	}

	
	public static int IndexOf(string? id)
	{
		int index = Themes.FindIndex((UiThemeDefinition theme) => string.Equals(theme.Id, id, StringComparison.Ordinal));
		return index < 0 ? 0 : index;
	}

	
	private static Vector4 Hex(int rgb, float alpha = 1f)
	{
		return new Vector4(
			((rgb >> 16) & 0xFF) / 255f,
			((rgb >> 8) & 0xFF) / 255f,
			(rgb & 0xFF) / 255f,
			alpha);
	}
}
