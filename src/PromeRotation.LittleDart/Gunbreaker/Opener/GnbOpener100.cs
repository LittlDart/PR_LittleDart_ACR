using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Rotation;

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
			1 => 16143u,
			2 => 0u,
			_ => 36934u,
		};
		if (actionId == 0)
		{
			return;
		}
		bool isGcd = actionId == 16143;
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
			list.Add(V(GnbAction.Of(36934u, (ActionType)1)));
		}
		list.Add(V(GnbAction.Of(16137u, (ActionType)0)));
		list.Add(V(GnbAction.Enemy(16164u, (ActionType)1)));
		list.Add(V(GnbAction.Of(16139u, (ActionType)0)));

		list.Add(V(GnbAction.Self(16138u, (ActionType)1)));
		GnbOpenerBase.AddPotionIfEnabled(list);
		list.Add(V(GnbAction.Of(16146u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16165u, (ActionType)1)));
		list.Add(V(GnbAction.Of(16156u, (ActionType)1)));
		list.Add(V(GnbAction.Of(25760u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16159u, (ActionType)1)));
		list.Add(V(GnbAction.Of(16153u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16147u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16157u, (ActionType)1)));
		list.Add(V(GnbAction.Of(16150u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16158u, (ActionType)1)));
		list.Add(V(GnbAction.Of(36937u, (ActionType)0)));
		list.Add(V(GnbAction.Of(36938u, (ActionType)0)));
		list.Add(V(GnbAction.Of(36939u, (ActionType)0)));
		return list;
	}

	private static PAction V(PAction action)
	{
		action.RequiresVerification = true;
		return action;
	}
}
