using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 「LB」热键：放当前这一档极限技。
///
/// <para>极限技的**技能 ID 是运行时决定的**（一段 / 二段 / 三段各一个 ID），
/// 所以不能像别的按钮那样写死一个 action，必须按一下现取 —— 这就是它要做成
/// <see cref="IHotkeyLogic"/> 而不是普通按钮的原因。取不到 = 不在副本里 / 还没攒够。</para>
///
/// <para><see cref="IconActionId"/> 只是**按钮图标**用的（三段 LB 的图标），与真正放出来的那一档无关。</para>
/// </summary>
public sealed class GnbLimitBreakHotkeyLogic : IHotkeyLogic
{
	/// <summary>按钮图标（三段极限技），不是真正出手的技能 ID。</summary>
	public const uint IconActionId = GnbSkills.极限技;

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
