using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbGnashingFangGcd : GnbResolverBase
{
	internal const double ChargeEpsilon = 0.98;

	protected override string ResolverDescription => "GCD：子弹连（烈牙/猛兽爪/凶禽爪）";

	
	private readonly record struct GnashingDecision(PAction? Action, bool ConsumeOpenerBridge, string Message);

	protected override CheckResult CheckReal()
	{
		GnashingDecision decision = Evaluate();
		return new CheckResult(decision.Action is not null, decision.Message);
	}

	public override PAction GetAction()
	{
		GnashingDecision decision = Evaluate();
		
		
		if (decision.Action is null)
		{
			return BuildGnashingAction();
		}
		
		if (decision.ConsumeOpenerBridge)
		{
			GnbHelper.ConsumeOpenerBridge();
		}
		return decision.Action;
	}

	
	
	
	private static GnashingDecision Evaluate()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new GnashingDecision(null, false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.烈牙))
		{
			return new GnashingDecision(null, false, "烈牙还没解锁（60 级）");
		}
		if (GnbHelper.ShouldUseAoeCombo())
		{
			return new GnashingDecision(null, false, "群体连招中：子弹连是单体三段连，群怪里不打（交给群体轴）");
		}
		uint nextComboStep = NextComboStep();
		if (nextComboStep != 0)
		{
			
			
			
			
			
			if (!GnbHelper.IsInMeleeRange())
			{
				return new GnashingDecision(null, false, "不在近战射程内（继续子弹连）");
			}
			return new GnashingDecision(GnbAction.Of(nextComboStep, ActionType.Gcd), false, $"子弹连（续打下一段 = {nextComboStep}）");
		}
		
		
		
		
		(bool yieldToBaseCombo, string comboGuardReason) = GnbComboGuard.ShouldYieldToBaseCombo(GnbComboGuard.GnashingFangGcdCost);
		if (yieldToBaseCombo)
		{
			return new GnashingDecision(null, false, comboGuardReason);
		}
		double charges = GnbHelper.GetGnashingFangCharges();
		if (charges < ChargeEpsilon)
		{
			return new GnashingDecision(null, false, "烈牙没有可用充能");
		}
		if (GnbHelper.GetAmmo() <= 0)
		{
			return new GnashingDecision(null, false, "没有晶壤，打不出烈牙（留一层进无情）");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new GnashingDecision(null, false, "不在近战射程内");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), false, "倾泻资源：打子弹连");
		}
		if (GnbHelper.IsPostOpenerBridgeWindow())
		{
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), true, "起手收尾桥接：接上「迅连斩 → 烈牙」那一发（一次性）");
		}
		if (GnbHelper.IsNoMercyInside())
		{
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), false, "无情内：子弹连起头");
		}
		if (GnbHelper.IsNoMercyPrepWindow())
		{
			if (charges <= GnbHelper.NoMercyPrepMinGnashingCharges)
			{
				return new GnashingDecision(null, false, $"进无情前准备窗口：烈牙充能不足 {GnbHelper.NoMercyPrepMinGnashingCharges:F1} 层（当前 {charges:F2}），留进无情窗口内再起头");
			}
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), false, $"进无情前的准备窗口：烈牙充能 {charges:F2} > {GnbHelper.NoMercyPrepMinGnashingCharges:F1}，直接用这一发起头");
		}
		if (GnbHelper.HasGnashingReserveCharge())
		{
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), false, "无情外 · 常规：还有两层充能，这一发起掉、留一层给穿目进无情");
		}
		return new GnashingDecision(null, false, $"只剩一层充能，留着给「穿目进无情」（还差 {2.0 - charges:F2} 层满）");
	}

	internal static PAction BuildGnashingAction()
	{
		uint nextComboStep = NextComboStep();
		if (nextComboStep == 0)
		{
			return GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd);
		}
		return GnbAction.Of(nextComboStep, ActionType.Gcd);
	}

	internal static uint NextComboStep()
	{
		if (GnbHelper.HasAura(GnbSkills.撕喉预备))
		{
			return GnbSkills.猛兽爪;
		}
		if (GnbHelper.HasAura(GnbSkills.裂膛预备))
		{
			return GnbSkills.凶禽爪;
		}
		return GnbHelper.GetLastComboSpellId() switch
		{
			GnbSkills.烈牙 => GnbSkills.猛兽爪,
			GnbSkills.猛兽爪 => GnbSkills.凶禽爪,
			_ => GnbHelper.GetAmmoComboStep() switch
			{
				1 => GnbSkills.猛兽爪,
				2 => GnbSkills.凶禽爪,
				_ => 0u,
			},
		};
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	internal static bool ShouldStartChainFirst()
	{
		if (!GnbHelper.IsNoMercyInside())
		{
			return false;
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.烈牙))
		{
			
			
			return false;
		}
		if (GnbHelper.ShouldUseAoeCombo())
		{
			
			return false;
		}
		if (NextComboStep() != 0)
		{
			
			return false;
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			
			return false;
		}
		return GnbHelper.GetGnashingFangCharges() >= ChargeEpsilon && GnbHelper.GetAmmo() > 0;
	}
}
