using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

public sealed class GnbAuroraHealerHotkey : GnbSupportHotkeyBase
{
	protected override uint SkillId => GnbSkills.极光;

	protected override string SkillName => "极光";

	protected override GnbHelper.SupportTargetKind TargetKind => GnbHelper.SupportTargetKind.Healer;

	protected override string IconFile => "GnbAuroraHealer.png";

		protected override float ConfiguredRange => 30f;
}
