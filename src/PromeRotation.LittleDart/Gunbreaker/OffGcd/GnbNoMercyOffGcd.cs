using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbNoMercyOffGcd : GnbResolverBase
{
	protected override string ResolverDescription => "oGCD：无情（60 秒爆发窗口）";

	protected override CheckResult CheckReal()
	{
		if (GnbHelper.GetGcdRemainSeconds() <= GnbHelper.NoMercyMinWeaveGcdRemain)
		{
			return new CheckResult(false, $"GCD 剩余窗口不足（≤{GnbHelper.NoMercyMinWeaveGcdRemain:F2}s），避免卡 GCD");
		}
		if (!GnbHelper.CanCastNow(GnbSkills.无情))
		{
			return new CheckResult(false, $"无情未就绪（冷却 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
		}
		if (!GnbHelper.HasEnemyToFight())
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但自身 {GnbHelper.AoeEnemyCountRange} 米内没有能打的怪"
				: "不在近战射程内（无情只在近战范围内开）");
		}
		if (GnbHelper.IsDailyMode() && GnbHelper.IsNoMercyLockedByDyingTarget())
		{
			return new CheckResult(false, $"日常模式：预期目标 {GnbHelper.NoMercyTtkSeconds:F0} 秒内死亡 ⇒ 锁住无情（留给下一只）");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new CheckResult(true, "倾泻资源：直接开启无情");
		}
		if (!PromeSettings.Instance.GetQt(GnbQt.UseNoMercy))
		{
			return new CheckResult(false, "QT「无情」关着");
		}
		if (GnbHelper.IsBattleStart())
		{
			return new CheckResult(true, "开怪第一格：直接开无情");
		}
		if (GnbHelper.ShouldUseAoeCombo() || GnbHelper.IsNoMercyAoeConditionMet())
		{
			return new CheckResult(true, $"群体轴：身边 {GnbHelper.GetAoeEnemyCount()} 只 ⇒ 开无情");
		}
		if (GnbHelper.HasAura(GnbSkills.穿目预备))
		{
			return new CheckResult(true, "只剩一个穿目：无情就绪就进窗口（不再压 GCD 等）");
		}
		return new CheckResult(true, "无情已就绪：卡 CD 开启爆发窗口（不延迟）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Self(GnbSkills.无情, ActionType.OffGcd);
	}
}
