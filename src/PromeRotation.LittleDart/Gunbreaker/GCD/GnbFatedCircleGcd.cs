using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// GCD：命运之环（16163）——
/// <list type="number">
/// <item>恶魔切防溢出腾位（待办 #11 · 已完成（docs\35 §8.3））：连击在恶魔切（16141）且晶壤达上限（<see cref="GnbHelper.IsAmmoAtCap"/>）时，先放命运之环再打恶魔杀；</item>
/// <item>日随无情外血壤超上限防吞（待办 #11 · 已完成（docs\35 §8.3））：日常模式下无情外晶壤 &gt; 3 且血壤剩余时间 ≤ 无情 CD 时，主动放命运之环泄掉超上限晶壤；</item>
/// <item>群体轴·无情内 / 倾泻资源：晶壤 ≥ 1 即放；</item>
/// <item>群体轴·无情外：晶壤达到常规上限（<see cref="GnbHelper.GetNormalAmmoCap"/>）时释放。</item>
/// </list>
/// </summary>
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
		// 1. 恶魔切（16141）连击防溢出腾位：只要开启 AOE 且身边有怪，恶魔切后满晶壤立即泄一发命运之环，防止恶魔杀 +1 晶壤溢出
		if (PromeSettings.Instance.GetQt(GnbQt.UseAoe) && GnbHelper.GetNearestEnemy() != null && GnbHelper.GetLastComboSpellId() == GnbSkills.恶魔切 && ActionHelper.IsUnlocked(GnbSkills.恶魔杀) && GnbHelper.IsAmmoAtCap())
		{
			return new CheckResult(true, $"恶魔切防溢出：晶壤已满（{ammo}/{GnbHelper.GetAmmoCap()}），先放命运之环腾位置再打恶魔杀");
		}
		// 2. 日随无情外血壤超上限（>3）防吞：只要满足基础 AOE 门槛（≥AoeEnemyCount）且超上限晶壤即将因血壤结束被吞，立即泄命运之环
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
