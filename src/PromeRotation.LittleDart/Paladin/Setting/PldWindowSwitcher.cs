using LittleDart.Common.Ui;

namespace LittleDart.Paladin.Setting;

internal static class PldWindowSwitcher
{
	
	public static void SwitchTo(bool shell)
	{
		InterfaceSettings settings = InterfaceSettings.Instance;
		settings.WindowKind = shell ? 1 : 0;
		settings.Save();
		if (shell)
		{
			PldSettingWindow.Instance.IsOpen = false;
			PldShellWindow.Register();
			PldShellWindow.Instance.IsOpen = true;
		}
		else
		{
			PldShellWindow.Instance.IsOpen = false;
			PldSettingWindow.Register();
			PldSettingWindow.Instance.IsOpen = true;
		}
		
		PldDeckWindow.ApplyVisibility();
		PromeHostTakeover.Refresh();
	}

	
	public static void ShowSelected()
	{
		SwitchTo(InterfaceSettings.UseShellWindow);
	}
}
