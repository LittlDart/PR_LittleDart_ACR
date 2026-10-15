using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbAoeBurstStrikeGcd : GnbAoeComboResolverBase
{
	protected override string ResolverDescription => "群体轴：爆发击（命运之环花不出晶壤时兜底）";

	protected override CheckResult CheckReal()
	{
		CheckResult commonCheck = GnbAoeComboResolverBase.AoeCommonCheck();
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (!ActionHelper.IsUnlocked(16162u))
		{
			return new CheckResult(false, "爆发击还没解锁（30 级）");
		}
		if (GnbHelper.GetAmmo() <= 0)
		{
			return new CheckResult(false, "没有晶壤可倒");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"群体轴：倒一发爆发击（晶壤 {GnbHelper.GetAmmo()}）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16162u, (ActionType)0);
	}
}
