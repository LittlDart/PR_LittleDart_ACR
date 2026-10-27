using LittleDart.Common;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// GCD：倍攻（25760）——
/// <list type="bullet">
/// <item>合并原单体倍攻、群体倍攻与 <c>GnbDumpDoubleDownGcd</c>；</item>
/// <item>88–99 级无情窗口内（待办 #10 · = T5（未做））：若烈牙尚未起头且可用（<c>NextComboStep() == 0 &amp;&amp; IsGnashingAvailable()</c>），先让位给子弹连 1（烈牙）起头转 CD，构成「子弹连1 → 倍攻 → 音速破 → 两个子弹连打完」的标准 8 GCD 循环（100 级逻辑不变）。</item>
/// </list>
/// </summary>
public class GnbDoubleDownGcd : GnbResolverBase
{
	private const int AmmoCost = 2;

	protected override string ResolverDescription => "GCD：倍攻（耗 2 晶壤 · 无情内）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.倍攻))
		{
			return new CheckResult(false, "倍攻还没解锁（按 IsUnlocked(倍攻) 判定，不硬编码等级）");
		}
		if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "不在无情窗口内");
		}
		// 社区轴的无情窗口顺序：烈牙 → 倍攻 → 音速破 → 猛兽爪 → 凶禽爪 → 狮心连×3。
		// 窗口内容固定、只按轴排先后（玩家 2026-10-19 口径）；子弹连还没起头时让位给它。
		// 判定只在 GnbGnashingFangGcd.ShouldStartChainFirst 一处（S1）。
		//
		// ★ 2026-10-19 玩家拍板（QT「先打血壤连」恢复生效，**倍攻抢在狮心连前可接受**）：
		//   QT 开 + 心有灵狮在身（狮心连打得到）+ 晶壤 > 2 ⇒ 倍攻**不让位**，先倒这一发；
		//   其余情况仍让位给子弹连。晶壤 ≤ 2 时让位是为了给子弹连留至少 1 发
		//   （倍攻耗 2，双发一起倒会把烈牙连直接饿死）。
		if (GnbGnashingFangGcd.ShouldStartChainFirst()
			&& (!GnbHelper.ShouldPrioritizeLionHeartChain() || GnbHelper.GetAmmo() <= AmmoCost))
		{
			return new CheckResult(false, "子弹连先起头（社区轴无情窗口：烈牙 → 倍攻 → 音速破 → …）");
		}
		if (GnbHelper.GetAmmo() < AmmoCost)
		{
			return new CheckResult(false, $"晶壤不足（需要 {AmmoCost} 发）");
		}
		if (!AbilityGate.ReadyForNextGcd(GnbSkills.倍攻))
		{
			return new CheckResult(false, "倍攻冷却中或不可施放");
		}
		if (GnbHelper.ShouldUseAoeCombo())
		{
			if (GnbHelper.GetNearestEnemy() == null)
			{
				return new CheckResult(false, $"自身 {GnbHelper.AoeEnemyCountRange} 米内没有能打的怪");
			}
			return new CheckResult(true, "群体轴：倒倍攻");
		}
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new CheckResult(true, "倾泻资源·无情内：倒倍攻");
		}
		// 待办 #10 · = T5（未做）：88–99 级无情窗口内，若还未打烈牙（子弹连1），先让烈牙起头转 CD，再接倍攻与音速破
		if (!GnbHelper.IsLevelAtLeast100() && GnbGnashingFangGcd.NextComboStep() == 0 && GnbHelper.IsGnashingAvailable())
		{
			return new CheckResult(false, "88–99 级无情内：先让子弹连 1（烈牙）起头，再打倍攻");
		}
		return new CheckResult(true, "无情内倒倍攻");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(GnbSkills.倍攻, (ActionType)0);
	}
}
