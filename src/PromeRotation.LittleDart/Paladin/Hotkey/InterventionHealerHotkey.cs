using LittleDart.Paladin.Skill;
namespace LittleDart.Paladin.Hotkey;

public sealed class InterventionHealerHotkey : PldSupportHotkeyBase
{
	protected override uint SkillId => PldSkills.干预;

	protected override string SkillName => "干预";

	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Healer;

	protected override string IconFile => "InterventionHealer.png";

	protected override float ConfiguredRange => 30f;
}
