using LittleDart.Common.Sprint;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.Always;

/// <summary>
/// 骑士「自动冲刺」（2026-10-15 起判据全部在公共层 <see cref="SprintResolverCore"/>，本类只填技能那一格）。
///
/// <list type="bullet">
/// <item>★ 挂在 <c>_alwaysResolvers</c>（不是 oGCD 列表）、出手用 <see cref="ActionType.Always"/>
/// —— 这是「不进战斗也触发」的唯一可行写法，依据见 <see cref="SprintResolverCore"/> 类注释。</item>
/// <item>数值：数怪半径读玩家既有设置 <c>MitigationEnemyCountRadius</c>（默认 10 米），
/// 距离区间读 <c>MitigationSprintMinRange / MaxRange</c>（默认 7 / 23 米）——
/// 与旧版 <c>OffGcd/SprintOffGcd.cs</c> 同源，玩家的老配置照旧生效。</item>
/// <item>怪数门槛**不再是**旧的 <c>MitigationSprintMinEnemies</c>（默认 4，且判据是
/// <c>nearby &lt;= 4</c> 其实要 5 只）—— 现在由公共层写死「≥3 只」，那三个
/// 旧常量已作废删除。</item>
/// </list>
/// </summary>
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
