using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbDumpGnashingFangGcd : GnbDumpResolverBase
{
	private readonly bool _insideNoMercy;

	protected override string ResolverDescription
	{
		get
		{
			if (!_insideNoMercy)
			{
				return "倾泻资源·无情外：子弹连";
			}
			return "倾泻资源·无情内：子弹连";
		}
	}

	public GnbDumpGnashingFangGcd(bool insideNoMercy)
	{
		_insideNoMercy = insideNoMercy;
	}

	protected override CheckResult CheckReal()
	{
		CheckResult commonCheck = GnbDumpResolverBase.DumpCommonCheck();
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (GnbHelper.IsNoMercyInside() != _insideNoMercy)
		{
			return new CheckResult(false, _insideNoMercy ? "不在无情窗口内" : "在无情窗口内");
		}
		if (!ActionHelper.IsUnlocked(16146u))
		{
			return new CheckResult(false, "烈牙还没解锁（60 级）");
		}
		if (!GnbHelper.IsGnashingAvailable())
		{
			return new CheckResult(false, "子弹连不可用（充能或晶壤不够）");
		}
		return new CheckResult(true, "倾泻资源：打子弹连");
	}

	public override PAction GetAction()
	{
		return GnbGnashingFangGcd.BuildGnashingAction();
	}
}
