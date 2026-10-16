using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

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
		if (!ActionHelper.IsUnlocked(25750u))
		{
			return new CheckResult(false, "英勇之剑尚未解锁（90 级）");
		}
		if (!ConfiteorChain.CanFollow(25749u, 25750u))
		{
			return new CheckResult(false, "悔罪连未接上（上一段不是真理之剑 / 超出 5 秒窗口 / 技能不可用）");
		}
		return new CheckResult(true, "悔罪连：英勇之剑（接在真理之剑之后）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(25750u, (ActionType)0);
	}
}
