using Dalamud.Bindings.ImGui;
using PromeRotation.Data;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using PromeRotation.Rotation;
using PromeRotation.Sample.Events;
using PromeRotation.Sample.Resolvers.Gcd;

namespace PromeRotation.Sample;

[RotationMetadata(RotationInfo.JobId, RotationInfo.Name, RotationInfo.Author, RotationInfo.Version)]
public sealed class SampleRotation : IRotation
{
    private readonly IRotationEventHandler _eventHandler = new SampleRotationEventHandler();

    private readonly List<IDecisionResolver> _alwaysResolvers = new();
    private readonly List<IDecisionResolver> _gcdResolvers = new();
    private readonly List<IDecisionResolver> _offGcdResolvers = new();

        public static IReadOnlyDictionary<string, bool> QtList { get; } = new Dictionary<string, bool>
    {
        ["Sample.Enabled"] = true,
    };

        public static IReadOnlyDictionary<string, Type> Openers { get; } = new Dictionary<string, Type>();

        public SampleRotation()
    {

        foreach (var (key, defaultValue) in QtList)
        {
            PromeSettings.Instance.AddQt(key, defaultValue);
        }

        _gcdResolvers.Add(new PlaceholderGcd());
    }

        public IRotationEventHandler GetEventHandler() => _eventHandler;

        public IOpener? GetOpener() => null;

        public PAction? NextAlways() => Resolve(_alwaysResolvers);

        public PAction? NextGcd() => Resolve(_gcdResolvers);

        public PAction? NextOffGcd() => Resolve(_offGcdResolvers);

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

        public void DrawQTs()
    {
    }

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
