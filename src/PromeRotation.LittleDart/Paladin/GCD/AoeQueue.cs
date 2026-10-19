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
		return PldHelper.HasValidTarget() && QtAndEnemyCountOk();
	}

	
	
	
	
	
	
	
	
	
	public static bool ShouldUseAoeIgnoringTarget()
	{
		return QtAndEnemyCountOk() && PldHelper.GetNearestEnemy(PldHelper.AoeEffectRange) != null;
	}

	
	
	
	
	
	
	
	public static void Tick()
	{
		if (!PromeSettings.Instance.GetQt(PldQt.UseAoe))
		{
			return;
		}
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount != _enemyCountTick)
		{
			_enemyCountTick = tickCount;
			_enemyCountCache = PldHelper.GetNearbyEnemyCount((int)instance.AoeRadius);
		}
	}

	private static bool QtAndEnemyCountOk()
	{
		if (!PromeSettings.Instance.GetQt(PldQt.UseAoe))
		{
			return false;
		}
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return false;
		}
		return _enemyCountCache >= instance.AoeEnemyCount;
	}
}
