using System;

namespace LittleDart.Common.Mitigation;

public sealed class MitigationThresholds
{
	public bool RequireTankStance = true;

	public int DamageWindowMs = 2500;

	public float EnemyCountRadius = 10f;

	public int WaveEndDebounceMs = 3000;

	public float DefaultNextPullSeconds = 45f;

	public int Phase1EndMs = 1000;

	public int Phase2StartMs = 2500;

	public int ReprisalMinEnemiesIn5m = 3;

	public int AoeWarnMs = 1500;

	public int ArmsLengthAggroOver = 3;

	public float TargetDeathSkipSeconds = 10f;

	public float TargetDeathSkipCooldownSeconds = 60f;

	public int ReserveMinCooldownSeconds = 20;

	public int MultiTargetEnemies = 3;

	public float StressEnemyWeight = 1f;

	public float StressDamageWeight = 2f;

	public float StressMidThreshold = 4f;

	public float StressHighThreshold = 8f;

	public float InvulnThresholdPercent = 70f;

	public int ScarceBigThresholdPercent = 25;

	public int BigThresholdPercent = 35;

	public int InvulnHpPercent = 25;

	public int ScarceBigAt => MitigationDefaults.ClampPercentThreshold(ScarceBigThresholdPercent);

	public int BigAt => Math.Max(MitigationDefaults.ClampPercentThreshold(BigThresholdPercent), ScarceBigAt);

	public int InvulnHpAt => MitigationDefaults.ClampPercentThreshold(InvulnHpPercent);
}
