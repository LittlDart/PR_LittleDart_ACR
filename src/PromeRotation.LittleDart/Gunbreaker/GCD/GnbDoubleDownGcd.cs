using LittleDart.Common;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbDoubleDownGcd : GnbResolverBase
{
	private const int AmmoCost = 2;

	private readonly bool _aoeOnly;

	protected override string ResolverDescription
	{
		get
		{
			if (!_aoeOnly)
			{
				return "GCD：倍攻（耗 2 晶壤 · 无情内）";
			}
			return "群体轴：倍攻（耗 2 晶壤 · 无情内外都要倒）";
		}
	}

	public GnbDoubleDownGcd(bool aoeOnly = false)
	{
		_aoeOnly = aoeOnly;
	}

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(25760u))
		{
			return new CheckResult(false, "倍攻还没解锁（90 级）");
		}
		if (_aoeOnly)
		{
			if (!GnbHelper.ShouldUseAoeCombo())
			{
				return new CheckResult(false, "不在群体连招里");
			}
			if (!GnbHelper.IsNoMercyInside())
			{
				return new CheckResult(false, "群体轴·无情外：倍攻只在无情窗口内放（把晶壤留给命运之环）");
			}
		}
		else if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "不在无情窗口内");
		}
		if (GnbHelper.GetAmmo() < 2)
		{
			return new CheckResult(false, $"晶壤不足（需要 {2} 发）");
		}
		if (!AbilityGate.ReadyForNextGcd(25760u))
		{
			return new CheckResult(false, "倍攻冷却中或不可施放");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, _aoeOnly ? "群体轴：倒倍攻（无情内外都倒）" : "无情内倒倍攻");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(25760u, (ActionType)0);
	}
}
