using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.UI.HotKey;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 「退避」热键：把仇恨交给**队伍里的另一个坦克**（换 T 用）。
///
/// <para>与骑士 <c>ShirkToPartnerHotkey</c> 逐字同构，只有三处不同：技能常量（<see cref="GnbSkills.退避"/>）、
/// 找坦克的助手（<c>GnbHelper.GetAnotherTank</c>，它已封装「排除自己 + 活着的坦克」）、日志前缀。</para>
///
/// <para><see cref="IsActive"/> 只用来给按钮**上色 / 置灰**（PR 的按钮据此画激活态），
/// 不影响能不能点 —— 真正的出击条件在 <see cref="OnClick"/> 里重判一遍。</para>
/// </summary>
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
