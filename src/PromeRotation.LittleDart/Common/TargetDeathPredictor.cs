namespace LittleDart.Common;

public static class TargetDeathPredictor
{
	public static bool TryPredictSecondsToDeath(HpTracker hp, int windowMs, out double seconds)
	{
		seconds = double.PositiveInfinity;
		float hpFraction = hp.TargetHpPercent();
		if (hpFraction < 0f)
		{
			return false;
		}
		float declinePerSecond = hp.TargetNetDeclinePerSecond(windowMs);
		if (declinePerSecond <= 0.01f)
		{
			return false;
		}
		seconds = hpFraction * 100f / declinePerSecond;
		return true;
	}

	public static bool WillDieWithin(HpTracker hp, double seconds, int windowMs)
	{
		if (TryPredictSecondsToDeath(hp, windowMs, out var predictedSeconds))
		{
			return predictedSeconds <= seconds;
		}
		return false;
	}
}
