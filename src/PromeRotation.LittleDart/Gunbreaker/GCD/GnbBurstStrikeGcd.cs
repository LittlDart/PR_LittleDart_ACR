using System;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbBurstStrikeGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：爆发击";

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.爆发击))
		{
			return new CheckResult(false, "爆发击还没解锁（30 级）");
		}
		int ammo = GnbHelper.GetAmmo();
		if (ammo <= 0)
		{
			return new CheckResult(false, "没有晶壤");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		uint lastComboId = GnbHelper.GetLastComboSpellId();
		bool comboAlive = GnbHelper.GetComboTimeLeftSeconds() > 0.0;
		bool aoeActive = GnbHelper.ShouldUseAoeCombo() || GnbHelper.ShouldUseAoe(GnbBaseComboGcd.CurrentAoeThreshold);
		bool fatedCircleUnlocked = ActionHelper.IsUnlocked(GnbSkills.命运之环);

		
		if (GnbHelper.IsAmmoAtCap())
		{
			if (lastComboId == GnbSkills.残暴弹 && ActionHelper.IsUnlocked(GnbSkills.迅连斩))
			{
				return new CheckResult(true, $"晶壤已满（{ammo}/{GnbHelper.GetAmmoCap()}）：先倒一发爆发击腾位置再打迅连斩");
			}
			
			if (lastComboId == GnbSkills.恶魔切 && ActionHelper.IsUnlocked(GnbSkills.恶魔杀) && (!aoeActive || !fatedCircleUnlocked))
			{
				return new CheckResult(true, $"恶魔切防溢出：晶壤已满（{ammo}/{GnbHelper.GetAmmoCap()}），先倒一发爆发击腾位置再打恶魔杀");
			}
		}

		
		if (GnbHelper.ShouldDumpBloodfestOvercapAmmo() && (!aoeActive || !fatedCircleUnlocked))
		{
			return new CheckResult(true, $"日随无情外血壤超上限防吞：晶壤 {ammo} > {GnbHelper.GetNormalAmmoCap()} 且血壤剩余时间 ≤ 无情 CD ⇒ 倒爆发击");
		}

		
		if (aoeActive)
		{
			return new CheckResult(false, "多目标：让位给命运之环 / AOE 连");
		}

		bool insideNoMercy = GnbHelper.IsNoMercyInside();
		bool isLevel100 = GnbHelper.IsLevelAtLeast100();
		bool isLevel88Plus = GnbHelper.IsLevelAtLeast(88);

		
		if (GnbHelper.IsDumpResource() && !insideNoMercy)
		{
			return new CheckResult(true, $"倾泻资源·无情外：倒爆发击（晶壤 {ammo}）");
		}

		
		if (insideNoMercy)
		{
			if (!isLevel100 && isLevel88Plus && !GnbHelper.IsDumpResource())
			{
				return new CheckResult(false, "88–99 级无情窗口内不打爆发击（晶壤留给双子弹连与倍攻的 8 GCD 循环）");
			}
			return new CheckResult(true, $"无情内倒爆发击（晶壤 {ammo}）");
		}

		
		
		
		
		
		
		
		
		
		
		if (isLevel100 && PromeSettings.Instance.GetQt(GnbQt.UseNoMercy) && !GnbHelper.IsBaseComboInProgress())
		{
			int gcdsToNoMercy = (int)Math.Floor(GnbHelper.GetNoMercyCooldownSeconds() / (double)GnbHelper.GetGcdSeconds()) + 1;
			int remainder = gcdsToNoMercy % 3;
			bool entryGcd = gcdsToNoMercy <= 1;
			if (remainder != 0 && (entryGcd ? ammo > 0 : ammo > 1))
			{
				return new CheckResult(true, $"100 级无情前调轴：离无情 {gcdsToNoMercy} 个 GCD（余数 {remainder} ≠ 3 的整数倍）⇒ 爆发击吃余数（晶壤 {ammo}）");
			}
		}

		
		if (!isLevel88Plus && GnbHelper.IsNoMercyReady())
		{
			return new CheckResult(true, $"88 级以下无情前那一格：倒一发爆发击（晶壤 {ammo}）");
		}

		
		if (isLevel100 && GnbHelper.IsPotionAmmoDumpEnabled() && !GnbHelper.IsNoMercyPrepWindow())
		{
			return new CheckResult(true, $"药效内倒爆发击（晶壤 {ammo}）");
		}

		return new CheckResult(false, "未满足爆发击释放条件（非腾位 / 非无情内 / 非无情前防断连 / 非药效卸晶壤 / 非倾泻资源）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(GnbSkills.爆发击, (ActionType)0);
	}
}
