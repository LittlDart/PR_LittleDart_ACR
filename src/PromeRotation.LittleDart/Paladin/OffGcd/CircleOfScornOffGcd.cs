using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class CircleOfScornOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：厄运流转（双伤 1）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(PldSkills.厄运流转))
		{
			return new CheckResult(false, "厄运流转还没解锁");
		}
		if (!TwinDamage.ShouldUse())
		{
			return new CheckResult(false, "双伤的两个锁没过（战逃/绝对统治冷却不在窗口）或眼前没有能打的怪");
		}
		
		
		
		if (!TwinDamage.ReadyWithCanCastAoe(PldSkills.厄运流转))
		{
			return new CheckResult(false, "厄运流转没冷却好 / 刚放过");
		}
		return new CheckResult(true, "双伤 1：厄运流转");
	}

	public override PAction GetAction()
	{
		TwinDamage.Diag(TwinDamage.ShouldUse(), ActionHelper.IsUnlocked(PldSkills.厄运流转), TwinDamage.ReadyWithCanCastAoe(PldSkills.厄运流转), ActionHelper.IsUnlocked(PldSkills.偿赎剑), TwinDamage.ReadyWithCanCastAoe(PldSkills.偿赎剑));
		
		return PldAction.Of(PldSkills.厄运流转, ActionType.OffGcd);
	}
}
