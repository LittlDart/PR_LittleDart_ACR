using System;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;

namespace LittleDart.Paladin.GCD;

internal static class AoeQueue
{
	private static long _enemyCountTick = long.MinValue;

	private static int _enemyCountCache;

	public static bool ShouldUseAoe()
	{
		if (!PromeSettings.Instance.GetQt("AOE"))
		{
			return false;
		}
		if (!PldHelper.HasValidTarget())
		{
			return false;
		}
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return false;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount != _enemyCountTick)
		{
			_enemyCountTick = tickCount;
			_enemyCountCache = PldHelper.GetNearbyEnemyCount((int)instance.AoeRadius);
		}
		return _enemyCountCache >= instance.AoeEnemyCount;
	}
}
