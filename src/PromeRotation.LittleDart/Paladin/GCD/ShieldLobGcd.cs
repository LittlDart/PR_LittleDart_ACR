using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class ShieldLobGcd : PldResolverBase
{
	protected override string ResolverDescription => "远离：投盾";

	protected override CheckResult CheckReal()
	{
		if (!PromeSettings.Instance.GetQt("远离投盾"))
		{
			return new CheckResult(false, "QT「远离投盾」没开");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(24u))
		{
			return new CheckResult(false, "投盾未解锁");
		}
		if (!PldHelper.IsTargetFar())
		{
			return new CheckResult(false, "目标在近战范围内，交给近战基础连招");
		}
		if (!PldHelper.PassesRangedShieldLobGate())
		{
			return new CheckResult(false, "没到「飞盾释放阈值」");
		}
		if (!IsReadyAndInRange(24u))
		{
			return new CheckResult(false, "投盾冷却中或不在射程内");
		}
		return new CheckResult(true, "远离：投盾（最后手段）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(24u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
