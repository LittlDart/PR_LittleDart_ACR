using LittleDart.Common.Sprint;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.Always;

/// <summary>
/// 绝枪战士「自动冲刺」（2026-10-15 补齐：**原来是占位**，<c>CheckReal()</c> 只返回「尚未实现」，
/// QT 拨开也永远不放）。
///
/// <list type="bullet">
/// <item>★ 与骑士**同一个公共判据** <see cref="SprintResolverCore"/>（QT 名、技能 ID、怪数门槛
/// ≥3、距离区间 7~23 米全部同口径），本类只填技能那一格。</item>
/// <item>★ 挂在 <c>_alwaysResolvers</c>（不是 oGCD 列表）、出手用 <see cref="ActionType.Always"/>
/// —— 这是「不进战斗也触发」的唯一可行写法，依据见 <see cref="SprintResolverCore"/> 类注释。</item>
/// <item>数怪半径读 <c>GnbSettings.MitigationEnemyCountRadius</c>（默认 10 米）；
/// 距离区间**没有**对应的绝枪设置字段 ⇒ 直接用公共默认 7 / 23 米（与骑士同值）。</item>
/// </list>
/// </summary>
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
