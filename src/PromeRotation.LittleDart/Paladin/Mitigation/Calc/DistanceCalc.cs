namespace LittleDart.Paladin.Mitigation.Calc;

public static class DistanceCalc
{
	public static float ToTarget()
	{
		if (PldHelper.GetCurrentTarget() != null)
		{
			return PldHelper.GetDistanceToTarget();
		}
		return float.MaxValue;
	}

	public static bool InRange(float min, float max)
	{
		float distance = ToTarget();
		if (distance >= min)
		{
			return distance <= max;
		}
		return false;
	}
}
