using LittleDart.Common.Ui;

namespace LittleDart.Gunbreaker.Setting;

internal static class GnbDeckWindow
{
	private static LittleDartDeckWindow? _instance;

	internal static LittleDartDeckWindow Instance => _instance ??= new LittleDartDeckWindow(new DeckContext
	{
		WindowId = "littledart_gnb_deck",
		JobLabel = "绝枪战士",
		JobShort = "GNB",
		Title = "// CONTROL DECK · GNB",
		IsDailyMode = GnbHelper.IsDailyMode,
		ToggleMode = GnbSettingUi.ToggleMode,
		OpenSettings = GnbWindowSwitcher.ShowSelected,
		AutoPullEnabled = () => GnbAutoPull.IsEnabled,
		SetAutoPull = GnbAutoPull.SetEnabled,
	});

	internal static void Register()
	{
		Instance.Register();
	}

	internal static void Unregister()
	{
		_instance?.Unregister();
	}

	
	internal static void ApplyVisibility()
	{
		if (InterfaceSettings.UseShellWindow)
		{
			
			Instance.Register();
			Instance.IsOpen = true;
			return;
		}
		if (_instance != null)
		{
			_instance.IsOpen = false;
		}
	}
}
