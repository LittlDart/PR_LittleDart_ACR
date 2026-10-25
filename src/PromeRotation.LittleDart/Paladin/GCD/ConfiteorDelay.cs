using PromeRotation.Data;

namespace LittleDart.Paladin.GCD;

public static class ConfiteorDelay
{
	private static int _remainingGcds;

	public static bool IsDelayed()
	{
		if (!PromeSettings.Instance.GetQt(PldQt.DelayConfiteor))
		{
			return false;
		}
		return _remainingGcds > 0;
	}

	public static void StartDelay()
	{
		if (PromeSettings.Instance.GetQt(PldQt.DelayConfiteor))
		{
			_remainingGcds = 2;
		}
	}

	public static void Tick()
	{
		if (_remainingGcds > 0)
		{
			_remainingGcds--;
		}
	}

	public static void Reset()
	{
		_remainingGcds = 0;
	}
}
