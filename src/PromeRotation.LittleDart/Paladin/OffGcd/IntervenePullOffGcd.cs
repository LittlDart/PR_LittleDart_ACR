using LittleDart.Common.Pull;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class IntervenePullOffGcd : PldResolverBase
{
	private static readonly PullJobProfile Slot = new PullJobProfile
	{
		JobName = "骑士",
		LogPrefix = "[LittleDart-PLD/拉怪]",
		SkillId = 16461u,
		QtKey = "突进拉怪",
		MinAggro = () => PldSettings.Instance.IntervenePullMinAggro,
		ActionRange = () => PldHelper.GetActionRange(16461u),
		Readings = PldMitigationProfile.Instance,
		BuildAction = () => PldAction.Of(16461u, (ActionType)1),
		DiagnosticsEnabled = () => Diagnostics.Enabled,
		Log = PldHelper.LogTagged
	};

	/// <summary>
	/// 两条分支（都在公共层 <see cref="PullResolverCore"/>，2026-10-15 起 **两职业共用**）：
	/// <list type="number">
	/// <item>**贴身突进**：**当前选定目标**在 15~20 米（到目标圈）⇒ 贴上去；</item>
	/// <item>拉怪：一仇数 ≥ <c>PldSettings.IntervenePullMinAggro</c> 且范围内有「不是我一仇 / 没进战斗」的怪 ⇒ 拽过来。</item>
	/// </list>
	/// ⚠️ 这条**不读** `PldSettings.InterveneKeepCharges` / `InterveneKeepDistance`
	/// —— 那两个是 `InterveneOffGcd`（调停当伤害技那一档）的设置，与拉怪/贴人这条无关。
	/// </summary>
	protected override string ResolverDescription => "oGCD：调停·拉怪 / 贴身突进（漏网的怪拽过来；当前目标 15~20 米时贴上去）";

	protected override CheckResult CheckReal()
	{
		return PullResolverCore.Check(Slot);
	}

	public override PAction GetAction()
	{
		return PullResolverCore.Build(Slot);
	}
}
