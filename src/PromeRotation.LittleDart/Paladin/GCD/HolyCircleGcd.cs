using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class HolyCircleGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体：圣环（需强化 buff，权重低于悔罪连）";

	protected override CheckResult CheckReal()
	{
		// ★★ 2026-10-16 玩家口径：AOE 轴**不看当前选定的目标**。
		//   圣环 16458 的 Action 表实测：Range = 0、EffectRange = 5、CastType = 2、
		//   CanTargetSelf = 是、CanTargetHostile = 否 ⇒ **自身中心 AOE，不需要选中目标**。
		if (!AoeQueue.ShouldUseAoeIgnoringTarget())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 自身 5 米内能打的怪不够）");
		}
		if (!ActionHelper.IsUnlocked(16458u))
		{
			return new CheckResult(false, "圣环未解锁（72 级后才有）");
		}
		if (!PldHelper.HasDivineMight())
		{
			return new CheckResult(false, "没有「神圣魔法效果提高」，圣环会被读条连打");
		}
		if (!IsReadyAndInRange(16458u))
		{
			return new CheckResult(false, "圣环冷却中或身前没有能打的怪");
		}
		return new CheckResult(true, "群体：圣环（强化 buff 中）");
	}

	public override PAction GetAction()
	{
		// 圣环 16458 是自身中心（Range = 0）⇒ 不需要给「打哪只」，保持原样。
		return PldAction.Of(16458u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		// ★ 2026-10-16：换成 AOE 版（AOE 成立时按 5 米内最近的一只判射程）。
		return PldHelper.IsReadyAndInRangeForNextGcdAoe(actionId);
	}
}
