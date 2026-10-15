using LittleDart.Common;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbDumpDoubleDownGcd : GnbDumpResolverBase
{
	private const int DoubleDownAmmoCost = 2;

	protected override string ResolverDescription => "倾泻资源·无情内：倍攻";

	protected override CheckResult CheckReal()
	{
		CheckResult commonCheck = GnbDumpResolverBase.DumpCommonCheck();
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "不在无情窗口内");
		}
		if (!ActionHelper.IsUnlocked(25760u))
		{
			return new CheckResult(false, "倍攻还没解锁（90 级）");
		}
		if (GnbHelper.GetAmmo() < 2)
		{
			return new CheckResult(false, $"晶壤不足（需要 {2} 发）");
		}
		if (!AbilityGate.ReadyForNextGcd(25760u))
		{
			return new CheckResult(false, "倍攻冷却中");
		}
		return new CheckResult(true, "倾泻资源·无情内：倒倍攻");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(25760u, (ActionType)0);
	}
}
