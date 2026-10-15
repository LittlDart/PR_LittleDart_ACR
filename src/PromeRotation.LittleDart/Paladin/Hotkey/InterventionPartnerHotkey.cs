namespace LittleDart.Paladin.Hotkey;

public sealed class InterventionPartnerHotkey : PldSupportHotkeyBase
{
	protected override uint SkillId => 7382u;

	protected override string SkillName => "干预";

	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Partner;

	protected override string IconFile => "InterventionPartner.png";

	protected override float ConfiguredRange => 30f;
}
