using ECommons.Logging;
using LittleDart.Gunbreaker.Skill;
using LittleDart.Gunbreaker.Planners;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbPlanGcd : GnbResolverBase
{
	public static readonly GnbPlanEngine Engine = new GnbPlanEngine(GnbPlan100Opener.Steps, GnbSkills.迅连斩);

	protected override string ResolverDescription => "GCD：起手相位尾巴（利刃斩/残暴弹/迅连斩 · 状态驱动）";

	protected override CheckResult CheckReal()
	{
		return Engine.CheckCurrent();
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public override PAction GetAction()
	{
		PAction? action = Engine.GetAction();
		if (action is not null)
		{
			return action;
		}
		PluginLog.Warning($"[LittleDart-GNB] 相位尾巴 Check 通过却取不到技能（{Engine.Status}）⇒ 兜底 利刃斩 {GnbSkills.利刃斩}");
		return GnbAction.Of(GnbSkills.利刃斩, (ActionType)0);
	}
}
