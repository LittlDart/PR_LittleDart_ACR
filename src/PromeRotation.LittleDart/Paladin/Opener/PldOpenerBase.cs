using System.Collections.Generic;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;
using LittleDart.Paladin.Skill;

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
			countdownHandler.AddAction(10000, PldAction.Self(PldSkills.圣光幕帘, ActionType.OffGcd));
		}
		if (instance.OpenerPreReadMs > 0)
		{
			countdownHandler.AddAction(instance.OpenerPreReadMs, PldAction.Enemy(PldSkills.圣灵, ActionType.Gcd));
		}
		countdownHandler.AddAction(300, PldAction.Enemy(PldSkills.调停, ActionType.OffGcd));
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
