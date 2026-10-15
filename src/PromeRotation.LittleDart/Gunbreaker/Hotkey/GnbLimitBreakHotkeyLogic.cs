using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

public sealed class GnbLimitBreakHotkeyLogic : IHotkeyLogic
{
	public const uint IconActionId = 31399u;

	public bool IsActive()
	{
		return LimitBreakHelper.GetLimitBreakActionId() != 0;
	}

	public void OnClick()
	{
		uint limitBreakActionId = LimitBreakHelper.GetLimitBreakActionId();
		if (limitBreakActionId == 0)
		{
			GnbFeedback.Fail("LB 失败：极限技当前不可用（不在副本里 / 还没攒够）");
			return;
		}
		HotkeyQueueManager.TryEnqueue(new PAction(limitBreakActionId, ActionType.LimitBreak, ActionTargetType.Self));
		GnbFeedback.Info($"已触发极限技（技能 ID {limitBreakActionId}）");
	}
}
