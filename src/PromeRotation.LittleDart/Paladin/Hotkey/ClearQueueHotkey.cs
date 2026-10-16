using PromeRotation.UI.HotKey;

namespace LittleDart.Paladin.Hotkey;

public sealed class ClearQueueHotkey : IHotkeyLogic
{
	public bool IsActive()
	{
		return false;
	}

	public void OnClick()
	{
		PldHelper.ClearHighPriorityQueues();
		PldFeedback.Info("已清扫技能队列");
	}
}
