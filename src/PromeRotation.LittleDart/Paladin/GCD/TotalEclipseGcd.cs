using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class TotalEclipseGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体：全蚀斩";

	protected override CheckResult CheckReal()
	{
		
		
		
		
		
		
		
		if (!AoeQueue.ShouldUseAoeIgnoringTarget())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 自身 5 米内能打的怪不够）");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.全蚀斩))
		{
			return new CheckResult(false, "全蚀斩未解锁");
		}
		
		if (PldHelper.IsComboAlive() && PldHelper.GetLastComboSpellId() == PldSkills.全蚀斩)
		{
			return new CheckResult(false, "AOE 连击已起手，交给日珥斩");
		}
		if (!IsReadyAndInRange(PldSkills.全蚀斩))
		{
			return new CheckResult(false, "全蚀斩冷却中或身前没有能打的怪");
		}
		return new CheckResult(true, "群体：全蚀斩（AOE 连击起手）");
	}

	public override PAction GetAction()
	{
		
		return PldAction.Of(PldSkills.全蚀斩, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		
		return PldHelper.IsReadyAndInRangeForNextGcdAoe(actionId);
	}
}
