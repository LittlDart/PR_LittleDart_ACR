using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbFatedCircleOutsideNoMercyGcd : GnbFatedCircleInsideNoMercyGcd
{

	protected override string ResolverDescription => $"群体轴·无情外：命运之环（晶壤 ≥ {OutsideAmmoThreshold}）";

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
		if (GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "在无情窗口内（交给无情内那一档）");
		}
		int ammo = GnbHelper.GetAmmo();
		if (ammo < OutsideAmmoThreshold)
		{
			return new CheckResult(false, $"没有晶壤（本档取 ≥{OutsideAmmoThreshold}，现在 {ammo} 发）");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"群体轴·无情外：命运之环（晶壤 {ammo}）");
	}
}
