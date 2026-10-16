using System;

namespace LittleDart.Common.Mitigation;

/// <summary>
/// 自保规则（2026-10-15 玩家需求）——「**按自己血量**交一个技能」这条职业私有规则。
///
/// <para>背景：原有的减伤判断器只管「威胁判据」（死刑读条 / AOE 读条 / 群怪压力），
/// 交什么由分类（大减 / 短减 / 团队减 / 无敌）在 <see cref="MitigationEngine.Evaluate"/> 里仲裁，
/// **没有任何一条按「自己血量」出手的规则**。绝枪需要的两条
/// （血量 &lt;50% ⇒ 刚玉之心；血量 &lt;70% 且身上没有极光 ⇒ 极光）走的就是这个插槽。</para>
///
/// <para>仲裁点在 <c>MitigationEngine.TryPickSelfCare</c>：**排在保命无敌之后、站定闸门之前**，
/// 与无敌同档 ⇒ **不受**「原地站定 1 秒」闸门约束（血崩时正在走位也必须能交）。
/// 规则之间**按 <see cref="MitigationJobProfile.SelfCareRules"/> 的列表顺序**取第一个成立的
/// —— 顺序 = 优先级。</para>
/// </summary>
public sealed class MitigationSelfCareRule
{
	/// <summary>
	/// 要交的技能。必须是 <c>Registry</c> 里的那一份（面板显示、找技能名、构建动作都走它）。
	/// <c>SkillId == 0</c> 视为无效规则、直接跳过。
	/// </summary>
	public MitigationSkillDef Def = new MitigationSkillDef();

	/// <summary>
	/// 实时判据。入参 = **自身当前血量比例（0..1）**（读不到时引擎不会调用它），
	/// 返回 <c>true</c> = 这一轮该交。
	///
	/// <para>⚠️ 这里只写「血量 / 身上有没有 buff」这类**本规则专属**的条件；
	/// 解锁、冷却、「这个技能已经在生效期」、「刚交过」这些公共闸门由引擎负责
	/// （<c>MitigationEngine.IsEligible</c> / <c>IsSkillActive</c>），不要在这里重复。</para>
	/// </summary>
	public Func<float, bool> Trigger = (float _) => false;

	/// <summary>
	/// 命中文案（显示在 PR 调试面板与诊断日志里）。写成人能看懂的中文，
	/// 例如 <c>"自保·血量&lt;50% 交刚玉之心"</c>。引擎会在后面附上实时读数。
	/// </summary>
	public string Reason = string.Empty;
}
