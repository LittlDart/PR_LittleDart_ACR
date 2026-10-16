using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Paladin;

public static class PldAction
{
	public static PAction Of(uint actionId, ActionType type)
	{
		ActionTargetType resolvedTarget = default;
		ActionTargetType targetType = (ActionTargetType)((!ActionHelper.TryResolveActionTargetType(actionId, out resolvedTarget)) ? 1 : ((int)resolvedTarget));
		return new PAction(actionId, type, targetType);
	}

	/// <summary>
	/// ★★ 2026-10-16 玩家口径（与绝枪那一轮同一套）：**需要选中一只怪的群体技能**把出手目标钉到
	/// <paramref name="target"/>（AOE 成立时 = 5 米内离自己最近的那只），从而**不依赖玩家当前选中的目标**。
	///
	/// <para><b>为什么用 <c>NetworkTid</c> 而不是 <c>Core.SetTarget</c>：</b>
	/// PR 的 PvE 派发（<c>ActionUpdater.TryExecuteAction</c>，impl:11645）会**先**
	/// <c>TargetResolver.Resolve(action.Target)</c>，解析不到直接 <c>return false</c>
	/// —— **这一步在 NetworkTid 之前**。所以「<c>Target</c> 类型 + 玩家没选目标」这一格，
	/// 保持 <c>Target</c> 类型只能靠改游戏里的选中目标来救（会跟玩家抢选中框）。
	/// PR 自己对时间轴技能用的正是本方法这一套：<c>Target = Self</c>（解析必定非空 ⇒ 不会提前 return）
	/// + <c>NetworkTid</c> = 目标实体 Id（impl:42228-42232），
	/// 而 NetworkTid 会覆盖真正传给 <c>UseAction</c> 的那个 id（impl:11651 / 13333）。
	/// 同一套写在绝枪侧 <c>GnbAction.Aoe</c>（GNB 先落地，骑士这一轮照抄）。</para>
	///
	/// <para>技能本身若是**自身中心**（Action 表 Range = 0：全蚀斩 7381 / 日珥斩 16457 /
	/// 圣环 16458 / 厄运流转 23），<see cref="Of"/> 已经解析成 <c>Self</c>
	/// ⇒ **原样返回、一个字段都不改**。</para>
	/// </summary>
	public static PAction Aoe(uint actionId, ActionType type, IBattleChara? target)
	{
		PAction action = Of(actionId, type);
		if (target == null || action.Target != ActionTargetType.Target)
		{
			return action;
		}
		return new PAction(actionId, type, ActionTargetType.Self)
		{
			NetworkTid = ((IGameObject)target).EntityId
		};
	}

	public static PAction Self(uint actionId, ActionType type)
	{
		return new PAction(actionId, type, (ActionTargetType)0);
	}

	public static PAction Enemy(uint actionId, ActionType type)
	{
		return new PAction(actionId, type, (ActionTargetType)1);
	}

	public static PAction On(uint actionId, ActionType type, ActionTargetType target)
	{
		return new PAction(actionId, type, target);
	}
}
