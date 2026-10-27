using PromeRotation.Core;

namespace LittleDart.Common.Mitigation;

public static class MitigationBossRoom
{
	public static bool? Probe()
	{
		try
		{
#pragma warning disable PR_EXPERIMENTAL
			return GameData.IsInBossRoom();
#pragma warning restore PR_EXPERIMENTAL
		}
		catch
		{
			return null;
		}
	}
}
