using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Core;

namespace LittleDart.Common.Mitigation.Calc;

public static class SelfDeathPredictor
{
	public static bool TryPredictSecondsToDeath(HpTracker hp, int windowMs, out double seconds)
	{
		seconds = double.PositiveInfinity;
		float declinePerSecond = hp.SelfNetDeclinePerSecond(windowMs);
		if (declinePerSecond <= 0.01f)
		{
			return false;
		}
		float hpFraction = CurrentSelfHpPercent();
		if (hpFraction < 0f)
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

	private static float CurrentSelfHpPercent()
	{
		try
		{
			IBattleChara me = Core.Me;
			if (me == null)
			{
				return -1f;
			}
			ICharacter meCharacter = (ICharacter)(object)me;
			if (meCharacter.MaxHp == 0)
			{
				return -1f;
			}
			return (float)meCharacter.CurrentHp / (float)meCharacter.MaxHp;
		}
		catch
		{
			return -1f;
		}
	}
}
