using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class RequiescatOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：安魂祈祷/绝对统治（强绑定）";

	protected override CheckResult CheckReal()
	{
		if (!PromeSettings.Instance.GetQt(PldQt.UseRequiescat))
		{
			return new CheckResult(false, "QT「绝对统治」没开");
		}
		
		
		
		
		
		if (!PldHelper.HasTargetOrAoeCluster())
		{
			return new CheckResult(false, "眼前没有能打的怪");
		}
		if (!PldHelper.IsFofActive())
		{
			return new CheckResult(false, "不在战逃反应窗口内（要先打战逃反应）");
		}
		
		if (TwinDamage.IsRequiescatCastable())
		{
			return new CheckResult(true, "战逃窗口内，交安魂祈祷 / 绝对统治");
		}
		return new CheckResult(false, "安魂祈祷 / 绝对统治 还没冷却好");
	}

	public override PAction GetAction()
	{
		
		
		
		if (ActionHelper.IsUnlocked(PldSkills.绝对统治) && PldHelper.IsReadyWithCanCastAoe(PldSkills.绝对统治) && !ActionHelper.RecentlyUsed(PldSkills.绝对统治, 500))
		{
			return PldAction.Aoe(PldSkills.绝对统治, ActionType.OffGcd, PldHelper.GetCastTarget(PldSkills.绝对统治));
		}
		return PldAction.Aoe(PldSkills.安魂祈祷技能, ActionType.OffGcd, PldHelper.GetCastTarget(PldSkills.安魂祈祷技能));
	}
}
