using System;
using PromeRotation.Data;

namespace LittleDart.Common.Sprint;

/// <summary>
/// 「自动冲刺」的**职业插槽**（公共逻辑 × 职业插槽：判据一行都不在这里，全在
/// <see cref="SprintResolverCore"/>）。
///
/// <para>两职业的默认值刻意完全一致（数值的**唯一权威**是 <see cref="SprintResolverCore"/> 的常量）：
/// 冲刺(Action 3) 是所有战斗职业共用的**角色技能**，没有职业差异 ——
/// 唯一值得分歧的是「数怪半径 / 距离区间」要不要让玩家调，所以做成了 <c>Func</c>，
/// 骑士侧读它自己的既有设置，绝枪侧读不动就落公共默认。</para>
/// </summary>
public sealed class SprintJobProfile
{
	public string JobName = string.Empty;

	public string LogPrefix = "[LittleDart/冲刺]";

	/// <summary>冲刺的技能 ID。两职业都是 3 —— 保留成插槽只是为了以后换职业时不必改核心。</summary>
	public uint SkillId = SprintResolverCore.SprintActionId;

	/// <summary>QT 键名（= 存档键名，改名 = 玩家配置失效）。两侧都用 <c>自动冲刺</c>。</summary>
	public string QtKey = "自动冲刺";

	/// <summary>数怪半径（米，**以我为中心**）。</summary>
	public Func<float> CountRadius = () => SprintResolverCore.DefaultEnemyCountRadius;

	/// <summary>「附近至少几只怪」才交冲刺。</summary>
	public Func<int> MinEnemies = () => SprintResolverCore.MinEnemyCount;

	/// <summary>到**当前选定目标**的距离区间（米）；读的是「到目标圈」的中心距减判定半径。</summary>
	public Func<float> MinTargetDistance = () => SprintResolverCore.DefaultMinTargetDistance;

	public Func<float> MaxTargetDistance = () => SprintResolverCore.DefaultMaxTargetDistance;

	/// <summary>
	/// 到当前选定目标的距离（米）。职业侧填自己的 <c>GetDistanceToTarget()</c>
	/// ——**两职业这份实现逐字相同**（中心距 − 我判定半径 − 目标判定半径），没目标时返回
	/// <c>float.MaxValue</c>。
	/// </summary>
	public Func<float> TargetDistance = () => float.MaxValue;

	/// <summary>
	/// 出手构造。★ 必须是 <see cref="ActionType.Always"/>（不是 OffGcd）—— 见
	/// <see cref="SprintResolverCore"/> 的类注释「为什么必须挂在 Always 槽」。
	/// </summary>
	public Func<PAction> BuildAction = () => new PAction(SprintResolverCore.SprintActionId, ActionType.Always, ActionTargetType.Self);
}
