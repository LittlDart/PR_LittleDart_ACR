using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 「刚玉之心 → 血量最低的队友」热键（六宫格 · 第 3 行第 1 列）。
///
/// <para>技能 = <see cref="GnbSkills.刚玉之心"/>（25758，减伤 oGCD）。</para>
///
/// <para><b>「血量最低」判的是绝对值不是百分比</b>（<c>GnbHelper.GetLowestAbsoluteHpPartner</c>，
/// 与骑士 <c>GetLowestAbsoluteHpPartner</c> 同口径）：四人本里 T 的血量上限本来就比奶妈高，
/// 按百分比挑会在「T 掉到 60%、奶妈还是满血」时挑中 T，而按绝对值挑才是真的快死那个。</para>
/// </summary>
public sealed class GnbHeartOfCorundumLowestHpHotkey : GnbSupportHotkeyBase
{
	protected override uint SkillId => GnbSkills.刚玉之心;

	protected override string SkillName => "刚玉之心";

	protected override GnbHelper.SupportTargetKind TargetKind => GnbHelper.SupportTargetKind.LowestHp;

	protected override string IconFile => "GnbHeartOfCorundumLowestHp.png";

	/// <summary>刚玉之心射程 30m（与骑士「干预」同档）。</summary>
	protected override float ConfiguredRange => 30f;
}
