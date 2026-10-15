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
