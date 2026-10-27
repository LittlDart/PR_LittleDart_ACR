using System.Collections.Generic;
using System.Numerics;
using PromeRotation.UI.Components;

namespace LittleDart.Common.Ui;

public enum UiSurface
{
	
	Prome = 0,

	
	Shell = 1,
}

public static class UiKit
{
	public static UiSurface Surface { get; set; } = UiSurface.Prome;

	private static bool IsShell => Surface == UiSurface.Shell;

	public static void SectionHeader(string title, string? description = null, bool accent = true)
	{
		if (IsShell)
		{
			ShellWidgets.SectionHeader(title, description, accent);
		}
		else
		{
			PromeUi.SectionHeader(title, description, accent);
		}
	}

	public static bool TabButton(string id, string label, bool selected, float width = 0f)
	{
		return IsShell
			? ShellWidgets.TabButton(id, label, selected, width)
			: PromeUi.TabButton(id, label, selected, width);
	}

	public static bool ToggleRow(string id, string label, string description, ref bool value)
	{
		return IsShell
			? ShellWidgets.ToggleRow(id, label, description, ref value)
			: PromeUi.ToggleRow(id, label, description, ref value);
	}

	
	
	
	
	
	
	public static bool Checkbox(string id, string label, string description, ref bool value)
	{
		if (IsShell)
		{
			string text = string.IsNullOrEmpty(description) ? label : label + "  " + description;
			return ShellWidgets.Checkbox(id, text, ref value);
		}
		return PromeUi.ToggleRow(id, label, description ?? string.Empty, ref value);
	}

	
	
	
	
	
	internal static bool ModeSwitch(string id, bool daily, ref ModeSwitchAnim anim)
	{
		return IsShell
			? ShellWidgets.ModeSwitch(id, daily, ref anim)
			: ThemeModeSwitch.Draw(id, daily, ref anim);
	}

	public static bool ComboRow(string id, string label, string description, ref int selectedIndex, IReadOnlyList<string> options, float controlWidth = 180f)
	{
		return IsShell
			? ShellWidgets.ComboRow(id, label, description, ref selectedIndex, options, controlWidth)
			: PromeUi.ComboRow(id, label, description, ref selectedIndex, options, controlWidth);
	}

	public static bool InputIntRow(string id, string label, string description, ref int value, int step = 1, int fastStep = 10, float controlWidth = 180f)
	{
		return IsShell
			? ShellWidgets.InputIntRow(id, label, description, ref value, step, fastStep, controlWidth)
			: PromeUi.InputIntRow(id, label, description, ref value, step, fastStep, controlWidth);
	}

	public static bool SliderRow(string id, string label, string description, ref float value, float minimum, float maximum, string format = "%.1f", float controlWidth = 180f)
	{
		return IsShell
			? ShellWidgets.SliderRow(id, label, description, ref value, minimum, maximum, format, controlWidth)
			: PromeUi.SliderRow(id, label, description, ref value, minimum, maximum, format, controlWidth);
	}

	public static bool PrimaryButton(string id, string label, Vector2 size)
	{
		return IsShell ? ShellWidgets.PrimaryButton(id, label, size) : PromeUi.PrimaryButton(id, label, size);
	}

	public static void StatusPill(string id, string label, Vector4 color)
	{
		if (IsShell)
		{
			ShellWidgets.StatusPill(id, label, color);
		}
		else
		{
			PromeUi.StatusPill(id, label, color);
		}
	}
}
