using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 「刚玉之心 → 搭档坦克」热键（六宫格 · 第 1 行第 1 列）。
/// 技能 = <see cref="GnbSkills.刚玉之心"/>（25758，减伤 oGCD）。
/// </summary>
public sealed class GnbHeartOfCorundumPartnerHotkey : GnbSupportHotkeyBase
{
	protected override uint SkillId => GnbSkills.刚玉之心;

	protected override string SkillName => "刚玉之心";

	protected override GnbHelper.SupportTargetKind TargetKind => GnbHelper.SupportTargetKind.Partner;

	protected override string IconFile => "GnbHeartOfCorundumPartner.png";

	/// <summary>刚玉之心射程 30m（与骑士「干预」同档）。</summary>
	protected override float ConfiguredRange => 30f;
}
