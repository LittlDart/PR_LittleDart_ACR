namespace PromeRotation.Sample.Actions;

/// <summary>
/// 技能 ID 常量表。
/// </summary>
/// <remarks>
/// 约定（从 AE 时代继承下来的做法）：<b>代码里不许出现裸数字技能 ID</b>，全都在这里起名字。
/// 好处是换了版本改一个数字就行，而且读代码时能直接看出打的是哪个技能。
/// </remarks>
public static class ActionIds
{
    /// <summary>
    /// 占位技能。真正的 ACR 请把它换成你会用到的技能，例如：
    /// <code>public const uint Glare = 16533;   // 闪耀</code>
    /// </summary>
    public const uint Placeholder = 0;
}
