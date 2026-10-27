using System.Collections.Generic;
using PromeRotation.Data;

namespace LittleDart.Paladin.Opener;

public class PldOpener80 : PldOpenerBase
{
	public override string OpenerName => "80级起手";

	public override List<PAction> InCombatSequence => BuildDeterministicPrefix();
}
