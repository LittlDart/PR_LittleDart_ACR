using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class ExpiacionOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：偿赎剑（双伤 2）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(PldSkills.偿赎剑))
		{
			return new CheckResult(false, "偿赎剑还没学会（86 级）");
		}
		if (!TwinDamage.ShouldUse())
		{
			return new CheckResult(false, "双伤的两个锁没过（战逃/绝对统治冷却不在窗口）或眼前没有能打的怪");
		}
		
		
		
		
		
		
		
		if (!TwinDamage.ReadyWithCanCastAoe(PldSkills.偿赎剑))
		{
			return new CheckResult(false, "偿赎剑没冷却好 / 刚放过 / 近战射程内没有能打的怪");
		}
		if (PldHelper.GetCastTarget(PldSkills.偿赎剑) == null)
		{
			return new CheckResult(false, "近战射程内没有能打的怪（偿赎剑必须选中一只敌人）");
		}
		return new CheckResult(true, "双伤 2：偿赎剑");
	}

	public override PAction GetAction()
	{
		
		
		return PldAction.Aoe(PldSkills.偿赎剑, ActionType.OffGcd, PldHelper.GetCastTarget(PldSkills.偿赎剑));
	}
}
