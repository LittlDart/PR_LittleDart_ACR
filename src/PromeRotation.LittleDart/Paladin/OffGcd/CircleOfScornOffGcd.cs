using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class CircleOfScornOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：厄运流转（双伤 1）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(23u))
		{
			return new CheckResult(false, "厄运流转还没解锁");
		}
		if (!TwinDamage.ShouldUse())
		{
			return new CheckResult(false, "双伤的两个锁没过（战逃/绝对统治冷却不在窗口）或不在近战范围");
		}
		if (!TwinDamage.ReadyWithCanCast(23u))
		{
			return new CheckResult(false, "厄运流转没冷却好 / 刚放过");
		}
		return new CheckResult(true, "双伤 1：厄运流转");
	}

	public override PAction GetAction()
	{
		TwinDamage.Diag(TwinDamage.ShouldUse(), ActionHelper.IsUnlocked(23u), TwinDamage.ReadyWithCanCast(23u), ActionHelper.IsUnlocked(25747u), TwinDamage.ReadyWithCanCast(25747u));
		return PldAction.Of(23u, (ActionType)1);
	}
}
