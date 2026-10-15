using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class HolyCircleGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体：圣环（需强化 buff，权重低于悔罪连）";

	protected override CheckResult CheckReal()
	{
		if (!AoeQueue.ShouldUseAoe())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 没有目标 / 敌人数量不够）");
		}
		if (!ActionHelper.IsUnlocked(16458u))
		{
			return new CheckResult(false, "圣环未解锁（72 级后才有）");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "目标不在近战范围内");
		}
		if (!PldHelper.HasDivineMight())
		{
			return new CheckResult(false, "没有「神圣魔法效果提高」，圣环会被读条连打");
		}
		if (!IsReadyAndInRange(16458u))
		{
			return new CheckResult(false, "圣环冷却中或不在射程内");
		}
		return new CheckResult(true, "群体：圣环（强化 buff 中）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(16458u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
