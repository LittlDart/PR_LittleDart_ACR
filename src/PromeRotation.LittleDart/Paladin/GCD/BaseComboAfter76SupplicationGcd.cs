using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class BaseComboAfter76SupplicationGcd : PldResolverBase
{
	protected override string ResolverDescription => "基础连招(76级后)：祈告剑";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.祈告剑))
		{
			return new CheckResult(false, "祈告剑未解锁（76 级后才有）");
		}
		if (!PldHelper.HasAura(PldSkills.祈告剑预备))
		{
			return new CheckResult(false, "没有「祈告剑预备」");
		}
		if (!IsReadyAndInRange(PldSkills.祈告剑))
		{
			return new CheckResult(false, "祈告剑冷却中或不在射程内");
		}
		return new CheckResult(true, "基础连招(76级后)：祈告剑");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.祈告剑, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
