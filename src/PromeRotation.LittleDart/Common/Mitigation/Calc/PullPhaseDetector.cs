namespace LittleDart.Common.Mitigation.Calc;

public static class PullPhaseDetector
{
	public static bool IsPulling(int enemiesIn5m, int minEnemiesIn5m)
	{
		return enemiesIn5m < minEnemiesIn5m;
	}
}
