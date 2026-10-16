using LittleDart.Common;
using LittleDart.Common.Mitigation.Calc;

namespace LittleDart.Paladin.Mitigation.Calc;

public static class SelfDeathPredictor
{
	public static bool TryPredictSecondsToDeath(HpTracker hp, int windowMs, out double seconds)
	{
		return LittleDart.Common.Mitigation.Calc.SelfDeathPredictor.TryPredictSecondsToDeath(hp, windowMs, out seconds);
	}

	public static bool WillDieWithin(HpTracker hp, double seconds, int windowMs)
	{
		return LittleDart.Common.Mitigation.Calc.SelfDeathPredictor.WillDieWithin(hp, seconds, windowMs);
	}
}
