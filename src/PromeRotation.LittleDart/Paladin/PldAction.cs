using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Paladin;

public static class PldAction
{
	public static PAction Of(uint actionId, ActionType type)
	{
		
		
		if (!ActionHelper.TryResolveActionTargetType(actionId, out ActionTargetType resolvedTarget))
		{
			resolvedTarget = ActionTargetType.Target;
		}
		return new PAction(actionId, type, resolvedTarget);
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public static PAction Aoe(uint actionId, ActionType type, IBattleChara? target)
	{
		PAction action = Of(actionId, type);
		if (target == null || action.Target != ActionTargetType.Target)
		{
			return action;
		}
		return new PAction(actionId, type, ActionTargetType.Self)
		{
			NetworkTid = ((IGameObject)target).EntityId
		};
	}

	public static PAction Self(uint actionId, ActionType type)
	{
		return new PAction(actionId, type, ActionTargetType.Self);
	}

	public static PAction Enemy(uint actionId, ActionType type)
	{
		return new PAction(actionId, type, ActionTargetType.Target);
	}

	public static PAction On(uint actionId, ActionType type, ActionTargetType target)
	{
		return new PAction(actionId, type, target);
	}
}
