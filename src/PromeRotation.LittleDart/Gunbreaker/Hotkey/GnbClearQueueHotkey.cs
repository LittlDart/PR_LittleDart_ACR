using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 「清扫队列」热键：把 PR 的技能队列（普通 + 强制）整个清掉。
///
/// <para>用途：时间轴 / 起手往队列里塞了技能、但你不想让它按原计划打出来时，
/// 点一下立刻清空（例如 BOSS 提前转阶段、起手被打断）。</para>
///
/// <para>它不是技能，所以 <see cref="IsActive"/> 恒 false（按钮永远不上色），
/// 图标由面板注册处传的 <c>ClearQueue.png</c> 提供。</para>
/// </summary>
public sealed class GnbClearQueueHotkey : IHotkeyLogic
{
	public bool IsActive()
	{
		return false;
	}

	public void OnClick()
	{
		// 清队列本身在 GnbHelper 里（与 `/LittleDart-GNB clear` 同一个实现）。
		GnbHelper.ClearHighPriorityQueues();
		GnbFeedback.Info("已清扫技能队列");
	}
}
