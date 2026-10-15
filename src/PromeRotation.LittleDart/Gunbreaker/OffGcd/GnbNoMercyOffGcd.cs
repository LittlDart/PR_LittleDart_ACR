using LittleDart.Gunbreaker.GCD;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbNoMercyOffGcd : GnbResolverBase
{
	private const uint AllowDelayAtSavageClaw = 16147u;

	private const uint AllowDelayAtWickedTalon = 16150u;

	protected override string ResolverDescription => "oGCD：无情（60 秒爆发窗口）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(16138u))
		{
			return new CheckResult(false, "无情还没解锁（2 级）");
		}
		if (!PromeSettings.Instance.GetQt("无情"))
		{
			return new CheckResult(false, "QT「无情」关着");
		}
		if (!GnbHelper.IsNoMercyReady())
		{
			return new CheckResult(false, $"无情冷却中（{GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内（无情只在近战范围内开）");
		}

		if (GnbHelper.IsDailyMode() && GnbHelper.IsNoMercyLockedByDyingTarget())
		{
			return new CheckResult(false, $"日常模式：预期目标 {GnbHelper.NoMercyTtkSeconds:F0} 秒内死亡 ⇒ 锁住无情（留给下一只）");
		}
		if (GnbHelper.IsBattleStart())
		{
			return new CheckResult(true, "开怪第一格：直接开无情");
		}
		if (GnbHelper.IsNoMercyAoeConditionMet())
		{
			return new CheckResult(true, $"群体轴：身边 {GnbHelper.GetAoeEnemyCount()} 只 + 预期目标死亡时间够久 ⇒ 开无情");
		}
		if (!PromeSettings.Instance.GetQt("续剑进无情"))
		{
			if (GnbHelper.IsDumpResource())
			{
				return new CheckResult(false, "倾泻资源进行中（简单轴也压着）");
			}
			return new CheckResult(true, "「续剑进无情」关着：简单轴 —— 无情好了就开");
		}
		if (GnbHelper.IsNoMercyHeldOpen())
		{
			return new CheckResult(false, "轴要求先压着（倾泻资源 / 子弹连起不了头 / 音速破预备在手）");
		}
		uint comboStep = GnbGnashingFangGcd.NextComboStep();
		if (GnbHelper.HasAura(1844u))
		{
			return new CheckResult(true, "只剩一个穿目，无情进窗口");
		}
		if (PromeSettings.Instance.GetQt("允许子弹连偏移") && (comboStep == 16147 || comboStep == 16150))
		{
			return new CheckResult(true, "允许子弹连偏移：子弹连中途进无情");
		}
		return new CheckResult(false, "还没到进无情的时机（等「只剩一个穿目」）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Self(16138u, (ActionType)1);
	}
}
