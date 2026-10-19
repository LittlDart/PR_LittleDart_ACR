using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.DalamudServices;

namespace LittleDart.Common.Mitigation.Calc;

public static class EnemyCounter
{
	private const float AggroScanRadius = 25f;

	public static int CountWithin(MitigationJobProfile profile, float radius)
	{
		return profile.NearbyEnemyCount(radius);
	}

	public static int CountAggroOnMe(MitigationJobProfile profile)
	{
		try
		{
			IBattleChara me = profile.Me();
			if (me == null)
			{
				return 0;
			}
			ulong myEntityId = ((IGameObject)me).EntityId;
			int aggroCount = 0;
			foreach (IGameObject item in (IEnumerable<IGameObject>)Svc.Objects)
			{
				IBattleChara enemy = (IBattleChara)(object)((item is IBattleChara) ? item : null);
				if (enemy != null && profile.IsEnemy(enemy) && !(profile.DistanceTo(enemy) > 25f) && ((IGameObject)enemy).TargetObjectId == myEntityId)
				{
					aggroCount++;
				}
			}
			return aggroCount;
		}
		catch
		{
			return profile.HasTankStance() ? CountWithin(profile, 10f) : 0;
		}
	}

	public static IBattleChara? PickPullTarget(MitigationJobProfile profile, float range)
	{
		try
		{
			IBattleChara me = profile.Me();
			if (me == null)
			{
				return null;
			}
			ulong myEntityId = ((IGameObject)me).EntityId;
			IBattleChara bestTarget = null;
			float bestDistance = float.MaxValue;
			int bestPriority = 0;
			foreach (IGameObject item in (IEnumerable<IGameObject>)Svc.Objects)
			{
				IBattleChara enemy = (IBattleChara)(object)((item is IBattleChara) ? item : null);
				if (enemy == null || !profile.IsEnemy(enemy))
				{
					continue;
				}
				bool notOnMe = ((IGameObject)enemy).TargetObjectId != myEntityId;
				bool outOfCombat = (((ICharacter)enemy).StatusFlags & StatusFlags.InCombat) == 0;
				if (notOnMe || outOfCombat)
				{
					int priority = ((!outOfCombat) ? 1 : 2);
					float distance = profile.DistanceTo(enemy);
					if (!(distance > 25f) && (!(range > 0f) || !(distance > range)) && (priority > bestPriority || (priority == bestPriority && distance < bestDistance)))
					{
						bestTarget = enemy;
						bestPriority = priority;
						bestDistance = distance;
					}
				}
			}
			return bestTarget;
		}
		catch
		{
			return null;
		}
	}
}
