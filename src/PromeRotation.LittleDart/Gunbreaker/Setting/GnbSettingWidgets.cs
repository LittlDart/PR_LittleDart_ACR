using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using PromeRotation.Data;
using PromeRotation.UI.Components;

namespace LittleDart.Gunbreaker.Setting;

public static class GnbSettingWidgets
{
	
	
	
	
	
	
	private static Vector4 RedText => PromeUiTheme.Mix(PromeUiTheme.Danger, PromeUiTheme.Text, 0.3f);

	private static void Save()
	{
		GnbSettings.Instance?.Save();
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
		int roundedValue = (int)MathF.Round(value);
		int originalValue = roundedValue;
		PromeUi.InputIntRow(label, label, string.Empty, ref roundedValue, 1, 10, 180f);
		if (roundedValue != originalValue)
		{
			value = Math.Clamp(roundedValue, min, max);
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
		bool editedValue = value;
		if (PromeUi.ToggleRow("pld_fld_" + label, label, string.Empty, ref editedValue))
		{
			value = editedValue;
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
		bool editedValue = value;
		ImGui.PushStyleColor((ImGuiCol)0, RedText);
		bool changed = ImGui.Checkbox($"{label}###pld_red_{label}", ref editedValue);
		ImGui.PopStyleColor();
		if (changed)
		{
			value = editedValue;
			Save();
		}
	}

	public static void RedQtCheckbox(string qtName, string label)
	{
		bool qt = PromeSettings.Instance.GetQt(qtName);
		ImGui.PushStyleColor((ImGuiCol)0, RedText);
		bool changed = ImGui.Checkbox($"{label}###qt_{qtName}", ref qt);
		ImGui.PopStyleColor();
		if (changed)
		{
			PromeSettings.Instance.SetQt(qtName, qt);
			Save();
		}
	}

	public static void BoundCheckbox(string id, string label, bool value, Action<bool> onChanged, string description = "")
	{
		bool editedValue = value;
		if (PromeUi.ToggleRow(id, label, description ?? string.Empty, ref editedValue))
		{
			onChanged(editedValue);
		}
	}

	public static void BoundComboRow(string id, string label, int index, string[] items, Action<int> onChanged, string description = "")
	{
		if (items.Length != 0)
		{
			int editedIndex = ((index >= 0 && index < items.Length) ? index : 0);
			if (PromeUi.ComboRow(id, label, description ?? string.Empty, ref editedIndex, (IReadOnlyList<string>)items, 180f))
			{
				onChanged(editedIndex);
			}
		}
	}
}
