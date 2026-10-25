using System;

namespace LittleDart.Common.Mitigation;

public sealed class MitigationThresholds
{
	public bool RequireTankStance = true;

	public int DamageWindowMs = 2500;

	public float EnemyCountRadius = 10f;

	public int WaveEndDebounceMs = 3000;

	public int ReprisalMinEnemiesIn5m = 3;

	public int AoeWarnMs = MitigationDefaults.AoeWarnMs;

	public float TargetDeathSkipSeconds = 10f;

	public float TargetDeathSkipCooldownSeconds = 60f;

	public int InvulnHpPercent = 25;

	public int InvulnHpAt => MitigationDefaults.ClampPercentThreshold(InvulnHpPercent);
}
