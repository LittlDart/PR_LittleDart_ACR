using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class FofOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：战逃反应";

	protected override CheckResult CheckReal()
	{
		if (!PromeSettings.Instance.GetQt("战逃反应"))
		{
			return new CheckResult(false, "QT「战逃反应」没开");
		}
		if (PromeSettings.Instance.GetQt("延后战逃"))
		{
			return new CheckResult(false, "QT「延后战逃」开着，不自动开");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(20u))
		{
			return new CheckResult(false, "战逃反应还没解锁");
		}
		if (!ActionHelper.CanCast(20u, (IBattleChara)null) || ActionHelper.RecentlyUsed(20u, 500))
		{
			return new CheckResult(false, "战逃反应没冷却好 / 刚放过");
		}
		if (PromeSettings.Instance.GetQt("智能远战逃") && PldHelper.IsTargetFar())
		{
			return new CheckResult(false, "智能远战逃：离得远，先让远程 GCD 用掉这格");
		}
		return new CheckResult(true, "战逃反应可用，开爆发");
	}

	public override PAction GetAction()
	{
		return PldAction.Self(20u, (ActionType)1);
	}
}
