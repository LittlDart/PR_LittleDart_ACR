using System;
using System.Collections.Generic;

namespace LittleDart.Common;

public enum TargetProfile
{
	Unknown = 0,

	SingleBoss = 1,

	Pack = 2,
}

public static class TargetClassifier
{
	public const float EngageRadius = 15f;

	public const double BossSurvivesSeconds = 30.0;

	public const int PackEnemyCount = 3;

	public static TargetProfile Classify(int enemyCount, Func<double, bool> targetSurvivesLongerThan)
	{
		if (enemyCount <= 0)
		{
			return TargetProfile.Unknown;
		}
		if (enemyCount >= PackEnemyCount)
		{
			return TargetProfile.Pack;
		}
		bool survives;
		try
		{
			survives = targetSurvivesLongerThan(BossSurvivesSeconds);
		}
		catch
		{
			return TargetProfile.Unknown;
		}
		return survives ? TargetProfile.SingleBoss : TargetProfile.Pack;
	}

	public static string Describe(TargetProfile profile)
	{
		return profile switch
		{
			TargetProfile.SingleBoss => "单体 boss",
			TargetProfile.Pack => "群怪",
			_ => "判不出（未进战 / 无目标 / 血量速率未采到）",
		};
	}

	public static string DescribeWithInputs(TargetProfile profile, int enemyCount, bool? survives30s)
	{
		string survivesText = survives30s switch
		{
			true => $"活得 > {BossSurvivesSeconds:F0}s",
			false => $"活不到 {BossSurvivesSeconds:F0}s",
			_ => "存活时间读不出（按「活得久」算）",
		};
		return $"目标形态={Describe(profile)}（交战半径 {EngageRadius:F0}m 内 {enemyCount} 只；{survivesText}）";
	}

	public static IReadOnlyList<string> KnownLimitations { get; } = new[]
	{
		"双目标 boss 战（boss + 常驻小弟）会被判成群怪 —— 因为判据只看数量与存活时间，不看 rank。",
		"拉怪途中（怪还在往你这跑）会被判成群怪 —— 这正是玩家想要的「拉怪堆」语义。",
		"刚进战的前 2.5 秒血量速率还没采够 ⇒ 可能先返回 Unknown 再变成 SingleBoss。",
	};
}
