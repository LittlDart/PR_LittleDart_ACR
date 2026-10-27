using System.Collections.Generic;
using LittleDart.Gunbreaker.GCD;
using PromeRotation.Data;
using PromeRotation.Helpers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Planners;

public static class GnbPlan100Opener
{
	public static readonly IReadOnlyList<GnbPlanStep> Steps = [
		new GnbPlanStep("迅连斩（残暴弹连击还活着 · 中途进入）", IsBrutalShellLive, () => GnbAction.Of(GnbSkills.迅连斩, (ActionType)0)),
		new GnbPlanStep("基础三连重建（利刃斩 → 残暴弹）", NeedsComboRebuild, RebuildIntent, Optional: false, 2),
		new GnbPlanStep("尾巴收工（连击已停在迅连斩）", IsSolidBarrelLanded, () => (PAction?)null)
	];

	public static bool IsBrutalShellLive()
	{
		if (!GnbHelper.IsBrutalShellComboAlive())
		{
			return false;
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.迅连斩))
		{
			return false;
		}
		return GnbHelper.IsInMeleeRange();
	}

	public static bool NeedsComboRebuild()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return false;
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.利刃斩))
		{
			return false;
		}
		uint lastComboSpellId = GnbHelper.GetLastComboSpellId();
		if (GnbHelper.GetComboTimeLeftSeconds() > 0.0 && lastComboSpellId != GnbSkills.利刃斩)
		{
			return false;
		}
		return GnbHelper.IsInMeleeRange();
	}

	public static bool IsSolidBarrelLanded()
	{
		if (GnbHelper.GetLastComboSpellId() == GnbSkills.迅连斩)
		{
			return GnbHelper.GetComboTimeLeftSeconds() > 0.0;
		}
		return false;
	}

	private static PAction? RebuildIntent()
	{
		// 判定只允许一处：基础三连的「下一格打什么」在 GnbBaseComboGcd.NextBaseComboAction（docs\18 S1 / docs\39 §7 批次 G-2）。
		// 本格不接第三段（includeThirdStage: false）—— 行为与批次 G 之前逐字一致。
		uint lastComboSpellId = GnbHelper.GetLastComboSpellId();
		bool comboActive = GnbHelper.GetComboTimeLeftSeconds() > 0.0;
		return GnbAction.Of(GnbBaseComboGcd.NextBaseComboAction(comboActive, lastComboSpellId, includeThirdStage: false), (ActionType)0);
	}
}
