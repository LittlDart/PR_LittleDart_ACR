namespace LittleDart.Common;

public static class AbilityGate
{
	private static TimeSnapshot _scopeSnapshot;

	private static bool _scopeActive;

	public static bool InDecisionScope => _scopeActive;

	public static TimeSnapshot Current
	{
		get
		{
			if (!_scopeActive)
			{
				return TimeSnapshot.Capture();
			}
			return _scopeSnapshot;
		}
	}

	public static void BeginDecisionScope()
	{
		_scopeSnapshot = TimeSnapshot.Capture();
		_scopeActive = true;
	}

	public static void EndDecisionScope()
	{
		_scopeActive = false;
	}

	public static bool ReadyForNextGcd(uint actionId)
	{
		return Current.WillBeReady(actionId);
	}

	public static bool ReadyNow(uint actionId)
	{
		return default(TimeSnapshot).ReadyNow(actionId);
	}
}
