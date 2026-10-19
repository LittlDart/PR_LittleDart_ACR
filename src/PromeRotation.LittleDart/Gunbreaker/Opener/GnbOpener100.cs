using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Rotation;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.Opener;

public class GnbOpener100 : GnbOpenerBase
{
	public const int RequiredLevel = 100;

	private readonly bool _trajectoryOpener;

	private const int PullAtCountdownMs = 300;

	public override string OpenerName => "100级高难起手";

	public override List<PAction> InCombatSequence => BuildSequence();

	public GnbOpener100(bool trajectoryOpener)
	{
		_trajectoryOpener = trajectoryOpener;
	}

	public override void InitializeCountdown(CountDownHandler countdownHandler)
	{
		uint actionId = (GnbSettings.Instance?.PullAction ?? 0) switch
		{
			1 => GnbSkills.闪雷弹, 
			2 => 0u, 
			_ => GnbSkills.弹道, 
		};
		if (actionId == 0)
		{
			return;
		}
		bool isGcd = actionId == GnbSkills.闪雷弹;
		countdownHandler.AddAction(300, (Func<PAction>)(() =>
		{
			if (!GnbHelper.HasValidTarget())
			{
				return (PAction)null;
			}
			if (!GnbHelper.IsInMeleeRange() && GnbHelper.GetDistanceToTarget() > GnbHelper.GetActionRange(actionId))
			{
				return (PAction)null;
			}
			if (!ActionHelper.IsUnlocked(actionId) || !ActionHelper.CanCast(actionId, (IBattleChara)null))
			{
				return (PAction)null;
			}
			if (!isGcd)
			{
				GnbHelper.NotifyTrajectoryPull();
			}
			return GnbAction.Of(actionId, isGcd ? ActionType.Gcd : ActionType.OffGcd);
		}));
	}

	private List<PAction> BuildSequence()
	{
		List<PAction> list = new List<PAction>();
		if (_trajectoryOpener)
		{
			list.Add(V(GnbAction.Of(GnbSkills.弹道, (ActionType)1)));
		}
		list.Add(V(GnbAction.Of(GnbSkills.利刃斩, (ActionType)0)));
		list.Add(V(GnbAction.Enemy(GnbSkills.血壤, (ActionType)1)));
		list.Add(V(GnbAction.Of(GnbSkills.残暴弹, (ActionType)0)));
		
		
		
		
		
		
		
		
		
		
		
		
		
		list.Add(V(GnbAction.Self(GnbSkills.无情, (ActionType)1)));
		GnbOpenerBase.AddPotionIfEnabled(list);
		list.Add(V(GnbAction.Of(GnbSkills.烈牙, (ActionType)0)));
		list.Add(V(GnbAction.Of(GnbSkills.爆破领域, (ActionType)1)));
		list.Add(V(GnbAction.Of(GnbSkills.撕喉, (ActionType)1)));
		list.Add(V(GnbAction.Of(GnbSkills.倍攻, (ActionType)0)));
		list.Add(V(GnbAction.Of(GnbSkills.弓形冲波, (ActionType)1)));
		list.Add(V(GnbAction.Of(GnbSkills.音速破, (ActionType)0)));
		list.Add(V(GnbAction.Of(GnbSkills.猛兽爪, (ActionType)0)));
		list.Add(V(GnbAction.Of(GnbSkills.裂膛, (ActionType)1)));
		list.Add(V(GnbAction.Of(GnbSkills.凶禽爪, (ActionType)0)));
		list.Add(V(GnbAction.Of(GnbSkills.穿目, (ActionType)1)));
		list.Add(V(GnbAction.Of(GnbSkills.崛起之心, (ActionType)0)));
		list.Add(V(GnbAction.Of(GnbSkills.支配之心, (ActionType)0)));
		list.Add(V(GnbAction.Of(GnbSkills.终结之心, (ActionType)0)));
		return list;
	}

	private static PAction V(PAction action)
	{
		action.RequiresVerification = true;
		return action;
	}
}
