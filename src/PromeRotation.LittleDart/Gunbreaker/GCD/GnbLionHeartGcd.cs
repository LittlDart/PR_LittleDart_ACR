using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

public class GnbLionHeartGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：狮心连（崛起之心/支配之心/终结之心）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.血壤))
		{
			return new CheckResult(false, "血壤还没解锁（76 级）");
		}
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(GnbSkills.血壤);
		if (adjustedActionId == GnbSkills.血壤)
		{
			return new CheckResult(false, "血壤没有被替换成狮心连");
		}
		if (GnbHelper.IsLionHeartHeld())
		{
			return new CheckResult(false, $"压着等无情窗口（心有灵狮还剩 {GnbHelper.GetAuraTimeLeft(GnbSkills.心有灵狮):F1}s，无情 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
		}

		
		
		
		
		
		bool lionHeartStart = adjustedActionId == GnbSkills.崛起之心;
		
		if (lionHeartStart && !GnbHelper.ShouldUseAoeCombo() && GnbHelper.HasGnashingChainInProgress())
		{
			return new CheckResult(false, "子弹连还没打完：先续打（旧版靠注册位置保护）");
		}
		
		
		
		if (lionHeartStart && !GnbHelper.IsDumpResource() && !GnbHelper.ShouldUseAoeCombo()
			&& GnbSonicBreakGcd.SonicBreakGate().Success)
		{
			return new CheckResult(false, "单体轴：音速破优先（旧 :67 在狮心连 :68 之前）");
		}
		
		
		
		
		
		
		
		if (lionHeartStart && !GnbHelper.IsDumpResource()
			&& !GnbHelper.ShouldUseAoeCombo() && !GnbHelper.ShouldPrioritizeLionHeartChain()
			&& GnbGnashingFangGcd.ShouldStartChainFirst()
			&& GnbHelper.GetAuraTimeLeft(GnbSkills.心有灵狮) > 4.0 * (double)GnbHelper.GetGcdSeconds())
		{
			return new CheckResult(false, "社区轴：子弹连先起头、狮心连排窗口最后");
		}
		IBattleChara? castTarget = GnbHelper.GetCastTarget(adjustedActionId);
		if (castTarget == null)
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但 {GnbHelper.GetMeleeRange():F0} 米内没有能打的怪"
				: "没有有效目标 / 不在近战射程内");
		}
		string targetName = ((object)((IGameObject)castTarget).Name).ToString();
		if (GnbHelper.IsDumpResource() && GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(true, $"倾泻资源·无情内：打狮心连（adjusted={adjustedActionId}，打 {targetName}）");
		}
		return new CheckResult(true, $"狮心连（adjusted={adjustedActionId}，打 {targetName}）");
	}

	public override PAction GetAction()
	{
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(GnbSkills.血壤);
		uint actionId = adjustedActionId switch
		{
			GnbSkills.支配之心 => GnbSkills.支配之心,
			GnbSkills.终结之心 => GnbSkills.终结之心,
			_ => GnbSkills.崛起之心,
		};
		return GnbAction.Aoe(actionId, (ActionType)0, GnbHelper.GetCastTarget(actionId));
	}
}
