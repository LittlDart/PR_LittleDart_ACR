using Dalamud.Bindings.ImGui;
using PromeRotation.Data;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using PromeRotation.Rotation;
using PromeRotation.Sample.Events;
using PromeRotation.Sample.Resolvers.Gcd;

namespace PromeRotation.Sample;

/// <summary>
/// 这份 ACR 的入口。<see cref="RotationMetadataAttribute"/> 是 PR 发现它的依据，
/// 参数依次是：职业 ID、ACR 名字、作者、版本。
/// </summary>
/// <remarks>
/// <para>
/// <b>和官方指南的差异（以反编译为准）</b>：
/// <see cref="IRotation"/> 一共要求实现 <b>8</b> 个成员，不只是指南提到的 <c>NextGcd</c> /
/// <c>NextOffGcd</c> / <c>NextAlways</c> 三个。另外五个是
/// <see cref="UpdateDebugStatus"/>、<see cref="GetOpener"/>、<see cref="GetEventHandler"/>、
/// <see cref="DrawSettings"/>、<see cref="DrawQTs"/>。
/// </para>
/// <para>
/// <b>★ 列表顺序 = 优先级。</b>每个 <c>NextXxx()</c> 都是「从列表头开始问，
/// 第一个 <c>Check().Success</c> 的决策器赢，后面的一律不再问」。
/// 这条规则和 AE 的 <c>SlotResolver</c> 完全一样，是从 AE 带过来的最值钱的一条经验。
/// </para>
/// </remarks>
[RotationMetadata(RotationInfo.JobId, RotationInfo.Name, RotationInfo.Author, RotationInfo.Version)]
public sealed class SampleRotation : IRotation
{
    private readonly IRotationEventHandler _eventHandler = new SampleRotationEventHandler();

    // 三条独立的优先级列表。AE 时代叫 GCD / oGCD 两条轴，这里多了一条 Always。
    private readonly List<IDecisionResolver> _alwaysResolvers = new();
    private readonly List<IDecisionResolver> _gcdResolvers = new();
    private readonly List<IDecisionResolver> _offGcdResolvers = new();

    /// <summary>
    /// QT 开关的定义表：键是<b>存档用的字符串 ID</b>，值是默认值。
    /// </summary>
    /// <remarks>
    /// ⚠️ 键名就是玩家存档的 Key，<b>改名 = 玩家配置失效</b>（和 AE 的 QT 一样的坑）。
    /// 建议统一加前缀，避免和别的 ACR 撞名。
    /// </remarks>
    public static IReadOnlyDictionary<string, bool> QtList { get; } = new Dictionary<string, bool>
    {
        ["Sample.Enabled"] = true,
    };

    /// <summary>起手列表：名字 → 实现 <see cref="IOpener"/> 的类型。</summary>
    public static IReadOnlyDictionary<string, Type> Openers { get; } = new Dictionary<string, Type>();

    /// <summary>
    /// 构造：注册 QT 开关，并把决策器按优先级填进列表。
    /// </summary>
    public SampleRotation()
    {
        // 抄自 PR 自带的 SimpleRotation：把 QtList 里的开关注册给 PR 的存档系统。
        foreach (var (key, defaultValue) in QtList)
        {
            PromeSettings.Instance.AddQt(key, defaultValue);
        }

        // ★ 这里加进去的顺序 = 优先级顺序。越靠前越优先。
        _gcdResolvers.Add(new PlaceholderGcd());
    }

    /// <inheritdoc />
    public IRotationEventHandler GetEventHandler() => _eventHandler;

    /// <summary>没有起手就返回 null。</summary>
    public IOpener? GetOpener() => null;

    /// <inheritdoc />
    public PAction? NextAlways() => Resolve(_alwaysResolvers);

    /// <inheritdoc />
    public PAction? NextGcd() => Resolve(_gcdResolvers);

    /// <inheritdoc />
    public PAction? NextOffGcd() => Resolve(_offGcdResolvers);

    /// <summary>
    /// 「从列表头开始问，第一个通过的赢」——PR 的决策语义就这一句。
    /// </summary>
    private static PAction? Resolve(List<IDecisionResolver> resolvers)
    {
        foreach (var resolver in resolvers)
        {
            if (resolver.Check().Success)
            {
                return resolver.GetAction();
            }
        }

        return null;
    }

    /// <summary>
    /// 把每个决策器的当前状态刷进 PR 的调试面板（哪个通过、为什么不通过）。
    /// </summary>
    /// <remarks>
    /// 这是排查「某个技能没打出来」的第一入口。抄 SimpleRotation 的写法即可，
    /// PR 的界面会把 <see cref="CheckResult.Message"/> 显示出来，
    /// 所以 <c>Check()</c> 里的说明文字要写成人能看懂的中文。
    /// </remarks>
    public void UpdateDebugStatus()
    {
        RotationManager.AlwaysSolverStatus.Clear();
        RotationManager.GcdSolverStatus.Clear();
        RotationManager.OffGcdSolverStatus.Clear();

        Collect(_alwaysResolvers, RotationManager.AlwaysSolverStatus);
        Collect(_gcdResolvers, RotationManager.GcdSolverStatus);
        Collect(_offGcdResolvers, RotationManager.OffGcdSolverStatus);
    }

    private static void Collect(List<IDecisionResolver> resolvers, List<SolverStatus> sink)
    {
        foreach (var resolver in resolvers)
        {
            var result = resolver.Check();
            sink.Add(new SolverStatus
            {
                Name = resolver.GetType().Name,
                Success = result.Success,
                Message = result.Message,
            });
        }
    }

    /// <summary>QT 开关的快捷显示区。不需要就留空。</summary>
    public void DrawQTs()
    {
    }

    /// <summary>
    /// 职业设置页。PR 会把它画在自己的设置窗口里。
    /// </summary>
    /// <remarks>
    /// 这里用的是 Dalamud 自带的 ImGui 绑定（<c>Dalamud.Bindings.ImGui</c>）。
    /// ⚠️ API 15 起 ImGui 的命名空间从 <c>ImGuiNET</c> 改成了 <c>Dalamud.Bindings.ImGui</c>，
    /// 而且很多参数类型变成了 <c>ImU8String</c>——老教程里的写法抄过来会编译不过。
    /// </remarks>
    public void DrawSettings()
    {
        if (ImGui.BeginTabBar("SampleRotationTabs"))
        {
            if (ImGui.BeginTabItem("常规"))
            {
                ImGui.TextUnformatted($"ACR：{RotationInfo.Name}  v{RotationInfo.Version}");
                ImGui.TextUnformatted($"作者：{RotationInfo.Author}");
                ImGui.Separator();
                ImGui.TextUnformatted("骨架尚未实现任何技能。");

                var enabled = PromeSettings.Instance.GetQt("Sample.Enabled");
                if (ImGui.Checkbox("启用（占位开关）", ref enabled))
                {
                    PromeSettings.Instance.SetQt("Sample.Enabled", enabled);
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }
}
