using LittleDart.Common.Pull;
using LittleDart.Gunbreaker.Mitigation;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbTrajectoryPullOffGcd : GnbResolverBase
{
	private static readonly PullJobProfile Slot = new PullJobProfile
	{
		JobName = "绝枪战士",
		LogPrefix = "[LittleDart-GNB/拉怪]",
		SkillId = 36934u,
		QtKey = "突进拉怪",
		MinAggro = () => GnbSettings.Instance?.PullMinAggro ?? 3,
		ActionRange = () => GnbHelper.GetActionRange(36934u),
		Readings = GnbMitigationProfile.Instance,
		BuildAction = () => GnbAction.Of(36934u, (ActionType)1),
		DiagnosticsEnabled = () => GnbDiagnostics.Enabled,
		Log = GnbHelper.LogTagged
	};

	/// <summary>
	/// 两条分支（都在公共层 <see cref="PullResolverCore"/>，2026-10-15 起）：
	/// <list type="number">
	/// <item>**贴身突进**（玩家需求）：**当前选定目标**在 15~20 米（到目标圈）⇒ 贴上去；</item>
	/// <item>拉怪：一仇数 ≥ <c>GnbSettings.PullMinAggro</c> 且范围内有「不是我一仇 / 没进战斗」的怪 ⇒ 拽过来。</item>
	/// </list>
	/// 本决策器只填技能那一格（弹道 36934），判据一行都不在职业侧。
	/// </summary>
	protected override string ResolverDescription => "oGCD：弹道·拉怪 / 贴身突进（漏网的怪拽过来；当前目标 15~20 米时贴上去）";

	protected override CheckResult CheckReal()
	{
		return PullResolverCore.Check(Slot);
	}

	public override PAction GetAction()
	{
		return PullResolverCore.Build(Slot);
	}
}
