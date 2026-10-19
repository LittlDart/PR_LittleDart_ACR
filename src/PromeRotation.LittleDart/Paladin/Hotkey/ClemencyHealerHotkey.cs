namespace LittleDart.Paladin.Hotkey;

public sealed class ClemencyHealerHotkey : ClemencyPartnerHotkey
{
	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Healer;

	protected override string IconFile => "ClemencyHealer.png";
}
