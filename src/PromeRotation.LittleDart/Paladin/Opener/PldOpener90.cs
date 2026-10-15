using System.Collections.Generic;
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
			PldAction.Of(9u, (ActionType)0),
			PldAction.Of(15u, (ActionType)0)
		};
		PldOpenerBase.AddPotionIfEnabled(list);
		list.Add(PldAction.Of(3539u, (ActionType)0));
		list.Add(PldAction.Self(20u, (ActionType)1));
		list.Add(PldAction.Of(7383u, (ActionType)1));
		list.Add(PldAction.Of(3538u, (ActionType)0));
		list.Add(PldAction.Of(23u, (ActionType)1));
		list.Add(PldAction.Of(25747u, (ActionType)1));
		list.Add(PldAction.Of(16459u, (ActionType)0));
		list.Add(PldAction.Of(25748u, (ActionType)0));
		list.Add(PldAction.Of(25749u, (ActionType)0));
		list.Add(PldAction.Of(25750u, (ActionType)0));
		list.Add(PldAction.Of(16460u, (ActionType)0));
		list.Add(PldAction.Of(36918u, (ActionType)0));
		list.Add(PldAction.Of(36919u, (ActionType)0));
		return list;
	}
}
