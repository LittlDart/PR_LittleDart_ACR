using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace LittleDart.Common.Ui;

public static class ThemeSection
{
	
	
	
	
	public static void Draw(Action<bool> switchWindow)
	{
		InterfaceSettings settings = InterfaceSettings.Instance;

		
		
		List<string> names = new List<string> { "原版" };
		foreach (UiThemeDefinition set in ThemeCatalog.All)
		{
			names.Add(set.Name);
		}

		int index = settings.WindowKind == 1 ? 1 + ThemeCatalog.IndexOf(settings.ThemeId) : 0;
		if (UiKit.ComboRow("ld_ui_set", "主题选项", string.Empty, ref index, names, 280f))
		{
			if (index <= 0)
			{
				settings.WindowKind = 0;
				settings.Save();
				switchWindow(false);
			}
			else
			{
				settings.WindowKind = 1;
				settings.ThemeId = ThemeCatalog.All[index - 1].Id;
				settings.Save();
				switchWindow(true);
			}
		}
	}
}
