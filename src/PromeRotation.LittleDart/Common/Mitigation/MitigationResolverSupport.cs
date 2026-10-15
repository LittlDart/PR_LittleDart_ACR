using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Common.Mitigation;

public static class MitigationResolverSupport
{
	public static readonly MitigationSkillDef Placeholder = new MitigationSkillDef
	{
		Name = "占位（无动作）",
		SkillId = 0u,
		SelfTarget = true
	};

	public static CheckResult Check(MitigationEngine engine, MitigationJobProfile profile, MitigationSkillDef? def)
	{
		if (def == null)
		{
			return new CheckResult(false, "减伤注册表里找不到这个技能（注册表与决策器不一致）");
		}
		if (!engine.ShouldCast(def.SkillId))
		{
			return new CheckResult(false, "调度器本轮没选中这个减伤");
		}
		if (!ActionHelper.IsUnlocked(def.SkillId))
		{
			return new CheckResult(false, $"{def.Name} 还没解锁（技能 ID {def.SkillId}）");
		}
		if (!def.SelfTarget && !profile.HasValidTarget())
		{
			return new CheckResult(false, def.Name + " 需要对敌目标，但当前没有有效目标");
		}
		if (def.ResourceCost > 0 && !profile.HasResourceFor(def))
		{
			return new CheckResult(false, "职业资源不足，交不出" + def.Name);
		}
		if (!ActionHelper.IsReady(def.SkillId))
		{
			return new CheckResult(false, def.Name + " 冷却中（未就绪）");
		}
		return new CheckResult(true, def.Name + "：调度器已选中且条件齐备");
	}

	public static PAction GetAction(MitigationEngine engine, MitigationJobProfile profile, MitigationSkillDef? def)
	{
		MitigationSkillDef arg = def ?? Placeholder;
		// ★★ 2026-10-14：**这是全工程唯一「真的把减伤交出去」的落点**，所以「已交」记账必须在这里。
		//
		//   背景（改之前是个空的）：`IMitigationScheduler.NotifyCast` → `MitigationEngine.NotifyCast`
		//   → `MitigationState.RecordCast` 这条链**只有定义、没有任何调用方** ——
		//   于是 `LastCastMs` 永远是 0、「这个技能刚交过」的本地记录从来不生效。
		//   玩家需求「每个 AOE 只用一个群体减伤」正需要这份记账，所以在这里接上。
		//
		//   ⚠️ 语义是「**准备交**」而不是「确认出手」：到这里时 `Check` 已经过了
		//   `IsUnlocked` / `HasResourceFor` / `IsReady`（含 `CanCast`）三道闸，
		//   所以这是框架真要出这一发。若因射程/服务器等原因真的没出，代价是
		//   「这一次 AOE 少交一个减伤」—— 比「同一次 AOE 连交好几个大招」安全得多（玩家口径）。
		if (arg.SkillId != 0)
		{
			engine.NotifyCast(arg.SkillId);
		}
		return profile.BuildAction(arg);
	}
}
