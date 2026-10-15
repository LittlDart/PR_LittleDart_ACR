namespace LittleDart.Paladin.GCD;

internal static class FofQueue
{
	public static bool IsInFofWindow()
	{
		if (PldHelper.HasValidTarget() && PldHelper.HasCombo())
		{
			return PldHelper.IsFofActive();
		}
		return false;
	}
}
