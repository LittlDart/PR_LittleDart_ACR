using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

public sealed class GnbShirkToPartnerHotkey : IHotkeyLogic
{
	public bool IsActive()
	{
		if (ActionHelper.IsUnlocked(GnbSkills.退避) && ActionHelper.GetActionCooldown(GnbSkills.退避) <= 0f)
		{
			return GnbHelper.GetAnotherTank() != null;
		}
		return false;
	}

	public void OnClick()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.退避))
		{
			GnbFeedback.Fail("退避 失败：技能尚未解锁");
			return;
		}
		if (ActionHelper.GetActionCooldown(GnbSkills.退避) > 0f)
		{
			GnbFeedback.Fail("退避 失败：技能还在冷却");
			return;
		}
		IBattleChara? anotherTank = GnbHelper.GetAnotherTank();
		if (anotherTank == null)
		{
			GnbFeedback.Fail("退避 失败：队伍里没有另一个坦克");
			return;
		}
		HotkeyQueueManager.TryEnqueue(GnbHotkeys.OnTarget(GnbSkills.退避, ActionType.OffGcd, anotherTank));
		GnbFeedback.Info($"已退避 → {anotherTank.Name}");
	}
}
