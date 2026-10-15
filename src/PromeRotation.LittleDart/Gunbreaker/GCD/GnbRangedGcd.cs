using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbRangedGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：闪雷弹（远程兜底）";

	protected override CheckResult CheckReal()
	{
		if (!PromeSettings.Instance.GetQt("远离闪雷弹"))
		{
			return new CheckResult(false, "QT『远离闪雷弹』关着");
		}
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16143u))
		{
			return new CheckResult(false, "闪雷弹还没解锁");
		}
		if (GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "已经在近战范围内，不需要远程");
		}
		if (!GnbHelper.IsReadyAndInRangeForNextGcd(16143u))
		{
			return new CheckResult(false, "闪雷弹冷却中或不在射程内");
		}
		return new CheckResult(true, "贴不上去，远程兜底");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16143u, (ActionType)0);
	}
}
