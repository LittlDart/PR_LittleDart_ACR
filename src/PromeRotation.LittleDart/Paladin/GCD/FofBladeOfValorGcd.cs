using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofBladeOfValorGcd : PldResolverBase
{
	protected override string ResolverDescription => "悔罪连：英勇之剑（第 4 段）";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.英勇之剑))
		{
			return new CheckResult(false, "英勇之剑尚未解锁（90 级）");
		}
		if (!ConfiteorChain.CanFollow(3, PldSkills.英勇之剑))
		{
			return new CheckResult(false, "悔罪连未接上（安魂祈祷层数不是基线-3 / 技能不可用）");
		}
		return new CheckResult(true, "悔罪连：英勇之剑（接在真理之剑之后）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.英勇之剑, ActionType.Gcd);
	}
}
