using LittleDart.Gunbreaker.Setting;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbBaseComboGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：基础三连（利刃斩/残暴弹/迅连斩）";

	internal static int CurrentAoeThreshold => GnbSettings.Instance?.AoeEnemyCount ?? 3;

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (GnbHelper.ShouldUseAoe(CurrentAoeThreshold))
		{
			return new CheckResult(false, "多目标：让位给 AOE 连");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.利刃斩))
		{
			return new CheckResult(false, "利刃斩还没解锁");
		}
		return new CheckResult(true, "基础三连");
	}

	
	
	
	
	
	
	
	
	
	
	
	internal static uint NextBaseComboAction(bool comboActive, uint lastComboId, bool includeThirdStage)
	{
		if (comboActive && lastComboId == GnbSkills.利刃斩 && ActionHelper.IsUnlocked(GnbSkills.残暴弹))
		{
			return GnbSkills.残暴弹;
		}
		if (includeThirdStage && comboActive && lastComboId == GnbSkills.残暴弹 && ActionHelper.IsUnlocked(GnbSkills.迅连斩))
		{
			return GnbSkills.迅连斩;
		}
		return GnbSkills.利刃斩;
	}

	public override PAction GetAction()
	{
		uint lastComboSpellId = GnbHelper.GetLastComboSpellId();
		bool comboActive = GnbHelper.GetComboTimeLeftSeconds() > 0.0;
		return GnbAction.Of(NextBaseComboAction(comboActive, lastComboSpellId, includeThirdStage: true), (ActionType)0);
	}
}
