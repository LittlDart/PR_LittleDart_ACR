using System;
using LittleDart.Gunbreaker.Planners;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbPlanGcd : GnbResolverBase
{
	public static readonly GnbPlanEngine Engine = new GnbPlanEngine(GnbPlan100Opener.Steps, 16145u);

	protected override string ResolverDescription => "GCD：起手相位尾巴（利刃斩/残暴弹/迅连斩 · 状态驱动）";

	protected override CheckResult CheckReal()
	{
		return Engine.CheckCurrent();
	}

	public override PAction GetAction()
	{
		return Engine.GetAction() ?? throw new InvalidOperationException("[LittleDart-GNB] 相位引擎 Check 通过却交不出技能 —— 违反 docs18 第 5 节铁律 2。");
	}
}
