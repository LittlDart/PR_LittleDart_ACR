using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbGnashingFangGcd : GnbResolverBase
{
	internal const double ChargeEpsilon = 0.98;

	private readonly GnashingPolicy _policy;

	protected override string ResolverDescription => _policy switch
	{
		GnashingPolicy.OpenerBridge => "GCD：子弹连（起手收尾桥接 · 一次性）",
		GnashingPolicy.NoMercyContinue => "GCD：子弹连（无情内 · 续打未打完的那一发）",
		GnashingPolicy.NoMercyPrep => "GCD：子弹连（进无情前的准备）",
		GnashingPolicy.SonicFirst => "GCD：子弹连（无情内收尾）",
		GnashingPolicy.Level100Window => "GCD：子弹连（100 级以下 · 无情内第一段）",
		_ => "GCD：子弹连（无情外 · 常规）",
	};

	public GnbGnashingFangGcd(GnashingPolicy policy = GnashingPolicy.Default)
	{
		_policy = policy;
	}

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16146u))
		{
			return new CheckResult(false, "烈牙还没解锁（60 级）");
		}
		uint nextComboStep = NextComboStep();
		if (GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, "群体连招中：子弹连是单体三段连，群怪里不打（交给群体轴）");
		}
		if (nextComboStep != 0)
		{
			if (!GnbHelper.IsInMeleeRange())
			{
				return new CheckResult(false, "不在近战射程内（继续子弹连）");
			}
			return new CheckResult(true, $"子弹连（续打下一段 = {nextComboStep}）");
		}
		CheckResult commonCheck = CheckStartGate();
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (GnbHelper.GetGnashingFangCharges() < 0.98)
		{
			return new CheckResult(false, "烈牙没有可用充能");
		}
		if (GnbHelper.GetAmmo() <= 0)
		{
			return new CheckResult(false, "没有晶壤，打不出烈牙（留一层进无情）");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, "子弹连：起头（窗口闸门已放行）");
	}

	private CheckResult CheckStartGate()
	{
		if (_policy == GnashingPolicy.OpenerBridge)
		{
			return CheckOpenerBridgeGate();
		}
		bool insideNoMercy = GnbHelper.IsNoMercyInside();
		switch (_policy)
		{
		case GnashingPolicy.NoMercyContinue:
			if (!insideNoMercy)
			{
				return new CheckResult(false, "不在无情窗口内");
			}
			return new CheckResult(false, "没有未打完的子弹连（起头交给窗口内的收尾档）");
		case GnashingPolicy.SonicFirst:
			if (!insideNoMercy)
			{
				return new CheckResult(false, "不在无情窗口内");
			}
			if (GnbHelper.IsLevelAtLeast100())
			{
				return new CheckResult(true, "无情内：子弹连起头（100 级及以上）");
			}
			return new CheckResult(false, "100 级以下：起头由窗口内的第一段档负责");
		case GnashingPolicy.Level100Window:
			if (!insideNoMercy)
			{
				return new CheckResult(false, "不在无情窗口内");
			}
			if (!GnbHelper.IsLevelAtLeast100())
			{
				return new CheckResult(true, "100 级以下 · 无情内：子弹连第一段起头");
			}
			return new CheckResult(false, "100 级及以上：起头交给窗口内的收尾档");
		case GnashingPolicy.NoMercyPrep:
			if (insideNoMercy)
			{
				return new CheckResult(false, "在无情窗口内（交给窗口内的档）");
			}
			if (GnbHelper.IsNoMercyPrepWindow())
			{
				return new CheckResult(true, "进无情前的准备窗口：优先级最高，直接用这一发起头");
			}
			return StartWindowResult("无情外 · 常规");
		default:
			if (insideNoMercy)
			{
				return new CheckResult(false, "在无情窗口内（交给窗口内的档）");
			}
			return StartWindowResult("无情外 · 常规");
		}
		static CheckResult StartWindowResult(string tag)
		{
			if (GnbHelper.HasGnashingReserveCharge())
			{
				return new CheckResult(true, tag + "：还有两层充能，这一发起掉、留一层给穿目进无情");
			}
			if (GnbHelper.GetGnashingFangCharges() >= 0.98)
			{
				return new CheckResult(false, $"只剩一层充能，留着给「穿目进无情」（还差 {2.0 - GnbHelper.GetGnashingFangCharges():F2} 层满）");
			}
			return new CheckResult(false, "烈牙还没有可用充能");
		}
	}

	private CheckResult CheckOpenerBridgeGate()
	{
		if (!GnbHelper.IsPostOpenerBridgeWindow())
		{
			return new CheckResult(false, "不是「起手刚跑完」的那一格（起手没跑 / 已超过 " + $"{30.0:F0} 秒 / 这一发已经借过了）");
		}
		GnbHelper.ConsumeOpenerBridge();
		return new CheckResult(true, "起手收尾桥接：接上 docs\\14 四.1 行2 的「迅连斩 → 烈牙」那一发（一次性）");
	}

	public override PAction GetAction()
	{
		return BuildGnashingAction();
	}

	internal static PAction BuildGnashingAction()
	{
		uint nextComboStep = NextComboStep();
		if (nextComboStep == 0)
		{
			return GnbAction.Of(16146u, (ActionType)0);
		}
		return GnbAction.Of(nextComboStep, (ActionType)0);
	}

	internal static uint NextComboStep()
	{
		if (GnbHelper.HasAura(1842u))
		{
			return 16147u;
		}
		if (GnbHelper.HasAura(1843u))
		{
			return 16150u;
		}
		return GnbHelper.GetLastComboSpellId() switch
		{
			16146u => 16147u,
			16147u => 16150u,
			_ => GnbHelper.GetAmmoComboStep() switch
			{
				1 => 16147u,
				2 => 16150u,
				_ => 0u,
			},
		};
	}
}
