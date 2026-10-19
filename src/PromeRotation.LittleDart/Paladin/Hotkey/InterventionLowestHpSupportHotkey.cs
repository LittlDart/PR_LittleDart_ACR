using LittleDart.Paladin.Skill;
namespace LittleDart.Paladin.Hotkey;

public sealed class InterventionLowestHpSupportHotkey : PldSupportHotkeyBase
{
	protected override uint SkillId => PldSkills.干预;

	protected override string SkillName => "干预";

	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.LowestHp;

	protected override string IconFile => "InterventionLowestHp.png";

	protected override float ConfiguredRange => 30f;
}
