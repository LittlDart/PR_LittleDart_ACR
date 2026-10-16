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
		// ★ 2026-10-16 玩家口径：战逃反应 20 是**自身 buff**（Action 表 Range = 0 / EffectRange = 0），
		//   不该被「玩家没选目标」卡住。原来那一句是 `HasValidTarget()`（**没有**近战要求）
		//   ⇒ 换成 HasTargetOrAoeCluster()：AOE 成立时「5 米内有能打的怪」也算数，
		//   其余情况与老判据逐字等价（**只放宽、不收紧**）。
		if (!PldHelper.HasTargetOrAoeCluster())
		{
			return new CheckResult(false, "眼前没有能打的怪");
		}
		if (!ActionHelper.IsUnlocked(20u))
		{
			return new CheckResult(false, "战逃反应还没解锁");
		}
		if (!ActionHelper.CanCast(20u, (IBattleChara)null) || ActionHelper.RecentlyUsed(20u, 500))
		{
			return new CheckResult(false, "战逃反应没冷却好 / 刚放过");
		}
		// 「智能远战逃」是「目标离得远就先让远程 GCD 用掉这格」——AOE 成立时玩家已经站在人堆里，
		// 那条判据（读当前选定目标）不适用，跳过它。
		if (PromeSettings.Instance.GetQt("智能远战逃") && !PldHelper.IsAoeClusterMet() && PldHelper.IsTargetFar())
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
