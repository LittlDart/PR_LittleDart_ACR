using PromeRotation.Data;
using PromeRotation.Resolvers;
using PromeRotation.Sample.Actions;

namespace PromeRotation.Sample.Resolvers.Gcd;

/// <summary>
/// 占位 GCD 决策器：<b>永远不通过</b>。
/// </summary>
/// <remarks>
/// <para>
/// 它的作用是让你有一个能编译、能加载、但<b>不会真的出手</b>的骨架。
/// 写自己的第一格时，照抄这个文件，然后把 <see cref="Check"/> 换成真条件。
/// </para>
/// <para>
/// ★ PR 和 AE 一样是「<b>第一个 Check 通过的决策器赢</b>」（见 <c>SimpleRotation.NextGcd</c>），
/// 所以 <b>决策器在列表里的顺序 = 优先级</b>。改了条件但没挪列表位置 = 轴没变。
/// </para>
/// </remarks>
public sealed class PlaceholderGcd : IDecisionResolver
{
    /// <summary>
    /// 检查「现在该不该用这一格」。
    /// </summary>
    /// <remarks>
    /// 条件请按「<b>便宜 → 贵</b>」排序：开关 → 等级 → buff/连击 → 距离/敌人数。
    /// 返回的 Message 会出现在 PR 的调试状态里，所以写成自己看得懂的中文。
    /// </remarks>
    public CheckResult Check()
    {
        return new CheckResult(false, "占位决策器：还没实现");
    }

    /// <summary>
    /// 告诉 PR「准备用什么技能、是 GCD 还是能力技、目标是谁」。
    /// </summary>
    /// <remarks>
    /// ⚠️ 铁律（从 AE 时代继承）：<b><see cref="Check"/> 返回 true 的所有路径，
    /// 这里都必须能给出一个真技能。</b>否则框架会「以为你要出手，结果你空手」，
    /// 表现为那一格卡住或者刷屏告警。
    /// </remarks>
    public PAction GetAction()
    {
        // GetAdjustedActionId() 用来取「当前状态下这个技能实际对应的 ID」——
        // 技能被升级替代时（如闪耀 → 闪灼）会返回升级后的那个。
        // 它来自 PromeRotation.Extensions.SkillExtensions。
        return new PAction(ActionIds.Placeholder, ActionType.Gcd, ActionTargetType.Target);
    }
}
