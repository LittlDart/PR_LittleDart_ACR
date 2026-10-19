using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class TotalEclipseGcd : PldResolverBase
{
	protected override string ResolverDescription => "群体：全蚀斩";

	protected override CheckResult CheckReal()
	{
		// ★★ 2026-10-16 玩家口径：AOE 轴**不看当前选定的目标**。
		//   全蚀斩 7381 的 Action 表实测：Range = 0、EffectRange = 5、CastType = 2、
		//   CanTargetSelf = 是、CanTargetHostile = 否 ⇒ **自身中心 AOE，不需要选中目标**。
		//   原判据的两道目标闸门（AoeQueue 里的 HasValidTarget、下面那句 IsInMeleeRange）
		//   都在读 Core.Target ⇒ 玩家没选目标时这一整条 AOE 轴是哑的。
		//   现在只看「QT + 自身 5 米内 ≥ 门槛只怪 + 挑得出最近的一只」（AoeQueue 唯一实现），
		//   射程由下面 IsReadyAndInRange 的 AOE 版按「最近的一只」判。
		if (!AoeQueue.ShouldUseAoeIgnoringTarget())
		{
			return new CheckResult(false, "AOE 判定不成立（QT 没开 / 自身 5 米内能打的怪不够）");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.全蚀斩))
		{
			return new CheckResult(false, "全蚀斩未解锁");
		}
		// ★ 同 FastBladeGcd：只有计时还在走才算「AOE 连击已起手」，与日珥斩那一格成对。
		if (PldHelper.IsComboAlive() && PldHelper.GetLastComboSpellId() == PldSkills.全蚀斩)
		{
			return new CheckResult(false, "AOE 连击已起手，交给日珥斩");
		}
		if (!IsReadyAndInRange(PldSkills.全蚀斩))
		{
			return new CheckResult(false, "全蚀斩冷却中或身前没有能打的怪");
		}
		return new CheckResult(true, "群体：全蚀斩（AOE 连击起手）");
	}

	public override PAction GetAction()
	{
		// 全蚀斩 7381 是自身中心（Range = 0）⇒ 不需要给「打哪只」，保持原样。
		return PldAction.Of(PldSkills.全蚀斩, ActionType.Gcd);
	}

	private static bool IsReadyAndInRange(uint actionId)
	{
		// ★ 2026-10-16：换成 AOE 版 —— AOE 成立时「在射程内」按 5 米内最近的一只算（不看当前选定目标）。
		return PldHelper.IsReadyAndInRangeForNextGcdAoe(actionId);
	}
}
