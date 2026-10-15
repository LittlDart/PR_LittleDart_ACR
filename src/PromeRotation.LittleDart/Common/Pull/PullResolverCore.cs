using System;
using System.Collections.Concurrent;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using LittleDart.Common.Mitigation.Calc;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Common.Pull;

public static class PullResolverCore
{
	private const int DiagIntervalMs = 1000;

		public const float GapCloseMinDistance = 15f;

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

		IBattleChara? selected = Core.Target;
		if (selected != null && IsGapCloseTarget(slot, selected) && slot.IsReadyOnTarget(slot.SkillId, selected))
		{
			return new CheckResult(true, $"贴身突进：当前选定目标 {((IGameObject)selected).Name} 距 {slot.Readings.DistanceTo(selected):F1} 米（{GapCloseMinDistance:F0}~{GapCloseMaxDistance:F0} 米开区间）→ 贴上去");
		}

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
