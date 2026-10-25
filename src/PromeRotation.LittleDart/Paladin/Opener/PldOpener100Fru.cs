using System.Collections.Generic;
using PromeRotation.Data;

namespace LittleDart.Paladin.Opener;

public class PldOpener100Fru : PldOpenerBase
{
	public override string OpenerName => "100级绝伊甸起手";

	public override List<PAction> InCombatSequence => BuildDeterministicPrefix(doubleHolySpirit: true);
}
