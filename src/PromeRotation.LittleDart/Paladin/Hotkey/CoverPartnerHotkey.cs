namespace LittleDart.Paladin.Hotkey;

public class CoverPartnerHotkey : PldSupportHotkeyBase
{
	protected override uint SkillId => 27u;

	protected override string SkillName => "保护";

	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Partner;

	protected override string IconFile => "CoverPartner.png";

	protected override float ConfiguredRange => 20f;
}
