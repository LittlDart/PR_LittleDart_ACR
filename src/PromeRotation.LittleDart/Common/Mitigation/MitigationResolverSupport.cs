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

		if (arg.SkillId != 0)
		{
			engine.NotifyCast(arg.SkillId);
		}
		return profile.BuildAction(arg);
	}
}
