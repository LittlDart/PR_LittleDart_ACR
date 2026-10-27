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

	/// <summary>
	/// ★ 2026-10-16：「便宜组（90s 组）锁定」的**覆盖窗**（毫秒）。
	///
	/// <para>便宜组里最长的持续就是 铁壁 / 伪装 的 20s；覆盖窗 = 20s + 5s 余量。
	/// 过了这个窗口，<c>MitigationState.CommittedCombo</c> 这条锁定就过期，
	/// 组合选择器重新按「铁壁 / 小减任一就绪」现算 —— 否则 boss 房（从头到尾 InCombat）
	/// 会被第一次铁壁永久钉在 <c>PlanCheap</c> 上，那张表没有 Major 档，
	/// 后面的死刑永远轮不到星云 / 大星云。</para>
	/// </summary>
	public const int CheapComboCoverMs = 25000;

	public static int ClampPercentThreshold(int value)
	{
		return Math.Clamp(value, ThresholdPercentMin, ThresholdPercentMax);
	}
}
