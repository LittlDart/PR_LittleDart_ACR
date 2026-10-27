using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// GCD：AOE 连（恶魔切 16141 / 恶魔杀 16149）—— 合并原三档构造为单一决策器。
/// </summary>
public class GnbAoeComboGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：AOE 连（恶魔切/恶魔杀）";

	protected override CheckResult CheckReal()
	{
		if (GnbHelper.GetNearestEnemy() == null)
		{
			return new CheckResult(false, $"自身 {GnbHelper.AoeEnemyCountRange} 米内没有能打的怪");
		}
		if (GnbHelper.GetLastComboSpellId() == GnbSkills.恶魔切 && GnbHelper.GetComboTimeLeftSeconds() > 0.0 && ActionHelper.IsUnlocked(GnbSkills.恶魔杀))
		{
			return new CheckResult(true, "恶魔切 → 恶魔杀（续打 AOE 连击）");
		}
		if (!GnbHelper.ShouldUseAoe(GnbBaseComboGcd.CurrentAoeThreshold) && !GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, "没有达到 AOE 门槛（开关或敌人数不够）");
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.恶魔切))
		{
			return new CheckResult(false, "恶魔切还没解锁");
		}
		return new CheckResult(true, "AOE 连（恶魔切起头）");
	}

	public override PAction GetAction()
	{
		if (GnbHelper.GetLastComboSpellId() == GnbSkills.恶魔切 && GnbHelper.GetComboTimeLeftSeconds() > 0.0 && ActionHelper.IsUnlocked(GnbSkills.恶魔杀))
		{
			return GnbAction.Of(GnbSkills.恶魔杀, (ActionType)0);
		}
		return GnbAction.Of(GnbSkills.恶魔切, (ActionType)0);
	}
}
