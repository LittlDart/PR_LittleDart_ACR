using System.Collections.Generic;
using PromeRotation.Data;

namespace LittleDart.Paladin.Opener;

public class PldOpener70 : PldOpenerBase
{
	public override string OpenerName => "70级起手";

	public override List<PAction> InCombatSequence => BuildDeterministicPrefix();
}
