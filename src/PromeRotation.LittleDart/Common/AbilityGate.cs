using PromeRotation.Core;
using PromeRotation.Extensions;
using PromeRotation.Helpers;

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

	
	
	
	
	public const float ReadyNowEpsilonSeconds = 0.05f;

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static bool TryGetReadyActionId(uint actionId, out uint adjusted)
	{
		adjusted = ActionHelper.GetAdjustedActionId(actionId);
		if (adjusted == 0 || !ActionHelper.IsActionAvailableByLevelAndQuest(adjusted) || Core.Me == null || GameData.IsPlayerOccupied() || ActionHelper.RecentlyUsed(adjusted, 300))
		{
			return false;
		}
		return true;
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static bool IsReady(uint actionId, float cooldownWindowSeconds = 0f, float minCharges = 1f)
	{
		if (!TryGetReadyActionId(actionId, out uint adjusted))
		{
			return false;
		}
		if (ActionHelper.GetMaxCharges(adjusted) > 1)
		{
			return HasCharges(adjusted, minCharges);
		}
		return ActionHelper.GetActionCooldown(adjusted) <= cooldownWindowSeconds;
	}

	
	
	
	
	
	
	
	
	public static bool HasCharges(uint actionId, float minCharges = 1f)
	{
		if (!TryGetReadyActionId(actionId, out uint adjusted))
		{
			return false;
		}
		return SkillExtensions.GetActionCharges(adjusted) >= minCharges;
	}
}
