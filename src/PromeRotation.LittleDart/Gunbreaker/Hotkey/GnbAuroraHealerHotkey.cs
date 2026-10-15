using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 「极光 → 治疗」热键（六宫格 · 第 2 行第 2 列）。
/// 技能 = <see cref="GnbSkills.极光"/>（16151，Regen oGCD；给目标挂 <see cref="GnbSkills.极光buff"/> 1835）。
/// </summary>
public sealed class GnbAuroraHealerHotkey : GnbSupportHotkeyBase
{
	protected override uint SkillId => GnbSkills.极光;

	protected override string SkillName => "极光";

	protected override GnbHelper.SupportTargetKind TargetKind => GnbHelper.SupportTargetKind.Healer;

	protected override string IconFile => "GnbAuroraHealer.png";

	/// <summary>极光射程 30m（与骑士「干预」同档）。</summary>
	protected override float ConfiguredRange => 30f;
}
