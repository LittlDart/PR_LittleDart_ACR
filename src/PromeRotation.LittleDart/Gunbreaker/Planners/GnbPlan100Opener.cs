using System.Collections.Generic;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Gunbreaker.Planners;

public static class GnbPlan100Opener
{
	public static readonly IReadOnlyList<GnbPlanStep> Steps = [
		new GnbPlanStep("迅连斩（残暴弹连击还活着 · 中途进入）", IsBrutalShellLive, () => GnbAction.Of(16145u, (ActionType)0)),
		new GnbPlanStep("基础三连重建（利刃斩 → 残暴弹）", NeedsComboRebuild, RebuildIntent, Optional: false, 2),
		new GnbPlanStep("尾巴收工（连击已停在迅连斩）", IsSolidBarrelLanded, () => (PAction?)null)
	];

	public static bool IsBrutalShellLive()
	{
		if (!GnbHelper.IsBrutalShellComboAlive())
		{
			return false;
		}
		if (!ActionHelper.IsUnlocked(16145u))
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
		if (!ActionHelper.IsUnlocked(16137u))
		{
			return false;
		}
		uint lastComboSpellId = GnbHelper.GetLastComboSpellId();
		if (GnbHelper.GetComboTimeLeftSeconds() > 0.0 && lastComboSpellId != 16137)
		{
			return false;
		}
		return GnbHelper.IsInMeleeRange();
	}

	public static bool IsSolidBarrelLanded()
	{
		if (GnbHelper.GetLastComboSpellId() == 16145)
		{
			return GnbHelper.GetComboTimeLeftSeconds() > 0.0;
		}
		return false;
	}

	private static PAction? RebuildIntent()
	{
		uint lastComboSpellId = GnbHelper.GetLastComboSpellId();
		if (GnbHelper.GetComboTimeLeftSeconds() > 0.0 && lastComboSpellId == 16137 && ActionHelper.IsUnlocked(16139u))
		{
			return GnbAction.Of(16139u, (ActionType)0);
		}
		return GnbAction.Of(16137u, (ActionType)0);
	}
}
