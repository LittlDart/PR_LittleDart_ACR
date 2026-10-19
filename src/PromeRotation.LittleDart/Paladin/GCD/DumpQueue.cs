using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;

namespace LittleDart.Paladin.GCD;

internal static class DumpQueue
{
	public static bool IsEnabled()
	{
		if (PromeSettings.Instance.GetQt(PldQt.DumpResource))
		{
			return PldHelper.HasValidTarget();
		}
		return false;
	}

	public static bool HasMpForHolySpirit()
	{
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return ((ICharacter)me).CurrentMp >= PldSettings.Instance.HolySpiritMinMp;
		}
		return false;
	}
}
