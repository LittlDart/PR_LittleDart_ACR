using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class SpiritsWithinOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：深奥之灵（双伤 2 · 低等级）";

	protected override CheckResult CheckReal()
	{
		if (ActionHelper.IsUnlocked(25747u))
		{
			return new CheckResult(false, "已学会偿赎剑，深奥之灵这一档不存在");
		}
		if (!ActionHelper.IsUnlocked(29u))
		{
			return new CheckResult(false, "深奥之灵还没解锁");
		}
		if (!TwinDamage.ShouldUse())
		{
			return new CheckResult(false, "双伤的两个锁没过（战逃/绝对统治冷却不在窗口）或不在近战范围");
		}
		if (!TwinDamage.ReadyWithCanCast(29u))
		{
			return new CheckResult(false, "深奥之灵没冷却好 / 刚放过");
		}
		return new CheckResult(true, "双伤 2（低等级形态）：深奥之灵");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(29u, (ActionType)1);
	}
}
