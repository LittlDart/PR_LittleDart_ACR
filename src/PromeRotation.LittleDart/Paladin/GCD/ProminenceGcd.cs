using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class ProminenceGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体：日珥斩";

	protected override CheckResult CheckReal()
	{
		// ★★ 2026-10-16 玩家口径：AOE 轴**不看当前选定的目标**。
		//   日珥斩 16457 的 Action 表实测：Range = 0、EffectRange = 5、CastType = 2、
		//   CanTargetSelf = 是、CanTargetHostile = 否 ⇒ **自身中心 AOE，不需要选中目标**。
		if (!AoeQueue.ShouldUseAoeIgnoringTarget())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 自身 5 米内能打的怪不够）");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.日珥斩))
		{
			return new CheckResult(false, "日珥斩未解锁");
		}
		// ★ 与全蚀斩那一格成对：计时走完后不能再接日珥斩（否则全蚀斩被 id 挡、这里被计时挡 = 双向封死）。
		if (!PldHelper.IsComboAlive() || PldHelper.GetLastComboSpellId() != PldSkills.全蚀斩)
		{
			return new CheckResult(false, "AOE 连击不对：上一段不是全蚀斩（或连击计时已走完）");
		}
		if (!IsReadyAndInRange(PldSkills.日珥斩))
		{
			return new CheckResult(false, "日珥斩冷却中或身前没有能打的怪");
		}
		return new CheckResult(true, "群体：日珥斩（AOE 连击第 2 段）");
	}

	public override PAction GetAction()
	{
		// 日珥斩 16457 是自身中心（Range = 0）⇒ 不需要给「打哪只」，保持原样。
		return PldAction.Of(PldSkills.日珥斩, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		// ★ 2026-10-16：换成 AOE 版（AOE 成立时按 5 米内最近的一只判射程）。
		return PldHelper.IsReadyAndInRangeForNextGcdAoe(actionId);
	}
}
