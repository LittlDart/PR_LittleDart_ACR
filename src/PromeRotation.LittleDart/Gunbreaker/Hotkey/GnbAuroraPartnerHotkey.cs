using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

public sealed class GnbAuroraPartnerHotkey : GnbSupportHotkeyBase
{
	protected override uint SkillId => GnbSkills.极光;

	protected override string SkillName => "极光";

	protected override GnbHelper.SupportTargetKind TargetKind => GnbHelper.SupportTargetKind.Partner;

	protected override string IconFile => "GnbAuroraPartner.png";

	protected override float ConfiguredRange => 30f;
}
