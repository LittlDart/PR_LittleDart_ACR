using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

/// <summary>
/// oGCD：无情（16138 · 60 秒爆发窗口）——
/// <list type="bullet">
/// <item>使用 <see cref="GnbHelper.CanCastNow"/> 硬闸门（CD ≤ 0.05s）与 <see cref="GnbHelper.NoMercyMinWeaveGcdRemain"/>（0.65s），消除 0.3s 假就绪空转与 0.05s 死区；</item>
/// <item>★ 2026-10-19 玩家口径：「无情不应该被延迟。你看社区轴，无情完全没有被延迟。」
/// —— 删除旧的「有界等穿目」（下一 GCD 为凶禽爪时最多压 1 个 GCD）与「允许子弹连偏移」两条延迟分支。
/// 无情就绪即在最近一个 GCD 窗口开；子弹连的续剑会自然落进窗口（docs\14 §三 例一/例二 都成立：
/// 凶禽爪 → 无情 → 穿目、猛兽爪 → 无情 → 裂膛 都是合法形态）；</item>
/// <item>日常模式 15s TTK 濒死锁排在「倾泻资源」之前（P2），防止日常下误交无情。</item>
/// </list>
/// </summary>
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
