using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbFatedCircleInsideNoMercyGcd : GnbAoeComboResolverBase
{
	protected const int AmmoCost = 1;

		protected const int OutsideAmmoThreshold = 3;

		protected const int InsideAmmoThreshold = 1;

	protected override string ResolverDescription => "群体轴·无情内：命运之环（耗 1 晶壤）";

	protected override CheckResult CheckReal()
	{
		CheckResult commonCheck = GnbAoeComboResolverBase.AoeCommonCheck();
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (!ActionHelper.IsUnlocked(16163u))
		{
			return new CheckResult(false, "命运之环还没解锁（72 级）");
		}
		if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "不在无情窗口内（交给无情外那一档）");
		}
		if (GnbHelper.GetAmmo() < InsideAmmoThreshold)
		{
			return new CheckResult(false, $"没有晶壤（无情内不保留晶壤，门槛 {InsideAmmoThreshold} 发）");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"群体轴·无情内：命运之环（晶壤 {GnbHelper.GetAmmo()}）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16163u, (ActionType)0);
	}
}
