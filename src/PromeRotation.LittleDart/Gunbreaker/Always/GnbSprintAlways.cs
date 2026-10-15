using LittleDart.Common.Sprint;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.Always;

public class GnbSprintAlways : GnbResolverBase
{
	private static readonly SprintJobProfile Slot = new SprintJobProfile
	{
		JobName = "绝枪战士",
		LogPrefix = "[LittleDart-GNB/冲刺]",
		SkillId = SprintResolverCore.SprintActionId,
		QtKey = GnbQt.AutoSprint,
		CountRadius = () => GnbSettings.Instance?.MitigationEnemyCountRadius ?? SprintResolverCore.DefaultEnemyCountRadius,
		TargetDistance = GnbHelper.GetDistanceToTarget,
		BuildAction = () => GnbAction.Self(SprintResolverCore.SprintActionId, ActionType.Always)
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
