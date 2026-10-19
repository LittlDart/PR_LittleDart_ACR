using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbBloodfestOffGcd : GnbResolverBase
{
	protected override string ResolverDescription => "oGCD：血壤（补晶壤 + 开狮心）";

	/// <summary>
	/// 判据顺序（便宜 → 贵）：解锁 → QT「血壤」 → <b>晶壤已达上限</b> → <b>已有「心有灵狮」</b> →
	/// <b>不占无情位置</b> → <b>续剑让位（收窄版）</b> → QT「延后血壤」 → GCD 剩余 → 目标 →
	/// 技能位被替换 → 群体/倾泻 → CanCast。
	///
	/// <para><b>2026-10-19 玩家口径（问题 1 / 问题 2 整改，两处行为变化）：</b></para>
	/// <list type="number">
	/// <item><b>血壤不再压到无情窗口</b>：删掉旧的两条「等无情」闸门
	/// （①「QT 先打血壤连 开着 + 无情外 ⇒ 等窗口的第一格」②「无情还没进窗口（等它剩下 3 个 GCD）」）。
	/// 现在**好了就用**，允许插在烈牙连中间（与 docs\14 §4.6「血壤插在猛兽爪 / 裂膛之间」的轴一致）。
	/// 「先打血壤连」QT 的默认值/显示名保持原样，但**不再控制**无情窗口的 GCD 顺序
	/// （顺序由社区轴固定：<c>GnbGnashingFangGcd.ShouldStartChainFirst</c> 无条件决定）。</item>
	/// <item><b>续剑优先（2026-10-19 晚收窄，方案 A，docs\44）</b>：续剑待打时血壤**不再整格让位**，
	/// 只在两种情况下让 ——
	/// ① 「续剑正给无情让位」的窗口（无情本 GCD 可开 / 无情刚开 1s 防吞）：判据与续剑同源，
	/// 见 <see cref="GnbHelper.GetContinuationNoMercyYieldReason"/>；这是修
	/// 「凶禽爪 → 无情 → 血壤 → 音速破，穿目整发丢失」时真正要保护的场景；
	/// ② 其余时段续剑被「续剑后置」推到 GCD 后半才织入 ⇒ 血壤可以抢**第一织入格**，
	/// 但必须给续剑留出后半那一格：GCD 余量 &lt;
	/// <see cref="GnbHelper.BloodfestBeforeContinuationMinGcdRemain"/>（1.7s = 实测下发间隔 0.81s
	/// + 续剑织入门槛 0.6s + 约 0.3s 余量）时仍然让位 —— **避免血壤顶掉续剑**。
	/// <para>⚠️ 旧行为（续剑只要能打血壤就整格不出）的实测副作用：一轮开怪后第一发血壤固定晚
	/// 6~8.6s（4 份日志 8/8 晚例全部落在烈牙连续剑链上，max +8.578s；根因报告见 docs\44）。</para></item>
	/// </list>
	///
	/// <para><b>T7 / E2 两条判据</b>（来源：docs\31 T7 / docs\32 T7，原作者 血壤.cs:30-38）：
	/// ① 晶壤已达上限 ⇒ 不开血壤（上限一律用 <see cref="GnbHelper.GetAmmoCap"/>，血壤 buff 5051 期间 = 6，
	/// <b>不</b>照抄原作者 <c>Level &gt;= 88 ? 3 : 2</c>）；
	/// ② 已有「心有灵狮」（buff 3840）⇒ 不开血壤。</para>
	///
	/// <para><b>保留的旧闸门（不是本次整改对象）</b>：QT「延后血壤」= 玩家手动要求压到无情窗口
	/// （默认关）；群体连招中只在无情窗口内开（docs\23 §11.2 玩家口径）；倾泻资源进行中不开
	/// （docs\15 §7）。</para>
	///
	/// <para><b>新增闸门（2026-10-19）</b>：GCD 剩余 ≤ 0.3s 不出手 —— 与原作者
	/// <c>血壤.cs:16</c> 的 <c>GetGcdRemain() &lt;= 0.3</c> 同口径（原文件在
	/// <c>.dsh-ops\Nag0miACR\Gunbreaker\Action\OffGcd\血壤.cs</c>，不在 docs 摘录里），
	/// 目的仍是避免 oGCD 贴着 GCD 末尾插、卡掉下一格。</para>
	/// </summary>
	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.血壤))
		{
			return new CheckResult(false, "血壤还没解锁（76 级）");
		}
		if (!PromeSettings.Instance.GetQt(GnbQt.UseBloodfest))
		{
			return new CheckResult(false, "QT「血壤」关着");
		}
		// T7 ①：晶壤已达上限 ⇒ 不开血壤（来源：docs\31 T7 / docs\32 T7）
		if (GnbHelper.IsAmmoAtCap())
		{
			return new CheckResult(false, $"晶壤已满（{GnbHelper.GetAmmo()}/{GnbHelper.GetAmmoCap()}），不开血壤");
		}
		// T7 ②：已有「心有灵狮」（buff 3840）⇒ 不开血壤（来源：docs\31 T7 / docs\32 T7）
		if (GnbHelper.HasAura(GnbSkills.心有灵狮))
		{
			return new CheckResult(false, "已有「心有灵狮」，不开血壤");
		}
		// ★ 2026-10-19 方案 A（docs\44）：让位闸门收窄 + 两条硬保证。
		//   旧闸门：续剑只要能打（几乎整条烈牙连期间）血壤就整格让位 ⇒ 实测「一轮开怪后第一发血壤
		//   固定晚 6~8.6s」（4 份日志 8/8 晚例全部落在烈牙连续剑链上，max +8.578s）。
		//   新闸门分三段：
		//   ① 不占无情位置：无情本 GCD 内就能开 ⇒ 让无情先拿这一格（列表第 1 位本来就先赢；
		//      这条防的是「无情还差一点点时血壤抢走窗口 ⇒ 无情被迫晚一个 GCD」）。
		//   ② 续剑正给无情让位（无情本 GCD 可开 / 无情刚开 1s 防吞）⇒ 血壤同步让位 ——
		//      这是 2026-10-19 修「穿目整发丢失」时真正要保护的场景，判据与续剑同源（S1）。
		//   ③ 其余时段续剑是被「续剑后置」推到 GCD 后半才织入 ⇒ 血壤可以抢第一织入格，
		//      但必须给续剑留出后半那格：实测血壤→续剑下发间隔 0.753~0.813s + 续剑织入门槛 0.6s，
		//      再加 ~0.3s 余量 ⇒ 1.7s；余量不足就仍然让位 —— 避免血壤顶掉续剑。
		if (!GnbHelper.IsNoMercyActive() && GnbHelper.WillNoMercyBeReadyInCurrentGcd())
		{
			return new CheckResult(false, "无情本 GCD 可开：血壤让位（不占无情窗口）");
		}
		if (GnbHelper.HasContinuationReadyInMelee())
		{
			string? continuationYieldReason = GnbHelper.GetContinuationNoMercyYieldReason();
			if (continuationYieldReason != null)
			{
				return new CheckResult(false, $"续剑正给无情让位：血壤同步让位（{continuationYieldReason}）");
			}
			float gcdRemainForContinuation = GnbHelper.GetGcdRemainSeconds();
			if (gcdRemainForContinuation < GnbHelper.BloodfestBeforeContinuationMinGcdRemain)
			{
				return new CheckResult(false, $"续剑待打且 GCD 余量不足（{gcdRemainForContinuation:F2}s < {GnbHelper.BloodfestBeforeContinuationMinGcdRemain:F2}s）：血壤让位，避免顶掉续剑");
			}
		}
		// QT「延后血壤」：玩家显式要求压到无情窗口（默认关，唯一还保留的「等无情」闸门）。
		if (GnbHelper.DelayBloodfestUntilNoMercy() && !GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "QT「延后血壤」开着：等无情窗口");
		}
		// 新增（2026-10-19）：GCD 剩余 ≤ 0.3s 不出手，避免卡 GCD。
		// 同口径可回溯到原作者 `.dsh-ops\Nag0miACR\...\OffGcd\血壤.cs:16` 的 GetGcdRemain() <= 0.3。
		float gcdRemain = GnbHelper.GetGcdRemainSeconds();
		if (gcdRemain <= 0.3f)
		{
			return new CheckResult(false, $"GCD 剩余不足（{gcdRemain:F2}s ≤ 0.30s），避免卡 GCD");
		}
		if (GnbHelper.GetBloodfestCastTarget() == null)
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但自身 {GnbHelper.AoeEnemyCountRange} 米内没有能打的怪"
				: "没有有效目标（血壤必须选中一个敌人才能放）");
		}
		if (ActionHelper.GetAdjustedActionId(GnbSkills.血壤) != GnbSkills.血壤)
		{
			return new CheckResult(false, "血壤技能位已被替换成狮心连");
		}
		if (GnbHelper.ShouldUseAoeCombo() && !GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "群体连招中：血壤只在无情窗口内开（无情外不开，宁可空转）");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new CheckResult(false, "倾泻资源进行中");
		}
		if (!ActionHelper.CanCast(GnbSkills.血壤, (IBattleChara)null))
		{
			return new CheckResult(false, "血壤冷却中或不可施放");
		}
		return new CheckResult(true, $"开血壤（无情冷却 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Aoe(GnbSkills.血壤, (ActionType)1, GnbHelper.GetBloodfestCastTarget());
	}
}
