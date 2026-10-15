using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Paladin;

public static class PldAction
{
	public static PAction Of(uint actionId, ActionType type)
	{
		ActionTargetType resolvedTarget = default;
		ActionTargetType targetType = (ActionTargetType)((!ActionHelper.TryResolveActionTargetType(actionId, out resolvedTarget)) ? 1 : ((int)resolvedTarget));
		return new PAction(actionId, type, targetType);
	}

	public static PAction Self(uint actionId, ActionType type)
	{
		return new PAction(actionId, type, (ActionTargetType)0);
	}

	public static PAction Enemy(uint actionId, ActionType type)
	{
		return new PAction(actionId, type, (ActionTargetType)1);
	}

	public static PAction On(uint actionId, ActionType type, ActionTargetType target)
	{
		return new PAction(actionId, type, target);
	}
}
