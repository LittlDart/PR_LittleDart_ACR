using System;

namespace LittleDart.Common.Mitigation;

public static class MitigationDefaults
{
	public const int DefaultNextPullSeconds = 45;

	public const double MinPullIntervalSeconds = 3.0;

	public const double MaxPullIntervalSeconds = 180.0;

	public const int IntervalHistoryCount = 3;

	public const float EnemyCountRadius = 10f;

	public const int EnemySampleIntervalMs = 250;

	public const int HpSampleIntervalMs = 200;

	public const int DamageWindowMs = 2500;

	public const int Phase1EndMs = 1000;

	public const int Phase2StartMs = 2500;

	public const int WaveEndDebounceMs = 3000;

	public const int ReentryGuardMs = 1500;

	public const int EvalThrottleMs = 80;

	public const float StressEnemyWeight = 1f;

	public const float StressDamageWeight = 2f;

	public const float StressMidThreshold = 4f;

	public const float StressHighThreshold = 8f;

	public const float InvulnThresholdPercent = 70f;

	public const int ThresholdPercentMin = 10;

	public const int ThresholdPercentMax = 35;

	public const int ScarceBigThresholdPercent = 25;

	public const int BigThresholdPercent = 35;

	public const int InvulnHpPercent = 25;

	public static readonly ReserveRequirement ReserveLow = new ReserveRequirement(0, 1, 1);

	public static readonly ReserveRequirement ReserveMid = new ReserveRequirement(1, 1, 1);

	public static readonly ReserveRequirement ReserveHigh = new ReserveRequirement(2, 1, 1);

	public const int ReprisalMinEnemiesIn5m = 3;

	public const int AoeWarnMs = 1500;

	public const int ArmsLengthAggroOnMeOver = 3;

	public const int ReserveMinCooldownSeconds = 20;

	public const int MultiTargetEnemies = 3;

	public const double TargetDeathSkipSeconds = 10.0;

	public const double TargetDeathSkipCooldownSeconds = 60.0;

	public const double SelfHealDeathSeconds = 5.0;

	public static ReserveRequirement MinReserve(PressureBand band)
	{
		return band switch
		{
			PressureBand.High => ReserveHigh, 
			PressureBand.Mid => ReserveMid, 
			_ => ReserveLow, 
		};
	}

	public static int ClampPercentThreshold(int value)
	{
		return Math.Clamp(value, 10, 35);
	}
}
