using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Hotkey;

public sealed class ShirkToPartnerHotkey : IHotkeyLogic
{
	public bool IsActive()
	{
		if (ActionHelper.IsUnlocked(PldSkills.退避) && ActionHelper.GetActionCooldown(PldSkills.退避) <= 0f)
		{
			return PldHelper.GetAnotherTank() != null;
		}
		return false;
	}

	public void OnClick()
	{
		if (!ActionHelper.IsUnlocked(PldSkills.退避))
		{
			PldFeedback.Fail("退避 失败：技能尚未解锁");
			return;
		}
		if (ActionHelper.GetActionCooldown(PldSkills.退避) > 0f)
		{
			PldFeedback.Fail("退避 失败：技能还在冷却");
			return;
		}
		IBattleChara anotherTank = PldHelper.GetAnotherTank();
		if (anotherTank == null)
		{
			PldFeedback.Fail("退避 失败：队伍里没有另一个坦克");
			return;
		}
		HotkeyQueueManager.TryEnqueue(PldHotkeys.OnTarget(PldSkills.退避, ActionType.OffGcd, anotherTank));
		PldFeedback.Info($"已退避 → {((IGameObject)anotherTank).Name}");
	}
}
