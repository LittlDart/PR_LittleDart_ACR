using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;

namespace LittleDart.Common.Mitigation.Calc;

public static class EnemyAoeDetector
{
	public static bool IsIncomingAoe(MitigationJobProfile profile, int warnMs)
	{
		IBattleChara enemy = profile.CurrentTarget();
		if (enemy == null || !enemy.IsCasting)
		{
			return false;
		}
		if ((enemy.TotalCastTime - enemy.CurrentCastTime) * 1000f < (float)warnMs && PromeBattleData.AoeActions.Contains(enemy.CastActionId))
		{
			return true;
		}
		return PromeBattleData.AoeActions.Contains(enemy.CastActionId);
	}
}
