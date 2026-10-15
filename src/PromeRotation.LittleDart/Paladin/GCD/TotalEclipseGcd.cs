using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class TotalEclipseGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体：全蚀斩";

	protected override CheckResult CheckReal()
	{
		if (!AoeQueue.ShouldUseAoe())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 没有目标 / 敌人数量不够）");
		}
		if (!ActionHelper.IsUnlocked(7381u))
		{
			return new CheckResult(false, "全蚀斩未解锁");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "目标不在近战范围内");
		}
		if (PldHelper.GetLastComboSpellId() == 7381)
		{
			return new CheckResult(false, "AOE 连击已起手，交给日珥斩");
		}
		if (!IsReadyAndInRange(7381u))
		{
			return new CheckResult(false, "全蚀斩冷却中或不在射程内");
		}
		return new CheckResult(true, "群体：全蚀斩（AOE 连击起手）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(7381u, (ActionType)0);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		return PldHelper.IsReadyAndInRangeForNextGcd(actionId);
	}
}
