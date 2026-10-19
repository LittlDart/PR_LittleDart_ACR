using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class HolyCircleGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体：圣环（需强化 buff，权重低于悔罪连）";

	protected override CheckResult CheckReal()
	{
		
		
		
		if (!AoeQueue.ShouldUseAoeIgnoringTarget())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 自身 5 米内能打的怪不够）");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.圣环))
		{
			return new CheckResult(false, "圣环未解锁（72 级后才有）");
		}
		if (!PldHelper.HasDivineMight())
		{
			return new CheckResult(false, "没有「神圣魔法效果提高」，圣环会被读条连打");
		}
		if (!IsReadyAndInRange(PldSkills.圣环))
		{
			return new CheckResult(false, "圣环冷却中或身前没有能打的怪");
		}
		return new CheckResult(true, "群体：圣环（强化 buff 中）");
	}

	public override PAction GetAction()
	{
		
		return PldAction.Of(PldSkills.圣环, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		
		return PldHelper.IsReadyAndInRangeForNextGcdAoe(actionId);
	}
}
