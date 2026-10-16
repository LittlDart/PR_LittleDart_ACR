namespace LittleDart.Paladin.Hotkey;

public sealed class CoverLowestHpHotkey : CoverPartnerHotkey
{
	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.LowestHp;

	protected override string IconFile => "CoverLowestHp.png";
}
