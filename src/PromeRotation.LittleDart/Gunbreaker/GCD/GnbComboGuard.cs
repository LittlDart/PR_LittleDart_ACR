using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// 唯一连击护栏（docs\18 S1：任何「本格要不要让位给基础连击第 3 段」的判断只许在这里）。
/// <para>
/// 输入：本格动作要占几个 GCD（<c>gcdCost</c>）；输出：这一格该不该让位（<c>Yield</c>）+ 面板文案（<c>Reason</c>）。
/// 只读、无副作用 —— 可以放心在 <c>Check()</c> 里每帧调（PR 刷调试面板会每帧调一次）。
/// </para>
/// <para><b>2026-10-19 整改（玩家口径：问题 3）</b>：目标改成「无论如何都尽量保证进无情时不带基础连击」，
/// 且「子弹连要能打到凶禽爪 → 穿目进无情」。据此把规则收敛成一条：</para>
/// <list type="bullet">
/// <item><b>只允许在「起头」路径调用</b>（<c>NextComboStep() == 0</c>）——<b>这是本轮（2026-10-19）的新结论</b>，
/// 不是 docs\32 §7.9.3 的原文（那里要求「所有花 GCD 的格子都要问它」，那条仍待逐格上机接入，见 docs\36 §1 批次 I）。
/// 依据：烈牙连一旦开始必须连续打完，不许被基础连抢回一格 —— 否则会白丢 凶禽爪 + 穿目。
/// 旧实现把护栏放在「续打下一段」之前，实测日志（2026-10-19 03:18.945）正好丢了这两发。</item>
/// <item><b>无情外 + 基础连挂在 1/2 段 ⇒ 一律先补完第 3 段</b>（不再看连击剩余时间，也不再看
/// 「无情 5s 内到 + 已经在凶禽爪」那条旧规则 —— 那条来得太晚，见 docs\32 §7.9.2 漏点 C）。
/// 机制依据：烈牙 16146 / 猛兽爪 16147 / 凶禽爪 16150 / 爆发击 16162 / 倍攻 25760 / 音速破 16153 /
/// 狮心连 36937-36939 的 <c>PreservesCombo</c> 都是「是」（docs\16:73-77 与 docs\23 §11.1 的
/// 游戏内 sheet_row 实读）⇒ 这些 GCD 不会清掉基础连击窗口，所以「先起子弹连、回头再补 3」是可行的，
/// 但**进无情时**基础连会挂在半截。</item>
/// <item><b>无情内不保</b>（docs\32 §7.9.2 漏点 B）：无情 8 个 GCD 装不下基础三连，
/// 窗口里断基础连是刻意的取舍。</item>
/// <item><b>起手例外</b>：<c>GnbOpener100</c> 是静态表，起手期间决策器根本不会被问（docs\18），
/// 它本来就带着「残暴弹」进无情（第 3 格 血壤 → 残暴弹 → 无情），本护栏管不到、也不该去管。</item>
/// </list>
/// <para>
/// 来源：docs\31 T2、docs\32 §7.9.2–7.9.3、docs\36 §1 T2；「开无情那一刻不能挂着基础三连步进」这条
/// 来自 docs\14 §三 深红字 —— ⚠️ docs\14:138 明确写它是**机制层判断、本仓库未实测**，
/// 本轮按**用户口径**落地，上机若与描述不符，先回来改这条注释与规则。
/// </para>
/// </summary>
internal static class GnbComboGuard
{
	/// <summary>子弹连一格要占的 GCD 数（烈牙 → 猛兽爪 → 凶禽爪）。</summary>
	internal const int GnashingFangGcdCost = 3;

	/// <summary>
	/// 判断「本格花 <paramref name="gcdCost"/> 个 GCD 的动作要不要让位给基础连击第 3 段」。
	/// <para>⚠️ 调用点只允许放在**起头**路径（见类注释）；不要放回「续打下一段」之前。</para>
	/// </summary>
	/// <param name="gcdCost">本格动作要占几个 GCD（子弹连 = 3）。小于 1 按 1 处理。</param>
	/// <returns>Yield = 是否让位；Reason = 面板文案（不让位时为空串）。</returns>
	internal static (bool Yield, string Reason) ShouldYieldToBaseCombo(int gcdCost)
	{
		if (gcdCost < 1)
		{
			gcdCost = 1;
		}
		// 只有「基础连还需要继续」才需要护栏（上一段是 利刃斩/残暴弹，且连击窗口还活着）。
		if (!GnbHelper.IsBaseComboInProgress())
		{
			return (false, string.Empty);
		}
		// 无情内不保（docs\32 §7.9.2 漏点 B）。
		if (GnbHelper.IsNoMercyInside())
		{
			return (false, string.Empty);
		}
		// 不在近战射程时不要抢文案：让调用点自己的射程判据去解释（否则面板会显示
		// 「先补完第 3 段」，而实际那一格交给的是闪雷弹）。
		if (!GnbHelper.IsInMeleeRange())
		{
			return (false, string.Empty);
		}
		uint lastComboId = GnbHelper.GetLastComboSpellId();
		int remainingBaseGcds = lastComboId == GnbSkills.利刃斩 ? 2 : 1;
		return (true, $"基础连挂在 {BaseComboStepName(lastComboId)}（还要 {remainingBaseGcds} 个 GCD）："
			+ $"先补完第 3 段，再起本格 {gcdCost} GCD 的动作（进无情不带基础连击）");
	}

	private static string BaseComboStepName(uint lastComboId)
	{
		return lastComboId == GnbSkills.利刃斩 ? "利刃斩 1/3" : "残暴弹 2/3";
	}
}
