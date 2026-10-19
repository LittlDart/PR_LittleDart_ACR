using LittleDart.Common.Pull;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.OffGcd;

public class IntervenePullOffGcd : PldResolverBase
{
	private static readonly PullJobProfile Slot = new PullJobProfile
	{
		JobName = "骑士",
		LogPrefix = "[LittleDart-PLD/拉怪]",
		SkillId = PldSkills.调停,
		QtKey = "突进拉怪",
		MinAggro = () => PldSettings.Instance.IntervenePullMinAggro,
		ActionRange = () => PldHelper.GetActionRange(PldSkills.调停),
		Readings = PldMitigationProfile.Instance,
		BuildAction = () => PldAction.Of(PldSkills.调停, ActionType.OffGcd),
		DiagnosticsEnabled = () => Diagnostics.Enabled,
		Log = PldHelper.LogTagged
	};

	
	
	
	
	
	
	
	
	
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
