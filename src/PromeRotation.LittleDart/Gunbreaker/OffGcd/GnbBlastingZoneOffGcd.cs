using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbBlastingZoneOffGcd : GnbResolverBase
{
	protected override string ResolverDescription => "oGCD：爆破领域 / 危险领域";

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		uint blastingZoneId = (ActionHelper.IsUnlocked(GnbSkills.爆破领域) ? GnbSkills.爆破领域 : GnbSkills.危险领域);
		if (!ActionHelper.IsUnlocked(blastingZoneId))
		{
			return new CheckResult(false, "还没解锁（18 级 / 80 级）");
		}
		if (GnbHelper.IsBlastingZoneHeld())
		{
			return new CheckResult(false, "无情即将转好，压着留进窗口");
		}
		// ★ 2026-10-19 续剑优先（问题 1 的第二现场）：伤害 oGCD 不许抢续剑的窗口。
		// 实测日志 PRLog-20261019-011841-149.log：01:20:18.708 凶禽爪 → 01:20:19.603 无情 →
		// **01:20:20.407 爆破领域** → 01:20:21.253 音速破，穿目整发丢失（与血壤那次同因：
		// 无情刚开 1s 内续剑被「防吞增伤」暂缓，oGCD 趁空档占位）。
		// 社区轴（docs\14 §二）的处理是 凶禽爪 → 无情 → 穿目 —— 续剑必须优先。
		if (GnbHelper.HasContinuationReadyInMelee())
		{
			return new CheckResult(false, "续剑待打：爆破领域让位（续剑优先）");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new CheckResult(false, "倾泻资源进行中");
		}
		if (!ActionHelper.CanCast(blastingZoneId, (IBattleChara)null))
		{
			return new CheckResult(false, "冷却中或不可施放");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, "爆破领域：好了就开");
	}

	public override PAction GetAction()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.爆破领域))
		{
			return GnbAction.Of(GnbSkills.危险领域, (ActionType)1);
		}
		return GnbAction.Of(GnbSkills.爆破领域, (ActionType)1);
	}
}
