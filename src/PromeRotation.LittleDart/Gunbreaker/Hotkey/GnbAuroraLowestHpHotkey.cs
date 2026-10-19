using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 「极光 → 血量最低的队友」热键（六宫格 · 第 3 行第 2 列）。
///
/// <para>技能 = <see cref="GnbSkills.极光"/>（16151，Regen oGCD；给目标挂 <see cref="GnbSkills.极光buff"/> 1835）。</para>
///
/// <para><b>「血量最低」判的是绝对值</b>（与同排的刚玉之心那一格、以及骑士侧同口径）。</para>
/// </summary>
public sealed class GnbAuroraLowestHpHotkey : GnbSupportHotkeyBase
{
	protected override uint SkillId => GnbSkills.极光;

	protected override string SkillName => "极光";

	protected override GnbHelper.SupportTargetKind TargetKind => GnbHelper.SupportTargetKind.LowestHp;

	protected override string IconFile => "GnbAuroraLowestHp.png";

	/// <summary>极光射程 30m（与骑士「干预」同档）。</summary>
	protected override float ConfiguredRange => 30f;
}
