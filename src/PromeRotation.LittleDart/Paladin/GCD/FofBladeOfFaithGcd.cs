using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofBladeOfFaithGcd : PldResolverBase
{
	protected override string ResolverDescription => "悔罪连：信念之剑（第 2 段）";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.信念之剑))
		{
			return new CheckResult(false, "信念之剑尚未解锁（90 级）");
		}
		if (!ConfiteorChain.CanFollow(PldSkills.悔罪, PldSkills.信念之剑))
		{
			return new CheckResult(false, "悔罪连未接上（上一段不是悔罪 / 超出 5 秒窗口 / 技能不可用）");
		}
		return new CheckResult(true, "悔罪连：信念之剑（接在悔罪之后）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.信念之剑, ActionType.Gcd);
	}
}
