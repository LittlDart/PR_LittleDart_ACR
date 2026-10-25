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

	/// <summary>
	/// 基础三连「下一格打什么」的**唯一**判定（docs\18 S1；docs\39 §7 批次 G-2）。
	/// 常规 GCD 轴与本工程起手相位尾巴（<c>GnbPlan100Opener.RebuildIntent</c>）都只准调这里。
	/// </summary>
	/// <param name="comboActive">连击窗口还活着（<c>GnbHelper.GetComboTimeLeftSeconds() &gt; 0</c>）。</param>
	/// <param name="lastComboId">上一段连击 ID（<c>GnbHelper.GetLastComboSpellId()</c>）。</param>
	/// <param name="includeThirdStage">
	/// 是否允许接第三段（迅连斩）。两处行为差异就靠这个开关表达，**不是**两份判据：
	/// 常规轴 = <c>true</c>；起手相位尾巴的重建格 = <c>false</c>（它靠 <c>IntentCount: 2</c> 重复两次把
	/// 利刃斩 → 残暴弹 重建完，第三段由前一个「迅连斩还活着」的格接管）。
	/// </param>
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
