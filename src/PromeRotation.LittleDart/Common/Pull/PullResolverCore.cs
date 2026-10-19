using System;
using System.Collections.Concurrent;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using LittleDart.Common.Mitigation.Calc;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Common.Pull;

/// <summary>
/// 位移技判定器的**唯一实现**（公共逻辑 × 职业插槽：绝枪 `GnbTrajectoryPullOffGcd`(弹道 36934) /
/// 骑士 `IntervenePullOffGcd`(调停 16461) 各是一个只填 <see cref="PullJobProfile"/> 的薄判定器，
/// 判据一行都不在职业侧）。
///
/// <para><b>两条分支</b>（按顺序问，第一条成立就出手）：</para>
/// <list type="number">
/// <item><b>贴身突进</b>（2026-10-15 玩家需求，两职业共用）：**当前选定目标**是敌人，且
/// **到目标圈**的距离落在开区间 <c>(<see cref="GapCloseMinDistance"/>,
/// <see cref="GapCloseMaxDistance"/>)</c> = (15, 20) 米，且对它放得出技能 ⇒ 贴上去。
/// 典型场景：被击退 / 被拉开距离后要立刻回近战。**不额外要求「战斗中」** —— 脱战时 PR 只有在
/// 「主动攻击 / AutoPull」开着（或倒计时在跑）时才会问到决策层。</item>
/// <item><b>拉怪</b>（2026-10-13 版，一字未改）：一仇数 ≥ <see cref="PullJobProfile.MinAggro"/>，
/// 且射程内有「不是我一仇 / 没进战斗」的怪 ⇒ 把它拽过来。脱战时一仇数恒为 0 ⇒ **脱战不拉**。</item>
/// </list>
///
/// <para>⚠️ <see cref="Check"/> 与 <see cref="Build"/> 必须用**同一份**判据选目标
/// （框架铁律②：Check 成功的每条路径 GetAction 都得给出真技能、且是同一只怪）——
/// 所以目标解析写成 <see cref="IsGapCloseTarget"/> + <see cref="EnemyCounter"/> 的组合，
/// 两边各调一次、不各自发挥。</para>
/// </summary>
public static class PullResolverCore
{
	private const int DiagIntervalMs = 1000;

	/// <summary>
	/// ★★ 2026-10-15 玩家需求（**两个职业共用本核心**）：「贴身突进」距离区间的**下限**（米）。
	///
	/// <para>口径 = **到目标圈**的距离，与 <c>PullJobProfile.Readings.DistanceTo</c> 同一份实现
	/// （中心距 − 目标判定半径，见 <c>GnbHelper.GetDistanceTo</c> / <c>PldHelper</c> 同名方法）。</para>
	///
	/// <para>区间是**开区间**：<c>GapCloseMinDistance &lt; 距离 &lt; GapCloseMaxDistance</c>
	/// ⇒ 当前选定目标在这条带里就交位移技贴上去（不满足时退回「拉怪」那条，见 <see cref="Check"/>）。</para>
	/// </summary>
	public const float GapCloseMinDistance = 15f;

	/// <summary>
	/// 「贴身突进」距离区间的**上限**（米）= 20，正是弹道(36934) 与调停(16461) 的游戏射程
	/// （<c>Action.Range</c>，2026-10-15 用游戏内表核过：两者都是 20、都是 2 层充能、
	/// 都 <c>AffectsPosition = 是</c> 且需要面向目标）。
	///
	/// <para>⚠️ 上下限是**写死的常量**（玩家给的口径），不是各自技能表里的 <c>Range</c> ——
	/// 以后某个职业换成射程不是 20 的位移技，请同时改这里。</para>
	/// </summary>
	public const float GapCloseMaxDistance = 20f;

	private static readonly ConcurrentDictionary<string, long> LastDiagTicks = new ConcurrentDictionary<string, long>();

	public static CheckResult Check(PullJobProfile slot)
	{
		if (!PromeSettings.Instance.GetQt(slot.QtKey))
		{
			return new CheckResult(false, "QT「" + slot.QtKey + "」没开");
		}
		if (!slot.IsUnlocked(slot.SkillId))
		{
			return new CheckResult(false, "拉怪技能还没解锁");
		}
		// ★★ 2026-10-15 玩家需求：**分支①「贴身突进」** —— 当前**选定**的目标在
		//   (15, 20) 米（到目标圈）⇒ 直接用位移技贴上去。
		//   它排在分支②之前，因为这是玩家**手动选好**的目标：不必再按「一仇数」判门槛，
		//   也不必从「漏网的怪」里挑（boss 把自己击退到 18 米外、单拉一个 boss 时
		//   「一仇数不够/目标正打着我」都会让分支②判不成立，而这时候正该贴回去）。
		//   ⚠️ 只有「在区间内 **且** 对它能放出位移技」才走这条；放不出时**落到分支②**，
		//   行为与改动前完全一致（例：当前目标被挡视线，但仍有一只更近的漏网怪可拽）。
		//   ⚠️ 本分支**不额外要求「战斗中」**：脱战时 PR 只有在「主动攻击 / AutoPull」开着
		//   （或倒计时在跑）时才会问到决策层，所以不会凭空脱战突进 —— 那种情况本来就是玩家
		//   自己开了「主动攻击」。这一点与分支②「脱战不拉」的差别写在 docs\21。
		IBattleChara? selected = Core.Target;
		if (selected != null && IsGapCloseTarget(slot, selected) && slot.IsReadyOnTarget(slot.SkillId, selected))
		{
			return new CheckResult(true, $"贴身突进：当前选定目标 {((IGameObject)selected).Name} 距 {slot.Readings.DistanceTo(selected):F1} 米（{GapCloseMinDistance:F0}~{GapCloseMaxDistance:F0} 米开区间）→ 贴上去");
		}
		// ---- 分支②：拉怪（把不是一仇 / 没进战斗的怪拽过来）· 原逻辑一字未改 ----
		int minAggro = slot.MinAggro();
		int aggroOnMeCount = EnemyCounter.CountAggroOnMe(slot.Readings);
		if (aggroOnMeCount < minAggro)
		{
			return new CheckResult(false, $"自己当一仇的怪不够多（{aggroOnMeCount} < {minAggro}）");
		}
		float actionRange = slot.ActionRange();
		if (actionRange <= 0f)
		{
			actionRange = slot.FallbackRange;
		}
		IBattleChara pullTarget = EnemyCounter.PickPullTarget(slot.Readings, actionRange);
		if (pullTarget == null)
		{
			return new CheckResult(false, "范围内没有需要拉的怪（都已经是我的仇 / 都在战斗状态）");
		}
		if (!slot.IsReadyOnTarget(slot.SkillId, pullTarget))
		{
			return new CheckResult(false, "对选中的那只怪放不出技能（不可选中 / 冷却中 / 刚放过）");
		}
		return new CheckResult(true, $"拉到漏网的怪：{((IGameObject)pullTarget).Name}（一仇 {aggroOnMeCount} 只）");
	}

	/// <summary>
	/// 「贴身突进」的候选判据（2026-10-15）：**当前选定目标**是不是在
	/// <c>(GapCloseMinDistance, GapCloseMaxDistance)</c> 米这条带里。
	///
	/// <para>要求目标存在、是**敌人**（位移技是敌对目标技：弹道 / 调停都
	/// <c>CanTargetHostile = 是</c>、<c>CanTargetSelf = 否</c>），距离落在开区间内。
	/// 距离读不出来时 <c>DistanceTo</c> 返回 <c>float.MaxValue</c>（或 NaN）⇒ 一律判否，
	/// 不会凭读不到的距离交位移。</para>
	///
	/// <para>「能不能放出去」不在这里判 —— 那是 <c>IsReadyOnTarget</c>（含射程 / 视线 / 冷却 /
	/// 刚放过）的事，<see cref="Check"/> 与 <see cref="Build"/> 两边都调它，保证判定与出手同源。</para>
	/// </summary>
	private static bool IsGapCloseTarget(PullJobProfile slot, IBattleChara? target)
	{
		if (target == null || !slot.Readings.IsEnemy(target))
		{
			return false;
		}
		float distance = slot.Readings.DistanceTo(target);
		return distance > GapCloseMinDistance && distance < GapCloseMaxDistance;
	}

	public static PAction Build(PullJobProfile slot)
	{
		// ★ 2026-10-15：出手目标与 Check 用**同一份判据**（铁律②：Check 成功的每条路径
		//   GetAction 都要给得出真技能，且必须是同一只怪）。两条分支的目标选择：
		//   ① 贴身突进 ⇒ 就是当前选定的那只；② 拉怪 ⇒ 照旧 PickPullTarget 挑漏网的怪。
		IBattleChara? selected = Core.Target;
		bool gapClose = selected != null && IsGapCloseTarget(slot, selected) && slot.IsReadyOnTarget(slot.SkillId, selected);
		float actionRange = slot.ActionRange();
		if (actionRange <= 0f)
		{
			actionRange = slot.FallbackRange;
		}
		IBattleChara? pullTarget = (gapClose ? selected : EnemyCounter.PickPullTarget(slot.Readings, actionRange));
		Diag(slot, pullTarget, actionRange, gapClose);
		PAction pullAction = slot.BuildAction();
		if (pullTarget == null)
		{
			return pullAction;
		}
		pullAction.NetworkTid = (uint)((IGameObject)pullTarget).GameObjectId;
		if (Core.Target == null)
		{
			Core.SetTarget(pullTarget);
		}
		return pullAction;
	}

	private static void Diag(PullJobProfile slot, IBattleChara? pick, float range, bool gapClose)
	{
		if (slot.DiagnosticsEnabled())
		{
			long tickCount = Environment.TickCount64;
			if (!LastDiagTicks.TryGetValue(slot.LogPrefix, out var lastDiagTicks) || tickCount - lastDiagTicks >= 1000)
			{
				LastDiagTicks[slot.LogPrefix] = tickCount;
				bool hasPick = pick != null;
				int aggroOnMeCount = EnemyCounter.CountAggroOnMe(slot.Readings);
				bool pickNotInCombat = hasPick && (((ICharacter)pick).StatusFlags & StatusFlags.InCombat) == 0;
				bool isReadyOnPick = hasPick && slot.IsReadyOnTarget(slot.SkillId, pick);
				slot.Log($"{slot.LogPrefix} 分支={(gapClose ? "贴身突进" : "拉怪")} qt={PromeSettings.Instance.GetQt(slot.QtKey)} 一仇={aggroOnMeCount} 门槛={slot.MinAggro()} 候选={(hasPick ? ((object)((IGameObject)pick).Name).ToString() : "无")} 未进战斗={(hasPick ? pickNotInCombat.ToString() : "n/a")} 距={(hasPick ? slot.Readings.DistanceTo(pick).ToString("F1") : "n/a")} 射程={range:F1} ready={isReadyOnPick}");
			}
		}
	}
}
