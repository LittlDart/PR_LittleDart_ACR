using LittleDart.Common.Ui;

namespace LittleDart.Paladin.Setting;

internal static class PldDeckWindow
{
	private static LittleDartDeckWindow? _instance;

	internal static LittleDartDeckWindow Instance => _instance ??= new LittleDartDeckWindow(new DeckContext
	{
		WindowId = "littledart_pld_deck",
		JobLabel = "骑士",
		JobShort = "PLD",
		Title = "// CONTROL DECK · PLD",
		IsDailyMode = PldHelper.IsDailyMode,
		ToggleMode = PldSettingUi.ToggleMode,
		OpenSettings = PldWindowSwitcher.ShowSelected,
		AutoPullEnabled = () => PldAutoPull.IsEnabled,
		SetAutoPull = PldAutoPull.SetEnabled,
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
