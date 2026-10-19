using System.Collections.Generic;
using LittleDart.Paladin.Skill;
using PromeRotation.Data;

namespace LittleDart.Paladin.Opener;

public class PldOpener90 : PldOpenerBase
{
	public override string OpenerName => "90级起手";

	public override List<PAction> InCombatSequence => BuildSequence();

	private static List<PAction> BuildSequence()
	{
		List<PAction> list = new List<PAction>
		{
			
			PldAction.Of(PldSkills.先锋剑, ActionType.Gcd),
			PldAction.Of(PldSkills.暴乱剑, ActionType.Gcd)
		};
		PldOpenerBase.AddPotionIfEnabled(list);
		list.Add(PldAction.Of(PldSkills.王权剑, ActionType.Gcd));
		list.Add(PldAction.Self(PldSkills.战逃反应, ActionType.OffGcd));
		list.Add(PldAction.Of(PldSkills.安魂祈祷技能, ActionType.OffGcd));
		list.Add(PldAction.Of(PldSkills.沥血剑, ActionType.Gcd));
		list.Add(PldAction.Of(PldSkills.厄运流转, ActionType.OffGcd));
		list.Add(PldAction.Of(PldSkills.偿赎剑, ActionType.OffGcd));
		
		return list;
	}
}
