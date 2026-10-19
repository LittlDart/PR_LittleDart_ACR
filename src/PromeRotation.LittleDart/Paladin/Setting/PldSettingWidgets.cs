using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using PromeRotation.Data;
using PromeRotation.UI.Components;

namespace LittleDart.Paladin.Setting;

public static class PldSettingWidgets
{
	private const float SliderWidth = 150f;

	/// <summary>
	/// 告警文字色。★ 必须是**属性**不能是 <c>static readonly</c> 字段：
	/// 字段只在类型初始化时求值一次，切 PR 配色方案不会重算（docs\32:526「每帧读不缓存」）。
	/// 原写死 (0.92,0.36,0.36) 在浅色 Surface(255,255,255) 上只有 3.39:1；改成
	/// <c>Mix(Danger, Text, .3)</c> 后浅色 6.57:1 / 深色 6.89:1（docs\31 T16 / docs\39 §6 批次 E）。
	/// </summary>
	private static Vector4 RedText => PromeUiTheme.Mix(PromeUiTheme.Danger, PromeUiTheme.Text, 0.3f);

	private static void Save()
	{
		PldSettings.Instance?.Save();
	}

	public static void IntSlider(string label, ref int value, int min, int max)
	{
		float x = value;
		if (PromeUi.SliderRow(label, label, string.Empty, ref x, (float)min, (float)max, "%.0f", 150f))
		{
			int clampedValue = Math.Clamp((int)MathF.Round(x), min, max);
			if (clampedValue != value)
			{
				value = clampedValue;
				Save();
			}
		}
	}

	public static void IntStepper(string label, ref int value, int min, int max)
	{
		int editedValue = value;
		PromeUi.InputIntRow(label, label, string.Empty, ref editedValue, 1, 10, 180f);
		if (editedValue != value)
		{
			value = Math.Clamp(editedValue, min, max);
			Save();
		}
	}

	public static void IntStepperFloat(string label, ref float value, int min, int max)
	{
		int editedValue = (int)MathF.Round(value);
		int originalValue = editedValue;
		PromeUi.InputIntRow(label, label, string.Empty, ref editedValue, 1, 10, 180f);
		if (editedValue != originalValue)
		{
			value = Math.Clamp(editedValue, min, max);
			Save();
		}
	}

	public static void ComboRow(string label, ref int index, string[] items)
	{
		if (items.Length != 0)
		{
			if (index < 0 || index >= items.Length)
			{
				index = 0;
				Save();
			}
			if (PromeUi.ComboRow(label, label, string.Empty, ref index, (IReadOnlyList<string>)items, 180f))
			{
				Save();
			}
		}
	}

	public static void FieldCheckbox(string label, ref bool value)
	{
		bool pendingValue = value;
		if (PromeUi.ToggleRow("pld_fld_" + label, label, string.Empty, ref pendingValue))
		{
			value = pendingValue;
			Save();
		}
	}

	public static void QtCheckbox(string qtName, string label)
	{
		bool qt = PromeSettings.Instance.GetQt(qtName);
		if (PromeUi.ToggleRow("qt_" + qtName, label, string.Empty, ref qt))
		{
			PromeSettings.Instance.SetQt(qtName, qt);
			Save();
		}
	}

	public static void RedCheckbox(string label, ref bool value)
	{
		bool pendingValue = value;
		ImGui.PushStyleColor((ImGuiCol)0, RedText);
		bool clicked = ImGui.Checkbox($"{label}###pld_red_{label}", ref pendingValue);
		ImGui.PopStyleColor();
		if (clicked)
		{
			value = pendingValue;
			Save();
		}
	}

	public static void RedQtCheckbox(string qtName, string label)
	{
		bool qt = PromeSettings.Instance.GetQt(qtName);
		ImGui.PushStyleColor((ImGuiCol)0, RedText);
		bool clicked = ImGui.Checkbox($"{label}###qt_{qtName}", ref qt);
		ImGui.PopStyleColor();
		if (clicked)
		{
			PromeSettings.Instance.SetQt(qtName, qt);
			Save();
		}
	}

	public static void BoundCheckbox(string id, string label, bool value, Action<bool> onChanged, string description = "")
	{
		bool pendingValue = value;
		if (PromeUi.ToggleRow(id, label, description ?? string.Empty, ref pendingValue))
		{
			onChanged(pendingValue);
		}
	}

	public static void BoundComboRow(string id, string label, int index, string[] items, Action<int> onChanged, string description = "")
	{
		if (items.Length != 0)
		{
			int pendingIndex = ((index >= 0 && index < items.Length) ? index : 0);
			if (PromeUi.ComboRow(id, label, description ?? string.Empty, ref pendingIndex, (IReadOnlyList<string>)items, 180f))
			{
				onChanged(pendingIndex);
			}
		}
	}
}
