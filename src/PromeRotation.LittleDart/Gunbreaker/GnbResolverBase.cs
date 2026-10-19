using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker;

public abstract class GnbResolverBase : IDecisionResolver
{
	protected abstract string ResolverDescription { get; }

	
	
	
	
	public string Description => ResolverDescription;

	protected abstract CheckResult CheckReal();

	public abstract PAction GetAction();

	
	
	
	
	
	public CheckResult Check()
	{
		if (PromeSettings.Instance.GetQt(GnbQt.StopAttack))
		{
			return new CheckResult(false, "停手");
		}
		return CheckReal();
	}
}
