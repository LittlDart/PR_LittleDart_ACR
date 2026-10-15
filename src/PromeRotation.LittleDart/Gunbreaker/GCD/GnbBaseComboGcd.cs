using LittleDart.Gunbreaker.Setting;
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
		if (!ActionHelper.IsUnlocked(16137u))
		{
			return new CheckResult(false, "利刃斩还没解锁");
		}
		return new CheckResult(true, "基础三连");
	}

	public override PAction GetAction()
	{
		uint lastComboSpellId = GnbHelper.GetLastComboSpellId();
		bool comboActive = GnbHelper.GetComboTimeLeftSeconds() > 0.0;
		if (comboActive && lastComboSpellId == 16137 && ActionHelper.IsUnlocked(16139u))
		{
			return GnbAction.Of(16139u, (ActionType)0);
		}
		if (comboActive && lastComboSpellId == 16139 && ActionHelper.IsUnlocked(16145u))
		{
			return GnbAction.Of(16145u, (ActionType)0);
		}
		return GnbAction.Of(16137u, (ActionType)0);
	}
}
