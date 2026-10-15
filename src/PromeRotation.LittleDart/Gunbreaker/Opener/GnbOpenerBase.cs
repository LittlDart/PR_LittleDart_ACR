using System.Collections.Generic;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;

namespace LittleDart.Gunbreaker.Opener;

public abstract class GnbOpenerBase : IOpener
{
	public abstract string OpenerName { get; }

	public abstract List<PAction> InCombatSequence { get; }

	public virtual void InitializeCountdown(CountDownHandler countdownHandler)
	{
	}

	protected static void AddPotionIfEnabled(List<PAction> sequence)
	{
		if (!PromeSettings.Instance.GetQt(GnbQt.UsePotion))
		{
			return;
		}
		uint bestPotionId = GameData.GetBestPotionId();
		if (bestPotionId == 0)
		{
			return;
		}
		PAction potion = GnbAction.On(bestPotionId, ActionType.Item, ActionTargetType.Self);
		potion.RequiresVerification = true;
		sequence.Add(potion);
	}
}
