using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

public sealed class GnbAuroraLowestHpHotkey : GnbSupportHotkeyBase
{
	protected override uint SkillId => GnbSkills.极光;

	protected override string SkillName => "极光";

	protected override GnbHelper.SupportTargetKind TargetKind => GnbHelper.SupportTargetKind.LowestHp;

	protected override string IconFile => "GnbAuroraLowestHp.png";

		protected override float ConfiguredRange => 30f;
}
