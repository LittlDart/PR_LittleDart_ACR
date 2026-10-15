using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Common.Sprint;

public static class SprintResolverCore
{
	public const uint SprintActionId = 3u;

	public const int MinEnemyCount = 3;

	public const float DefaultEnemyCountRadius = 10f;

	public const float DefaultMinTargetDistance = 7f;

	public const float DefaultMaxTargetDistance = 23f;

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
