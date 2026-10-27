using System.Collections.Generic;
using PromeRotation.Data;

namespace LittleDart.Paladin.Opener;

public class PldOpener100 : PldOpenerBase
{
	public override string OpenerName => "100级标准起手";

	public override List<PAction> InCombatSequence => BuildDeterministicPrefix();
}
