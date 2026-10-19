using PromeRotation.Helpers;

namespace LittleDart.Paladin.GCD;

internal static class ConfiteorChain
{
	public const int ChainWindowMs = 5000;

	public static bool CanFollow(uint previous, uint current)
	{
		if (!ActionHelper.IsUnlocked(current))
		{
			return false;
		}
		if (!ActionHelper.RecentlyUsed(previous, 5000))
		{
			return false;
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(current))
		{
			return false;
		}
		return true;
	}
}
