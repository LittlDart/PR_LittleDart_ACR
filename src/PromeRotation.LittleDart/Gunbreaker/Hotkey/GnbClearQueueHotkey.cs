using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

public sealed class GnbClearQueueHotkey : IHotkeyLogic
{
	public bool IsActive()
	{
		return false;
	}

	public void OnClick()
	{

		GnbHelper.ClearHighPriorityQueues();
		GnbFeedback.Info("已清扫技能队列");
	}
}
