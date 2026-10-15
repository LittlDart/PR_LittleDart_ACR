using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;

namespace LittleDart.Paladin.Hotkey;

public class ClemencyPartnerHotkey : PldSupportHotkeyBase
{
	protected override uint SkillId => 3541u;

	protected override string SkillName => "深仁厚泽";

	protected override PldHelper.SupportTargetKind TargetKind => PldHelper.SupportTargetKind.Partner;

	protected override string IconFile => "ClemencyPartner.png";

	protected override float ConfiguredRange => 30f;

	protected override bool IsGcd => true;

	protected override bool HasResource()
	{
		uint minMp = PldSettings.Instance?.ClemencyMinMp ?? 2000;
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return ((ICharacter)me).CurrentMp >= minMp;
		}
		return false;
	}

	protected override bool IsSkillReady()
	{
		IBattleChara me = Core.Me;
		if (me != null)
		{
			return !me.IsCasting;
		}
		return false;
	}
}
