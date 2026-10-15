using System;

namespace LittleDart.Common.Mitigation;

public static class MitigationDefaults
{
	public const float EnemyCountRadius = 10f;

	public const int EnemySampleIntervalMs = 250;

	public const int HpSampleIntervalMs = 200;

	public const int DamageWindowMs = 2500;

	public const int WaveEndDebounceMs = 3000;

	public const int EvalThrottleMs = 80;

	public const int InvulnHpPercent = 25;

	public const int ThresholdPercentMin = 20;

	public const int ThresholdPercentMax = 50;

	public const int ReprisalMinEnemiesIn5m = 3;

	public const int AoeWarnMs = 4000;

	public const double TargetDeathSkipSeconds = 10.0;

	public const double TargetDeathSkipCooldownSeconds = 60.0;

	public const double SelfHealDeathSeconds = 5.0;

	public const int LowPressureMaxEnemies = 3;

	public const int FlavorMinEnemies = 2;

	public const int FlavorSelfHpPercent = 80;

	public static int ClampPercentThreshold(int value)
	{
		return Math.Clamp(value, ThresholdPercentMin, ThresholdPercentMax);
	}
}
