using LittleDart.Common.Mitigation;
using LittleDart.Gunbreaker.Mitigation;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

/// <summary>
/// 极光（16151）—— 2026-10-15 **不再占位**：改由减伤调度器的「自保插槽」驱动。
///
/// <para>规则（玩家口径，逐字）：**自身血量低于 70% 且身上没有极光 ⇒ 对自己使用极光**。
/// 规则本体在 <c>GnbMitigationProfile.BuildSelfCareRules</c>，
/// 仲裁在 <c>MitigationEngine.TryPickSelfCare</c> —— 本决策器只负责「出手」。</para>
///
/// <para>⚠️ 与旧占位版的两处差别：</para>
/// <list type="number">
/// <item>目标从 <c>LowestHealthPartyMember</c>（给最残血的队友）改成 <c>Self</c>（对自己）
/// —— 玩家口径明确写「对自己使用极光」。</item>
/// <item>「身上没有极光」不只是本决策器的判据，引擎还会用 <c>BuffId = 1835</c>
/// 判「这个技能正在生效期」⇒ **一次极光没走完不会再交第二次**（旧占位版没有这层）。</item>
/// </list>
///
/// <para>仍未实现的：**给队友 / 给搭档的极光**（需要热键系统，绝枪侧现在一个热键都没有
/// —— 见 docs\21 §四）。</para>
/// </summary>
public class GnbAuroraOffGcd : GnbResolverBase
{
	private static readonly MitigationSkillDef? Def = GnbMitigationConfig.Find(GnbSkills.极光);

	protected override string ResolverDescription => "自保：极光（血量<70% 且身上没有极光 · 对自己 · 调度器）";

	protected override CheckResult CheckReal()
	{
		return GnbMitigationResolverSupport.Check(Def);
	}

	public override PAction GetAction()
	{
		return GnbMitigationResolverSupport.GetAction(Def);
	}
}
