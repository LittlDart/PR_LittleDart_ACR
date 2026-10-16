using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbSonicBreakGcd : GnbResolverBase
{
	private readonly bool _aoeOnly;

	protected override string ResolverDescription
	{
		get
		{
			if (!_aoeOnly)
			{
				return "GCD：音速破（无情带出的 DoT）";
			}
			return "群体轴：音速破（无情带出的 DoT）";
		}
	}

	public GnbSonicBreakGcd(bool aoeOnly = false)
	{
		_aoeOnly = aoeOnly;
	}

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(16153u))
		{
			return new CheckResult(false, "音速破还没解锁（54 级）");
		}
		if (!_aoeOnly && GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, "群体连招中：交给群体轴那一档音速破");
		}
		if (_aoeOnly && !GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, "不在群体连招里");
		}
		if (!GnbHelper.HasAura(3886u))
		{
			return new CheckResult(false, "没有「音速破预备」（无情还没开）");
		}
		if (!GnbHelper.IsNoMercyInside() && !GnbHelper.IsNoMercyHeldOpen())
		{
			return new CheckResult(false, "无情马上要开，先把音速破留给窗口");
		}
		if (GnbHelper.GetCastTarget(16153u) == null)
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但 {GnbHelper.GetMeleeRange():F0} 米内没有能打的怪"
				: "没有有效目标 / 不在近战射程内");
		}
		return new CheckResult(true, "音速破（无情带出的 DoT）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Aoe(16153u, (ActionType)0, GnbHelper.GetCastTarget(16153u));
	}
}
