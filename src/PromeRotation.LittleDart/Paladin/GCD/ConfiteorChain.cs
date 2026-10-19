using System;
using LittleDart.Paladin.Skill;
using PromeRotation.Helpers;

namespace LittleDart.Paladin.GCD;

internal static class ConfiteorChain
{
	
	private const double BaselineTimeoutSeconds = 30.0;

	
	private static int _baselineStacks = -1;

	
	private static DateTime _baselineAt = DateTime.MinValue;

	
	
	
	
	
	public static void NoteChainHeadDecision()
	{
		_baselineStacks = PldHelper.GetAuraStack(PldSkills.安魂祈祷);
		_baselineAt = DateTime.UtcNow;
	}

	
	
	
	
	public static bool CanFollow(int stackOffset, uint current)
	{
		if (_baselineStacks < 0)
		{
			return false;
		}
		if ((DateTime.UtcNow - _baselineAt).TotalSeconds > BaselineTimeoutSeconds)
		{
			return false;
		}
		if (!ActionHelper.IsUnlocked(current))
		{
			return false;
		}
		if (!PldHelper.HasValidTarget())
		{
			return false;
		}
		if (PldHelper.GetAuraStack(PldSkills.安魂祈祷) != _baselineStacks - stackOffset)
		{
			return false;
		}
		return PldHelper.IsReadyAndInRangeForNextGcd(current);
	}

	
	public static void Reset()
	{
		_baselineStacks = -1;
		_baselineAt = DateTime.MinValue;
	}
}
