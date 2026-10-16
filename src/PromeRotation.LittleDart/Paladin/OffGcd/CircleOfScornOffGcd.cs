using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class CircleOfScornOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：厄运流转（双伤 1）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(23u))
		{
			return new CheckResult(false, "厄运流转还没解锁");
		}
		if (!TwinDamage.ShouldUse())
		{
			return new CheckResult(false, "双伤的两个锁没过（战逃/绝对统治冷却不在窗口）或眼前没有能打的怪");
		}
		// ★ 2026-10-16 玩家口径：厄运流转 23 的 Action 表实测 Range = 0 / EffectRange = 5 /
		//   CanTargetSelf = 是 ⇒ **自身中心 AOE，不需要选中目标**。ready 判据换成 AOE 版
		//   （AOE 成立时「在射程内」= 5 米内有能打的怪；不成立时与老函数逐字等价）。
		if (!TwinDamage.ReadyWithCanCastAoe(23u))
		{
			return new CheckResult(false, "厄运流转没冷却好 / 刚放过");
		}
		return new CheckResult(true, "双伤 1：厄运流转");
	}

	public override PAction GetAction()
	{
		TwinDamage.Diag(TwinDamage.ShouldUse(), ActionHelper.IsUnlocked(23u), TwinDamage.ReadyWithCanCastAoe(23u), ActionHelper.IsUnlocked(25747u), TwinDamage.ReadyWithCanCastAoe(25747u));
		// 厄运流转 23 是自身中心（Range = 0）⇒ 不需要给「打哪只」，保持原样。
		return PldAction.Of(23u, (ActionType)1);
	}
}
