using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Skill;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

/// <summary>
/// 等级档（70 / 80 / 90）的 GCD 队列基类：**只做「按本档顺序问第一个成立的决策器」**。
///
/// <para><b>★ 2026-10-18 官方口径整改（S5 / CHECKLIST C2）：</b>
/// 旧实现把队列做成 <c>_queue ?? (_queue = CreateQueue())</c> 的懒加载 ——
/// 第一次 <c>Check()</c> 会写字段（PR 每帧调 Check 刷调试面板）。现在队列在**构造时**注入，
/// Check 全程只读。另：<c>GetAction()</c> 不再用「队列第 0 格」兜底（那可能是个没有判据的技能），
/// 状态在 Check/GetAction 之间漂移时给一个永远合法的起手技能（先锋剑）。</para>
/// </summary>
public abstract class PldBracketQueueGcd : PldResolverBase
{
	private readonly PldResolverBase[] _queue;

	protected PldBracketQueueGcd(PldResolverBase[] queue)
	{
		_queue = queue;
	}

	protected abstract int BracketLevel { get; }

	private PldResolverBase[] Queue => _queue;

	protected override CheckResult CheckReal()
	{
		IBattleChara me = Core.Me;
		if (((me != null) ? ((ICharacter)me).Level : 0) != BracketLevel)
		{
			return new CheckResult(false, $"不是我的等级档（要精确 {BracketLevel} 级）");
		}
		if (TryPick(out _, out string message))
		{
			return new CheckResult(true, $"等级档 {BracketLevel}：{message}");
		}
		return new CheckResult(false, $"等级档 {BracketLevel}：本档队列里没有可用技能");
	}

	/// <summary>纯判据：按顺序问第一个成立的决策器（不写任何字段）。</summary>
	private bool TryPick(out PldResolverBase? pick, out string message)
	{
		PldResolverBase[] queue = Queue;
		for (int i = 0; i < queue.Length; i++)
		{
			CheckResult queueCheck = queue[i].Check();
			if (queueCheck.Success)
			{
				pick = queue[i];
				message = queueCheck.Message;
				return true;
			}
		}
		pick = null;
		message = string.Empty;
		return false;
	}

	public override PAction GetAction()
	{
		if (TryPick(out PldResolverBase? pick, out _))
		{
			return pick!.GetAction();
		}
		// Check 通过与 GetAction 在同一调用栈内，正常情况下不会走到这里；
		// 万一漂移，给一个无前置的真技能（基础连击起手），不用队列第 0 格冒险。
		return PldAction.Of(PldSkills.先锋剑, ActionType.Gcd);
	}
}
