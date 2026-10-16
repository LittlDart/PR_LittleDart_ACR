using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Mitigation.Calc;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class ClemencyGcd : PldResolverBase
{
	protected override string ResolverDescription => "深仁厚泽（预计5秒内死亡自救）";

	protected override CheckResult CheckReal()
	{
		if (!PromeSettings.Instance.GetQt("深仁厚泽"))
		{
			return new CheckResult(false, "QT「深仁厚泽」没开");
		}
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return new CheckResult(false, "设置还没加载");
		}
		if (!ActionHelper.IsUnlocked(3541u))
		{
			return new CheckResult(false, "深仁厚泽未解锁");
		}
		IBattleChara me = Core.Me;
		if (me == null || ((ICharacter)me).CurrentMp < instance.ClemencyMinMp)
		{
			return new CheckResult(false, "MP 不足（或自己不在场）");
		}
		if (PldHelper.IsMovingForCast())
		{
			return new CheckResult(false, "移动中，读条会被打断");
		}
		if (PldHelper.IsRecentlyCast(3541u, 3000))
		{
			return new CheckResult(false, "刚放过深仁厚泽，避免连续读条");
		}
		if (!IsReadyAndInRange(3541u))
		{
			return new CheckResult(false, "深仁厚泽冷却中或不在射程内");
		}
		if (!SelfDeathPredictor.WillDieWithin(MitigationScheduler.Instance.State.Hp, instance.MitigationClemencyDeathSeconds, 3000))
		{
			return new CheckResult(false, $"预计不会在 {instance.MitigationClemencyDeathSeconds:F0} 秒内死亡");
		}
		return new CheckResult(true, "深仁厚泽：预计即将死亡，自救");
	}

	public override PAction GetAction()
	{
		return PldAction.Self(3541u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return AbilityGate.ReadyForNextGcd(actionId);
	}
}
