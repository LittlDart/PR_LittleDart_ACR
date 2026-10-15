using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Common.Sprint;

/// <summary>
/// 「自动冲刺」判定器的**唯一实现**（2026-10-15 玩家需求：把骑士那份私有实现提到公共层，
/// 骑士 / 绝枪各挂一个只填 <see cref="SprintJobProfile"/> 的薄判定器）。
///
/// <para><b>判据（按顺序问，全过才交）</b>：QT「自动冲刺」开着 → 冲刺已解锁 →
/// 本地 500ms 内没交过 → <c>CanCast</c> → <b>我身边 <c>CountRadius</c> 米内 ≥
/// <see cref="MinEnemyCount"/> 只怪</b>（PR 的 <c>TargetHelper.EnemyInRange</c>）→
/// 当前选定目标到我的距离落在 <c>[MinTargetDistance, MaxTargetDistance]</c>。</para>
///
/// <para><b>★ 为什么必须挂在 Always 槽（<c>_alwaysResolvers</c>）+ <c>ActionType.Always</c>
/// —— 这是「不进战斗也触发」的唯一可行写法</b>（2026-10-15 读 PR 1.5.10.4 反编译确认）：</para>
/// <list type="number">
/// <item><b>决策侧</b>：<c>_offGcdResolvers</c> 只在**有 GCD 在转**时才会被问到
/// （<c>RotationManager.TickPve</c>：先判 <c>gcdRemain &gt; 0.6f</c> 才
/// <c>ProcessOffGcdDecision()</c>，impl:91794）。脱战时没有 GCD 在转 ⇒ 挂在 oGCD 列表里
/// **永远问不到** ⇒ 脱战不冲。</item>
/// <item><b>执行侧</b>：<c>ActionQueueManager.TryEnqueue</c> 是**按 <c>pAction.Type</c> 选队列**的
/// （Gcd→gcd 队列 / OffGcd→ogcd 队列 / Always→always 队列）。
/// <c>ActionUpdater.ExecuteCombat</c> 每帧**无条件**先取 always 队列（impl:10945），
/// 且 <c>GetIsTimely</c> 对 <see cref="ActionType.Always"/> **直接 return true**（impl:11425）
/// —— 既不看 GCD 窗口也不看 weave delay。</item>
/// <item>⚠️ 反过来：如果挂在 Always 列表里却返回 <c>ActionType.OffGcd</c>，这个技能会进
/// **oGCD 队列**，而该队列只在 <c>gcdRemain ≥ 0.6</c> 时被取 ⇒ 脱战时入队后**烂在队列里**
/// （出战斗的那一瞬间不会有 GCD 帮你把它取出来）。两处必须**同时**是 Always。</item>
/// </list>
///
/// <para><b>⚠️ 框架仍有一道「脱战闸门」不是这里能绕开的</b>：脱战时 PR 只在
/// <c>PromeSettings.ShouldExecuteAcr()</c> 为真时才走 <c>ExecuteCombat</c>，而它是
/// <c>if (!IsInCombat &amp;&amp; !AutoPull) return IsTargetInCombat();</c>（impl:124979）
/// ⇒ **脱战要真的冲出去，得满足其一**：① QT「主动攻击」（<c>AutoPull</c>）开着；
/// ② **当前选定的目标自己处于战斗状态**（<c>GameData.IsTargetInCombat</c>，impl:125364）。
/// 本判定器自身**不含任何「战斗中」条件**（这是玩家口径），上面那道闸门是 PR 的。</para>
///
/// <para><b>为什么 <see cref="Check"/> 是纯函数、不留「刚交过」的状态</b>：PR 每帧调
/// <c>Check()</c> 刷调试面板（<c>RotationManager</c>/ACR 的 <c>UpdateDebugStatus</c>），
/// 在里面写状态会让调试面板把真技能锁掉。防「同一个冲刺连入队两次」靠
/// <c>ActionHelper.RecentlyUsed(SkillId, 500)</c> —— 本 ACR 的 <c>Resolve()</c> 出手时会
/// 自己调 <c>ActionHelper.RecordAction</c>（<c>LittleDartRotation.cs</c> 的 <c>Resolve</c>），
/// 所以这个 500ms 窗口在本工程里是**真的有效**的（不是技能文档里警告的「恒 false」那种情况）；
/// 再叠一层游戏自己的冲刺冷却（<c>CanCast</c>）。</para>
/// </summary>
public static class SprintResolverCore
{
	/// <summary>冲刺（Sprint）的技能 ID。全职业共用的角色技能。</summary>
	public const uint SprintActionId = 3u;

	/// <summary>
	/// 身边至少这么多只怪才交冲刺（2026-10-15 玩家口径：**≥3 只即可**）。
	///
	/// <para>⚠️ 判据写的是 <c>nearby &gt;= MinEnemyCount</c>（**3 只就够**）——
	/// 骑士旧实现是 <c>nearby &lt;= MitigationSprintMinEnemies</c>（写 4 其实要 5 只），
	/// 那个「名字叫 Min 实际是 Min+1」的坑已随旧实现一起作废，别再照抄。</para>
	/// </summary>
	public const int MinEnemyCount = 3;

	/// <summary>数怪半径（米）的公共默认值 —— 与两职业设置里的 <c>MitigationEnemyCountRadius</c> 默认值一致。</summary>
	public const float DefaultEnemyCountRadius = 10f;

	public const float DefaultMinTargetDistance = 7f;

	public const float DefaultMaxTargetDistance = 23f;

	/// <summary>本地「刚交过」窗口（毫秒）。</summary>
	private const int RecentlyUsedWindowMs = 500;

	public static CheckResult Check(SprintJobProfile slot)
	{
		if (!PromeSettings.Instance.GetQt(slot.QtKey))
		{
			return new CheckResult(false, "QT「" + slot.QtKey + "」没开");
		}
		if (Core.Me == null)
		{
			return new CheckResult(false, "玩家还没加载出来");
		}
		if (!ActionHelper.IsUnlocked(slot.SkillId))
		{
			return new CheckResult(false, "冲刺还没解锁（等级不够 / 前置任务没做）");
		}
		if (ActionHelper.RecentlyUsed(slot.SkillId, RecentlyUsedWindowMs))
		{
			return new CheckResult(false, $"刚交出过冲刺（本 ACR 记的 {RecentlyUsedWindowMs}ms 防连点窗口内）");
		}
		if (!ActionHelper.CanCast(slot.SkillId, null))
		{
			return new CheckResult(false, "冲刺现在放不出（冷却中 / 状态不允许）");
		}
		// ★ 敌人数用 PR 的公开 API（不再是各职业私有的 GetNearbyEnemyCount）：
		//   TargetHelper.EnemyInRange 只数「可选中 && 没死 && IsEnemy」的怪，
		//   距离算的是**去掉双方判定半径**的中心距（贴脸的怪更容易被算进来）。
		float radius = slot.CountRadius();
		int minEnemies = slot.MinEnemies();
		int nearbyEnemyCount = (int)TargetHelper.EnemyInRange(radius);
		if (nearbyEnemyCount < minEnemies)
		{
			return new CheckResult(false, $"身边敌人不够多（{nearbyEnemyCount} < {minEnemies}，{radius:F0} 米内）");
		}
		if (Core.Target == null)
		{
			return new CheckResult(false, "没有选定目标 ⇒ 判不了冲刺距离区间");
		}
		float distance = slot.TargetDistance();
		float minDistance = slot.MinTargetDistance();
		float maxDistance = slot.MaxTargetDistance();
		if (!(distance >= minDistance) || !(distance <= maxDistance))
		{
			return new CheckResult(false, $"不在冲刺距离区间 [{minDistance:F0}, {maxDistance:F0}] 内（当前 {distance:F1} 米）");
		}
		return new CheckResult(true, $"{radius:F0} 米内有 {nearbyEnemyCount} 只怪（≥{minEnemies}），目标距 {distance:F1} 米 ∈ [{minDistance:F0}, {maxDistance:F0}] → 自动冲刺");
	}

	public static PAction Build(SprintJobProfile slot)
	{
		return slot.BuildAction();
	}
}
