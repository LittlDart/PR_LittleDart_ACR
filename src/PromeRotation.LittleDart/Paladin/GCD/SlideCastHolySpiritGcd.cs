using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class SlideCastHolySpiritGcd : PldResolverBase
{
	protected override string ResolverDescription => "远离：滑步圣灵（等站定后读条）";

	protected override CheckResult CheckReal()
	{
		if (!PldSlideCast.IsEnabled)
		{
			return new CheckResult(false, "滑步模式没生效（QT「滑步模式」没开，或当前是日常模式）");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(7384u))
		{
			return new CheckResult(false, "圣灵未解锁（64 级后才有）");
		}
		if (PldHelper.HasDivineMight())
		{
			return new CheckResult(false, "有「神圣魔法效果提高」，交给上一档瞬发圣灵");
		}
		if (!PldHelper.IsTargetFar())
		{
			return new CheckResult(false, "目标在近战范围内，交给近战基础连招");
		}
		if (!PldHelper.PassesRangedHolySpiritGate())
		{
			return new CheckResult(false, "没到「圣灵释放阈值」");
		}
		IBattleChara me = Core.Me;
		if (me == null || ((ICharacter)me).CurrentMp < PldSettings.Instance.HolySpiritMinMp)
		{
			return new CheckResult(false, "MP 不足（或自己不在场）");
		}
		if (!PldSlideCast.CanSlideCast)
		{
			return new CheckResult(false, "滑步模式：玩家还在移动（PR 无移动锁，等他自己停下）");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(7384u))
		{
			return new CheckResult(false, "圣灵不在可施放状态（冷却中 / 够不着 / 被占用）");
		}
		return new CheckResult(true, "远离：滑步圣灵（已站定）");
	}

	public override PAction GetAction()
	{
		PldSlideCast.RequestCast();
		PldSlideCast.NotifyCasting();
		return PldAction.Of(7384u, (ActionType)0);
	}
}
