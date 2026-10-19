using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// GCD：音速破（16153）—— 合并原单体档与群体档（旧 <c>GnbSonicBreakGcd</c> /
/// <c>GnbSonicBreakGcd(aoeOnly: true)</c>）为单一决策器。
///
/// <para>
/// ★ 2026-10-17（A 方案 · 恢复旧轴）：合并把两档收成一个类之后，旧版靠**注册位置**表达的
/// 两处先后必须由本类自己补回来，否则合并会改变轴：
/// </para>
/// <list type="number">
/// <item>群体轴：旧版群体档排在「命运之环」**之后**（旧 <c>:97</c> &gt; <c>:95/:96</c>）；
/// 注册表里本类也排在命运之环之后（:FC → :SB），群体轴顺序靠位置保证
/// 「狮心连 → 命运之环 → 音速破」；单体轴由**狮心连**反向让位给本类（见 GnbLionHeartGcd）。</item>
/// <item>倾泻资源：旧版倾泻链（子弹连 <c>:59/:60</c>、爆发击 <c>:64</c>）全排在音速破 <c>:67</c> 之前；
/// 合并后本类在倾泻链之前，所以这里显式让位。</item>
/// <item>链保护：旧版 <c>GnbGnashingFangGcd(NoMercyContinue)</c>（<c>:66</c>）在音速破之前，
/// 子弹连没打完时先续打；合并后靠这里补。</item>
/// </list>
/// </summary>
public class GnbSonicBreakGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：音速破（无情带出的 DoT）";

	/// <summary>
	/// 「音速破现在能不能打」的唯一判据 —— 本类 <see cref="CheckReal"/> 与
	/// <c>GnbLionHeartGcd</c> 的让位共用这一份，避免两处各写一遍。
	/// 只判技能自身可用性，不含「让位」判据。
	/// </summary>
	internal static CheckResult SonicBreakGate()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.音速破))
		{
			return new CheckResult(false, "音速破还没解锁（54 级）");
		}
		if (!GnbHelper.HasAura(GnbSkills.音速破预备))
		{
			return new CheckResult(false, "没有「音速破预备」（无情还没开）");
		}
		if (!GnbHelper.IsNoMercyInside() && !GnbHelper.IsNoMercyHeldOpen())
		{
			return new CheckResult(false, "无情马上要开，先把音速破留给窗口");
		}
		if (GnbHelper.GetCastTarget(GnbSkills.音速破) == null)
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但 {GnbHelper.GetMeleeRange():F0} 米内没有能打的怪"
				: "没有有效目标 / 不在近战射程内");
		}
		return new CheckResult(true, GnbHelper.ShouldUseAoeCombo()
			? "群体轴：音速破（无情带出的 DoT）"
			: "音速破（无情带出的 DoT）");
	}

	protected override CheckResult CheckReal()
	{
		CheckResult gate = SonicBreakGate();
		if (!gate.Success)
		{
			return gate;
		}
		// ① 链保护：子弹连没打完 → 先续打。
		// 社区轴（docs\14 §二）的无情窗口里音速破插在烈牙与猛兽爪之间
		//   （烈牙 → 倍攻 → 音速破 → 猛兽爪 → 凶禽爪 → 狮心连×3；原作者 子弹连.cs:40-43 同款），
		//   组合窗口 PreservesCombo=是（docs\16:73-77），插一个音速破不会断连；
		//   但「下一段就是凶禽爪」时不放行 —— 凶禽爪的续剑（穿目）是窗口里最贵的一发。
		bool nextChainStepIsWickedTalon = GnbGnashingFangGcd.NextComboStep() == GnbSkills.凶禽爪;
		bool releaseInsideNoMercy = GnbHelper.IsNoMercyInside() && !nextChainStepIsWickedTalon;
		if (!GnbHelper.ShouldUseAoeCombo() && GnbHelper.HasGnashingChainInProgress() && !releaseInsideNoMercy)
		{
			return new CheckResult(false, nextChainStepIsWickedTalon && GnbHelper.IsNoMercyInside()
				? "下一段是凶禽爪：先续打，音速破不许把穿目推后"
				: "子弹连还没打完：先续打（旧版靠注册位置保护）");
		}
		// ①·2 社区轴：子弹连还没起头（且起得来）⇒ 先让烈牙起头，音速破排它后面。
		// 判定只在 GnbGnashingFangGcd.ShouldStartChainFirst 一处（S1）。
		// ★ 2026-10-19 追加（玩家拍板）：QT「先打血壤连」开 + 血壤连在身时，音速破**也可以抢在
		//   「未起头」的子弹连前面**（顺序变成 倍攻 → 音速破 → 狮心连×3 → 子弹连）。
		//   ① 的链保护不变：子弹连已经起头就仍由「续打」优先，音速破只跟**未连击**的子弹连比先后。
		if (!GnbHelper.ShouldUseAoeCombo() && GnbGnashingFangGcd.ShouldStartChainFirst()
			&& !GnbHelper.ShouldPrioritizeLionHeartChain())
		{
			return new CheckResult(false, "子弹连先起头（社区轴无情窗口：烈牙 → 倍攻 → 音速破 → …）");
		}
		// ② 倾泻资源：让位给倾泻链（旧版倾泻链全部排在音速破之前）
		if (GnbHelper.IsDumpResource() && (GnbHelper.IsGnashingAvailable() || GnbHelper.GetAmmo() > 0))
		{
			return new CheckResult(false, "倾泻资源：让位给倾泻链（旧版倾泻链排在音速破之前）");
		}
		// ③ 100 级以下：无情内先让烈牙起头（88–99 的 8 GCD 窗口 = 子弹连1 → 倍攻 → 音速破 → 子弹连2）。
		//   100 级放行 —— 社区轴把音速破排在烈牙之后、猛兽爪之前（① 已放行）。
		if (!GnbHelper.ShouldUseAoeCombo() && !GnbHelper.IsDumpResource() && GnbHelper.IsNoMercyInside()
			&& GnbHelper.IsGnashingAvailable() && !GnbHelper.IsLevelAtLeast100())
		{
			return new CheckResult(false, "无情内让位给子弹连（100 级以下先起头烈牙）");
		}
		return new CheckResult(true, gate.Message);
	}

	public override PAction GetAction()
	{
		return GnbAction.Aoe(GnbSkills.音速破, (ActionType)0, GnbHelper.GetCastTarget(GnbSkills.音速破));
	}
}
