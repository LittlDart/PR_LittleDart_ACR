using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker;

public abstract class GnbResolverBase : IDecisionResolver
{
	protected abstract string ResolverDescription { get; }

	protected abstract CheckResult CheckReal();

	public abstract PAction GetAction();

	public CheckResult Check()
	{
		if (PromeSettings.Instance.GetQt("停手"))
		{
			return new CheckResult(false, "停手");
		}
		CheckResult checkResult = CheckReal();
		if (checkResult.Success && GnbDiagnostics.Enabled)
		{
			GnbDiagnostics.OnHit(ResolverDescription, checkResult.Message);
		}
		return checkResult;
	}

	protected static CheckResult SkipNotImplemented()
	{
		return new CheckResult(false, "尚未实现");
	}
}
