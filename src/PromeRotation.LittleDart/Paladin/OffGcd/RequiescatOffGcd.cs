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
		// ★ 2026-10-16 玩家口径：**爆发窗口不该被「玩家没选目标」卡住**。
		//   安魂祈祷 7383（Range = -1）与绝对统治 36921（25 米）都是**要选敌人的**技能，
		//   但 AOE 成立时它们会打在「离自己最近的那一只」上（见 GetAction 的 PldAction.Aoe）。
		//   原判据是 `HasValidTarget()`（**没有**近战要求）⇒ 换成 HasTargetOrAoeCluster()：
		//   AOE 成立时「5 米内有能打的怪」也算数，其余情况与老判据逐字等价（**只放宽、不收紧**）。
		if (!PldHelper.HasTargetOrAoeCluster())
		{
			return new CheckResult(false, "眼前没有能打的怪");
		}
		if (!PldHelper.IsFofActive())
		{
			return new CheckResult(false, "不在战逃反应窗口内（要先打战逃反应）");
		}
		// IsRequiescatCastable 内部已换成 AOE 版 ready 判据（★ 2026-10-16）。
		if (TwinDamage.IsRequiescatCastable())
		{
			return new CheckResult(true, "战逃窗口内，交安魂祈祷 / 绝对统治");
		}
		return new CheckResult(false, "安魂祈祷 / 绝对统治 还没冷却好");
	}

	public override PAction GetAction()
	{
		// ⚠️ 两个分支都要给出「打哪只」：7383 是 Range = -1、36921 是 25 米，都是要选敌人的技能
		//    ⇒ 群体成立时钉到最近的一只（PldAction.Aoe），否则 PldHelper.GetCastTarget 原样返回
		//    当前选定目标，与改动前逐字等价。
		if (ActionHelper.IsUnlocked(PldSkills.绝对统治) && PldHelper.IsReadyWithCanCastAoe(PldSkills.绝对统治) && !ActionHelper.RecentlyUsed(PldSkills.绝对统治, 500))
		{
			return PldAction.Aoe(PldSkills.绝对统治, ActionType.OffGcd, PldHelper.GetCastTarget(PldSkills.绝对统治));
		}
		return PldAction.Aoe(PldSkills.安魂祈祷技能, ActionType.OffGcd, PldHelper.GetCastTarget(PldSkills.安魂祈祷技能));
	}
}
