using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.Hotkey;

public sealed class PldLimitBreakHotkeyLogic : IHotkeyLogic
{
	public const uint IconActionId = PldSkills.极限技;

	public bool IsActive()
	{
		return LimitBreakHelper.GetLimitBreakActionId() != 0;
	}

	public void OnClick()
	{
		uint limitBreakActionId = LimitBreakHelper.GetLimitBreakActionId();
		if (limitBreakActionId == 0)
		{
			PldFeedback.Fail("LB 失败：极限技当前不可用（不在副本里 / 还没攒够）");
			return;
		}
		HotkeyQueueManager.TryEnqueue(new PAction(limitBreakActionId, ActionType.LimitBreak, ActionTargetType.Self));
		PldFeedback.Info($"已触发极限技（技能 ID {limitBreakActionId}）");
	}
}
