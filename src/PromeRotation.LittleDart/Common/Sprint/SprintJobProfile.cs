using System;
using PromeRotation.Data;

namespace LittleDart.Common.Sprint;

public sealed class SprintJobProfile
{
	public string JobName = string.Empty;

	public string LogPrefix = "[LittleDart/冲刺]";

		public uint SkillId = SprintResolverCore.SprintActionId;

		public string QtKey = "自动冲刺";

		public Func<float> CountRadius = () => SprintResolverCore.DefaultEnemyCountRadius;

		public Func<int> MinEnemies = () => SprintResolverCore.MinEnemyCount;

		public Func<float> MinTargetDistance = () => SprintResolverCore.DefaultMinTargetDistance;

	public Func<float> MaxTargetDistance = () => SprintResolverCore.DefaultMaxTargetDistance;

		public Func<float> TargetDistance = () => float.MaxValue;

		public Func<PAction> BuildAction = () => new PAction(SprintResolverCore.SprintActionId, ActionType.Always, ActionTargetType.Self);
}
