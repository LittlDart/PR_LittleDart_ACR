using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class BladeOfHonorOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：荣耀之剑（悔罪连收尾）";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.荣耀之剑))
		{
			return new CheckResult(false, "荣耀之剑还没学会（100 级）");
		}
		if (!PldHelper.HasAura(PldSkills.荣耀之剑预备, 1))
		{
			return new CheckResult(false, "没有「荣耀之剑预备」buff（悔罪连还没打完）");
		}
		if (!ActionHelper.CanCast(PldSkills.荣耀之剑, (IBattleChara)null) || ActionHelper.RecentlyUsed(PldSkills.荣耀之剑, 500))
		{
			return new CheckResult(false, "荣耀之剑没冷却好 / 刚放过");
		}
		float actionRange = default;
		if (ActionHelper.TryGetActionRange(PldSkills.荣耀之剑, out actionRange) && actionRange > 0f && PldHelper.GetDistanceToTargetCircle() > actionRange)
		{
			return new CheckResult(false, "离目标太远，荣耀之剑够不着");
		}
		return new CheckResult(true, "有荣耀之剑预备，收尾");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.荣耀之剑, ActionType.OffGcd);
	}
}
