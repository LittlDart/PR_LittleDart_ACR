using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbFatedCircleGcd : GnbResolverBase
{
	protected const int InsideAmmoThreshold = 1;

	protected static int OutsideAmmoThreshold => GnbHelper.GetNormalAmmoCap();

	protected override string ResolverDescription => "GCD：命运之环";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.命运之环))
		{
			return new CheckResult(false, "命运之环还没解锁（72 级）");
		}
		int ammo = GnbHelper.GetAmmo();
		if (ammo <= 0)
		{
			return new CheckResult(false, "没有晶壤");
		}
		
		if (PromeSettings.Instance.GetQt(GnbQt.UseAoe) && GnbHelper.GetNearestEnemy() != null && GnbHelper.GetLastComboSpellId() == GnbSkills.恶魔切 && ActionHelper.IsUnlocked(GnbSkills.恶魔杀) && GnbHelper.IsAmmoAtCap())
		{
			return new CheckResult(true, $"恶魔切防溢出：晶壤已满（{ammo}/{GnbHelper.GetAmmoCap()}），先放命运之环腾位置再打恶魔杀");
		}
		
		if (GnbHelper.ShouldUseAoe(GnbBaseComboGcd.CurrentAoeThreshold) && GnbHelper.GetNearestEnemy() != null && GnbHelper.ShouldDumpBloodfestOvercapAmmo())
		{
			return new CheckResult(true, $"日随无情外血壤超上限防吞：晶壤 {ammo} > {OutsideAmmoThreshold} 且血壤剩余时间 ≤ 无情 CD ⇒ 泄命运之环");
		}
		CheckResult commonCheck = GnbHelper.CheckAoeCluster(GnbHelper.AoeComboThreshold);
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(true, $"群体轴·无情内：命运之环（晶壤 {ammo}）");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new CheckResult(true, $"群体轴·倾泻资源：命运之环（晶壤 {ammo}）");
		}
		if (ammo < OutsideAmmoThreshold)
		{
			return new CheckResult(false, $"没有晶壤（无情外取 ≥{OutsideAmmoThreshold}，现在 {ammo} 发）");
		}
		return new CheckResult(true, $"群体轴·无情外：命运之环（晶壤 {ammo}）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(GnbSkills.命运之环, (ActionType)0);
	}
}
