using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class ExpiacionOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：偿赎剑（双伤 2）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(25747u))
		{
			return new CheckResult(false, "偿赎剑还没学会（86 级）");
		}
		if (!TwinDamage.ShouldUse())
		{
			return new CheckResult(false, "双伤的两个锁没过（战逃/绝对统治冷却不在窗口）或眼前没有能打的怪");
		}
		// ★ 2026-10-16 玩家口径：偿赎剑 25747 的 Action 表实测 **Range = -1**（必须选一只敌人）、
		//   EffectRange = 5（打目标周围一圈）⇒ 它属于「要选目标的群体技能 ⇒ 打离自己最近的那一只」。
		//   于是这一格有两件事：
		//     ① ready 判据换成 AOE 版（AOE 成立时按「近战射程内最近的一只」判，不看当前选定目标）；
		//     ② 出手目标由 GetAction 里的 PldAction.Aoe 钉到同一只（写进 NetworkTid）——
		//        ⚠️ 这两处的半径必须同源（都走 PldHelper.GetCastRadius），否则会「Check 说够得着、
		//        UseAction 却打空」，撞铁律②。
		if (!TwinDamage.ReadyWithCanCastAoe(25747u))
		{
			return new CheckResult(false, "偿赎剑没冷却好 / 刚放过 / 近战射程内没有能打的怪");
		}
		if (PldHelper.GetCastTarget(25747u) == null)
		{
			return new CheckResult(false, "近战射程内没有能打的怪（偿赎剑必须选中一只敌人）");
		}
		return new CheckResult(true, "双伤 2：偿赎剑");
	}

	public override PAction GetAction()
	{
		// 群体成立时打「近战射程内离自己最近的一只」；单体路径 GetCastTarget 原样返回当前选定目标，
		// 效果与改动前一致（见 PldAction.Aoe / PldHelper.GetCastTarget 的注释）。
		return PldAction.Aoe(25747u, (ActionType)1, PldHelper.GetCastTarget(25747u));
	}
}
