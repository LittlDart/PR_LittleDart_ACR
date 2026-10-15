using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class ExpiacionOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：偿赎剑（双伤 2）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(25747u))
		{
			return new CheckResult(false, "偿赎剑还没学会（86 级）");
		}
		if (!TwinDamage.ShouldUse())
		{
			return new CheckResult(false, "双伤的两个锁没过（战逃/绝对统治冷却不在窗口）或不在近战范围");
		}
		if (!TwinDamage.ReadyWithCanCast(25747u))
		{
			return new CheckResult(false, "偿赎剑没冷却好 / 刚放过");
		}
		return new CheckResult(true, "双伤 2：偿赎剑");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(25747u, (ActionType)1);
	}
}
