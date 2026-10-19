using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbBowShockOffGcd : GnbResolverBase
{
	protected override string ResolverDescription => "oGCD：弓形冲波（只在无情内）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.弓形冲波))
		{
			return new CheckResult(false, "弓形冲波还没解锁（62 级）");
		}
		// ★ 2026-10-19 续剑优先（问题 1）：与血壤 / 爆破领域同一条规则，判据只在
		// GnbHelper.HasContinuationReadyInMelee 一处。社区轴（docs\14 §二）里续剑永远紧跟
		// 它自己的 GCD（凶禽爪 → 无情 → 穿目），伤害 oGCD 不抢这个窗口。
		if (GnbHelper.HasContinuationReadyInMelee())
		{
			return new CheckResult(false, "续剑待打：弓形冲波让位（续剑优先）");
		}
		if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "只能在无情窗口内开");
		}
		if (!GnbHelper.HasEnemyToFight())
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但自身 {GnbHelper.AoeEnemyCountRange} 米内没有能打的怪"
				: "不在近战射程内 / 没有有效目标");
		}
		if (!ActionHelper.CanCast(GnbSkills.弓形冲波, (IBattleChara)null))
		{
			return new CheckResult(false, "冷却中或不可施放");
		}
		return new CheckResult(true, "弓形冲波（无情内）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(GnbSkills.弓形冲波, (ActionType)1);
	}
}
