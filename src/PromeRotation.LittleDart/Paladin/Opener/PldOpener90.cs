using System.Collections.Generic;
using PromeRotation.Data;

namespace LittleDart.Paladin.Opener;

public class PldOpener90 : PldOpenerBase
{
	public override string OpenerName => "90级起手";

	public override List<PAction> InCombatSequence => BuildDeterministicPrefix();
}
