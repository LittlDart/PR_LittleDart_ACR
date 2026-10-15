namespace LittleDart.Common.Mitigation;

public static class MitigationSignals
{
	public static IBossTimer BossTimer { get; set; } = new UnknownBossTimer();

	public static double? ManualSecondsToBoss { get; set; }

	public static double? GetSecondsToBoss()
	{
		return ManualSecondsToBoss ?? BossTimer.GetSecondsToBoss();
	}

	public static void ClearManual()
	{
		ManualSecondsToBoss = null;
	}

	public static void SetManualSecondsToBoss(double? seconds)
	{
		ManualSecondsToBoss = seconds;
	}
}
