using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;

namespace LittleDart.Paladin.Hotkey;

public sealed class ShirkToPartnerHotkey : IHotkeyLogic
{
	public bool IsActive()
	{
		if (ActionHelper.IsUnlocked(7537u) && ActionHelper.GetActionCooldown(7537u) <= 0f)
		{
			return PldHelper.GetAnotherTank() != null;
		}
		return false;
	}

	public void OnClick()
	{
		if (!ActionHelper.IsUnlocked(7537u))
		{
			PldFeedback.Fail("退避 失败：技能尚未解锁");
			return;
		}
		if (ActionHelper.GetActionCooldown(7537u) > 0f)
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
		HotkeyQueueManager.TryEnqueue(PldHotkeys.OnTarget(7537u, (ActionType)1, anotherTank));
		PldFeedback.Info($"已退避 → {((IGameObject)anotherTank).Name}");
	}
}
