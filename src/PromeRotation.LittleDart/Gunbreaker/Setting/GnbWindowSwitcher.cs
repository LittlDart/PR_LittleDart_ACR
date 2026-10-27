using LittleDart.Common.Ui;

namespace LittleDart.Gunbreaker.Setting;

internal static class GnbWindowSwitcher
{
	
	public static void SwitchTo(bool shell)
	{
		InterfaceSettings settings = InterfaceSettings.Instance;
		settings.WindowKind = shell ? 1 : 0;
		settings.Save();
		if (shell)
		{
			GnbSettingWindow.Instance.IsOpen = false;
			GnbShellWindow.Register();
			GnbShellWindow.Instance.IsOpen = true;
		}
		else
		{
			GnbShellWindow.Instance.IsOpen = false;
			GnbSettingWindow.Register();
			GnbSettingWindow.Instance.IsOpen = true;
		}
		
		GnbDeckWindow.ApplyVisibility();
		PromeHostTakeover.Refresh();
	}

	
	public static void ShowSelected()
	{
		SwitchTo(InterfaceSettings.UseShellWindow);
	}
}
