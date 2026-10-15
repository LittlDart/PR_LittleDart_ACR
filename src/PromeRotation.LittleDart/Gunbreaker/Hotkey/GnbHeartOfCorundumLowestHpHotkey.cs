using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

public sealed class GnbHeartOfCorundumLowestHpHotkey : GnbSupportHotkeyBase
{
	protected override uint SkillId => GnbSkills.刚玉之心;

	protected override string SkillName => "刚玉之心";

	protected override GnbHelper.SupportTargetKind TargetKind => GnbHelper.SupportTargetKind.LowestHp;

	protected override string IconFile => "GnbHeartOfCorundumLowestHp.png";

		protected override float ConfiguredRange => 30f;
}
