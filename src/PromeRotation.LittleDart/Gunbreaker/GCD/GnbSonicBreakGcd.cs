using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

public class GnbSonicBreakGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：音速破（无情带出的 DoT）";

	
	
	
	
	
	internal static CheckResult SonicBreakGate()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.音速破))
		{
			return new CheckResult(false, "音速破还没解锁（54 级）");
		}
		if (!GnbHelper.HasAura(GnbSkills.音速破预备))
		{
			return new CheckResult(false, "没有「音速破预备」（无情还没开）");
		}
		if (!GnbHelper.IsNoMercyInside() && !GnbHelper.IsNoMercyHeldOpen())
		{
			return new CheckResult(false, "无情马上要开，先把音速破留给窗口");
		}
		if (GnbHelper.GetCastTarget(GnbSkills.音速破) == null)
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但 {GnbHelper.GetMeleeRange():F0} 米内没有能打的怪"
				: "没有有效目标 / 不在近战射程内");
		}
		return new CheckResult(true, GnbHelper.ShouldUseAoeCombo()
			? "群体轴：音速破（无情带出的 DoT）"
			: "音速破（无情带出的 DoT）");
	}

	protected override CheckResult CheckReal()
	{
		CheckResult gate = SonicBreakGate();
		if (!gate.Success)
		{
			return gate;
		}
		
		
		
		
		
		bool nextChainStepIsWickedTalon = GnbGnashingFangGcd.NextComboStep() == GnbSkills.凶禽爪;
		bool releaseInsideNoMercy = GnbHelper.IsNoMercyInside() && !nextChainStepIsWickedTalon;
		if (!GnbHelper.ShouldUseAoeCombo() && GnbHelper.HasGnashingChainInProgress() && !releaseInsideNoMercy)
		{
			return new CheckResult(false, nextChainStepIsWickedTalon && GnbHelper.IsNoMercyInside()
				? "下一段是凶禽爪：先续打，音速破不许把穿目推后"
				: "子弹连还没打完：先续打（旧版靠注册位置保护）");
		}
		
		
		
		
		
		if (!GnbHelper.ShouldUseAoeCombo() && GnbGnashingFangGcd.ShouldStartChainFirst()
			&& !GnbHelper.ShouldPrioritizeLionHeartChain())
		{
			return new CheckResult(false, "子弹连先起头（社区轴无情窗口：烈牙 → 倍攻 → 音速破 → …）");
		}
		
		if (GnbHelper.IsDumpResource() && (GnbHelper.IsGnashingAvailable() || GnbHelper.GetAmmo() > 0))
		{
			return new CheckResult(false, "倾泻资源：让位给倾泻链（旧版倾泻链排在音速破之前）");
		}
		
		
		if (!GnbHelper.ShouldUseAoeCombo() && !GnbHelper.IsDumpResource() && GnbHelper.IsNoMercyInside()
			&& GnbHelper.IsGnashingAvailable() && !GnbHelper.IsLevelAtLeast100())
		{
			return new CheckResult(false, "无情内让位给子弹连（100 级以下先起头烈牙）");
		}
		return new CheckResult(true, gate.Message);
	}

	public override PAction GetAction()
	{
		return GnbAction.Aoe(GnbSkills.音速破, (ActionType)0, GnbHelper.GetCastTarget(GnbSkills.音速破));
	}
}
