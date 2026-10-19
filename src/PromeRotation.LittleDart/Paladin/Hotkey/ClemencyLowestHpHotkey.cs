namespace LittleDart.Paladin.Hotkey;

public sealed class ClemencyLowestHpHotkey : ClemencyPartnerHotkey
{
	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.LowestHp;

	protected override string IconFile => "ClemencyLowestHp.png";
}
