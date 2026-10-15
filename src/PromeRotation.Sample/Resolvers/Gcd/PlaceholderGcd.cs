using PromeRotation.Data;
using PromeRotation.Resolvers;
using PromeRotation.Sample.Actions;

namespace PromeRotation.Sample.Resolvers.Gcd;

public sealed class PlaceholderGcd : IDecisionResolver
{
    public CheckResult Check()
    {
        return new CheckResult(false, "占位决策器：还没实现");
    }

    public PAction GetAction()
    {
        return new PAction(ActionIds.Placeholder, ActionType.Gcd, ActionTargetType.Target);
    }
}
