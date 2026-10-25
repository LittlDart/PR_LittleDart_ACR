using System;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// GCD：爆发击（16162）——
/// <list type="number">
/// <item>防溢出腾位（单体「残暴弹 → 迅连斩」与 AOE「恶魔切 → 恶魔杀」）：晶壤已满时先倒一发腾位置；</item>
/// <item>日随无情外血壤超上限防吞：晶壤 &gt; 3 且血壤 Buff 剩余时间 ≤ 无情 CD 时主动泄晶壤；</item>
/// <item>倾泻资源·无情外：开启「倾泻资源」时在无情外泄晶壤；</item>
/// <item>无情窗口内：100 级作为功能技后的通用兜底、88 级以下常规泄晶壤；<b>88–99 级无情内不打爆发击</b>（晶壤留给双子弹连 + 倍攻的 8 GCD 循环）；</item>
/// <item>★ 100 级 · 无情前「调轴」（2026-10-19 玩家口径「用判断方式决定爆发击如何调轴」）：
/// 不用固定秒数，改用 **GCD 余数判断** —— 离无情还有 <c>floor(无情CD / GCD) + 1</c> 个 GCD，
/// 一个完整 123 / 一条子弹连占 3 个 GCD；余数 ≠ 0 时用「余数发」爆发击把余数吃掉，
/// 让剩下的 GCD 正好塞满整数个 123 / 子弹连。只在基础连已完成时填；普通填充留 1 发晶壤，
/// 无情入口那一格允许打掉最后一发（进无情不带半截连击）。</item>
/// <item>88 级以下·无情前那一格：无情已就绪时先倒一发；</item>
/// <item>100 级·爆发药内卸晶壤：药效内且未进无情准备窗口时泄晶壤。</item>
/// </list>
/// </summary>
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

		// 1. 单体与 AOE 连击防溢出腾位（待办 #11 · 已完成（docs\35 §8.3）：含残暴弹 16139 与恶魔切 16141）
		if (GnbHelper.IsAmmoAtCap())
		{
			if (lastComboId == GnbSkills.残暴弹 && ActionHelper.IsUnlocked(GnbSkills.迅连斩))
			{
				return new CheckResult(true, $"晶壤已满（{ammo}/{GnbHelper.GetAmmoCap()}）：先倒一发爆发击腾位置再打迅连斩");
			}
			// 恶魔切（16141）后晶壤已满：若未解锁命运之环（<72 级）或当前未开 AOE，用爆发击防溢出（已开 AOE 且 ≥72 级时由前序 GnbFatedCircleGcd 优先放命运之环）
			if (lastComboId == GnbSkills.恶魔切 && ActionHelper.IsUnlocked(GnbSkills.恶魔杀) && (!aoeActive || !fatedCircleUnlocked))
			{
				return new CheckResult(true, $"恶魔切防溢出：晶壤已满（{ammo}/{GnbHelper.GetAmmoCap()}），先倒一发爆发击腾位置再打恶魔杀");
			}
		}

		// 2. 日随无情外血壤超上限晶壤（>3）防吞（待办 #11 · 已完成（docs\35 §8.3）：多目标且已解锁命运之环时已由前序命运之环处理，此处覆盖单体与 <72 级兜底）
		if (GnbHelper.ShouldDumpBloodfestOvercapAmmo() && (!aoeActive || !fatedCircleUnlocked))
		{
			return new CheckResult(true, $"日随无情外血壤超上限防吞：晶壤 {ammo} > {GnbHelper.GetNormalAmmoCap()} 且血壤剩余时间 ≤ 无情 CD ⇒ 倒爆发击");
		}

		// 多目标让位给命运之环 / AOE 连
		if (aoeActive)
		{
			return new CheckResult(false, "多目标：让位给命运之环 / AOE 连");
		}

		bool insideNoMercy = GnbHelper.IsNoMercyInside();
		bool isLevel100 = GnbHelper.IsLevelAtLeast100();
		bool isLevel88Plus = GnbHelper.IsLevelAtLeast(88);

		// 3. 倾泻资源（无情外）
		if (GnbHelper.IsDumpResource() && !insideNoMercy)
		{
			return new CheckResult(true, $"倾泻资源·无情外：倒爆发击（晶壤 {ammo}）");
		}

		// 4. 无情窗口内（待办 #10 · = T5（未做）：88–99 级无情内不打爆发击，保留晶壤打满「子弹连1 + 倍攻 + 音速破 + 双子弹连」8 GCD 循环）
		if (insideNoMercy)
		{
			if (!isLevel100 && isLevel88Plus && !GnbHelper.IsDumpResource())
			{
				return new CheckResult(false, "88–99 级无情窗口内不打爆发击（晶壤留给双子弹连与倍攻的 8 GCD 循环）");
			}
			return new CheckResult(true, $"无情内倒爆发击（晶壤 {ammo}）");
		}

		// 5. 100 级 · 无情前「调轴」：用判断方式决定要不要用爆发击填这一格（玩家 2026-10-19 口径：
		//    「用判断方式决定爆发击如何调轴」）。不再用 13.9s / 2.5s 魔法秒数，改成 GCD 余数判断：
		//      · 一个完整基础三连 = 3 个 GCD（一条子弹连也是 3 个 GCD）
		//      · gcdsToNoMercy = floor(无情 CD / 当前 GCD) + 1（+1 = 无情入口那一格）
		//      · 余数 r = gcdsToNoMercy % 3：r != 0 ⇒ 这段时间塞不满整数个 123 / 子弹连，
		//        用 r 发爆发击把余数吃掉（余 1 打 1 发、余 2 打 2 发；下一格自然回到「整除」，
		//        社区轴里爆发击成对出现就是这个余数在起作用，docs\14 §4.2 / §4.3）
		//      · 只允许在「基础连已完成」（没有待续段）时填充 —— 挂在 1/2 段时交给连击护栏
		//      · 留晶壤：普通填充要求 ammo > 1（给子弹连留 1 发）；无情入口那一格允许打掉最后一发
		//        （进无情不带半截基础连，比留 1 发更重要）
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

		// 6. 88 级以下 · 无情前那一格
		if (!isLevel88Plus && GnbHelper.IsNoMercyReady())
		{
			return new CheckResult(true, $"88 级以下无情前那一格：倒一发爆发击（晶壤 {ammo}）");
		}

		// 7. 100 级 · 爆发药内卸晶壤（非无情准备窗口）
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
