using System.Collections.Generic;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;

namespace LittleDart.Paladin.Opener;

public abstract class PldOpenerBase : IOpener
{
	private const int InterveneInCountDownMs = 300;

	public abstract string OpenerName { get; }

	public abstract List<PAction> InCombatSequence { get; }

	public virtual void InitializeCountdown(CountDownHandler countdownHandler)
	{
		PldSettings instance = PldSettings.Instance;
		if (instance.CurtainCallInCountdown)
		{
			countdownHandler.AddAction(10000, PldAction.Self(3540u, (ActionType)1));
		}
		if (instance.OpenerPreReadMs > 0)
		{
			countdownHandler.AddAction(instance.OpenerPreReadMs, PldAction.Enemy(7384u, (ActionType)0));
		}
		countdownHandler.AddAction(300, PldAction.Enemy(16461u, (ActionType)1));
	}

	protected static void AddPotionIfEnabled(List<PAction> sequence)
	{
		if (!PromeSettings.Instance.GetQt(PldQt.UsePotion))
		{
			return;
		}
		uint bestPotionId = GameData.GetBestPotionId();
		if (bestPotionId == 0)
		{
			return;
		}
		sequence.Add(PldAction.On(bestPotionId, ActionType.Item, ActionTargetType.Self));
	}
}
