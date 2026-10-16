using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class ReadCastHolySpiritRangedGcd : PldResolverBase
{
	protected override string ResolverDescription => "远离：读条圣灵（站定才放）";

	protected override CheckResult CheckReal()
	{
		if (!PromeSettings.Instance.GetQt("远离圣灵"))
		{
			return new CheckResult(false, "QT「远离圣灵」没开");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(7384u))
		{
			return new CheckResult(false, "圣灵未解锁（64 级后才有）");
		}
		if (!PldHelper.IsTargetFar())
		{
			return new CheckResult(false, "目标在近战范围内，交给近战基础连招");
		}
		if (!PldHelper.PassesRangedHolySpiritGate())
		{
			return new CheckResult(false, "没到「圣灵释放阈值」");
		}
		if (PldHelper.HasDivineMight())
		{
			return new CheckResult(false, "有「神圣魔法效果提高」，交给上一档瞬发圣灵");
		}
		if (PldHelper.IsMovingForCast())
		{
			return new CheckResult(false, "移动中，读条会被打断");
		}
		IBattleChara me = Core.Me;
		if (me == null || ((ICharacter)me).CurrentMp < PldSettings.Instance.HolySpiritMinMp)
		{
			return new CheckResult(false, "MP 不足（或自己不在场）");
		}
		if (!IsReadyAndInRange(7384u))
		{
			return new CheckResult(false, "圣灵冷却中或不在射程内");
		}
		return new CheckResult(true, "远离：读条圣灵（站定）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(7384u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
