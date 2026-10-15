using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;

namespace LittleDart.Common.Mitigation.Calc;

public static class DeathSentenceDetector
{
	public static bool IsCastingDeathSentence(MitigationJobProfile profile)
	{
		IBattleChara target = profile.CurrentTarget();
		if (target == null || !target.IsCasting)
		{
			return false;
		}
		return PromeBattleData.TankDeathActions.Contains(target.CastActionId);
	}
}
