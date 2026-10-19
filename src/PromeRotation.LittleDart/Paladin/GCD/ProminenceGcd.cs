using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class ProminenceGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体：日珥斩";

	protected override CheckResult CheckReal()
	{
		
		
		
		if (!AoeQueue.ShouldUseAoeIgnoringTarget())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 自身 5 米内能打的怪不够）");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.日珥斩))
		{
			return new CheckResult(false, "日珥斩未解锁");
		}
		
		if (!PldHelper.IsComboAlive() || PldHelper.GetLastComboSpellId() != PldSkills.全蚀斩)
		{
			return new CheckResult(false, "AOE 连击不对：上一段不是全蚀斩（或连击计时已走完）");
		}
		if (!IsReadyAndInRange(PldSkills.日珥斩))
		{
			return new CheckResult(false, "日珥斩冷却中或身前没有能打的怪");
		}
		return new CheckResult(true, "群体：日珥斩（AOE 连击第 2 段）");
	}

	public override PAction GetAction()
	{
		
		return PldAction.Of(PldSkills.日珥斩, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		
		return PldHelper.IsReadyAndInRangeForNextGcdAoe(actionId);
	}
}
