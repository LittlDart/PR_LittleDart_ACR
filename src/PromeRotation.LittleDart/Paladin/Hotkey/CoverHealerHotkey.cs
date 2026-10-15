namespace LittleDart.Paladin.Hotkey;

public sealed class CoverHealerHotkey : CoverPartnerHotkey
{
	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Healer;

	protected override string IconFile => "CoverHealer.png";
}
