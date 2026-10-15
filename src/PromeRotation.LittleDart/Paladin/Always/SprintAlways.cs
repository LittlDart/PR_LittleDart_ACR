using LittleDart.Common.Sprint;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.Always;

public class SprintAlways : PldResolverBase
{
	private static readonly SprintJobProfile Slot = new SprintJobProfile
	{
		JobName = "骑士",
		LogPrefix = "[LittleDart-PLD/冲刺]",
		SkillId = SprintResolverCore.SprintActionId,
		QtKey = PldQt.AutoSprint,
		CountRadius = () => PldSettings.Instance?.MitigationEnemyCountRadius ?? SprintResolverCore.DefaultEnemyCountRadius,
		MinTargetDistance = () => PldSettings.Instance?.MitigationSprintMinRange ?? SprintResolverCore.DefaultMinTargetDistance,
		MaxTargetDistance = () => PldSettings.Instance?.MitigationSprintMaxRange ?? SprintResolverCore.DefaultMaxTargetDistance,
		TargetDistance = PldHelper.GetDistanceToTarget,
		BuildAction = () => PldAction.Self(SprintResolverCore.SprintActionId, ActionType.Always)
	};

	protected override string ResolverDescription => "Always：冲刺（自动 · 公共层判定：身边 ≥3 只怪 + 目标在距离区间内）";

	protected override CheckResult CheckReal()
	{
		return SprintResolverCore.Check(Slot);
	}

	public override PAction GetAction()
	{
		return SprintResolverCore.Build(Slot);
	}
}
