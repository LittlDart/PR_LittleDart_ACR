using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

internal static class PldGcdPriority
{
	private readonly record struct Entry(Func<PldResolverBase> Create, int Top, int L70, int L80, int L90);

	
	private static readonly Entry[] Entries =
	[
		new(() => new ClemencyGcd(), 1, 0, 0, 0),
		new(() => new FofBladeOfFaithGcd(), 2, 0, 0, 1),
		new(() => new FofBladeOfTruthGcd(), 3, 0, 0, 2),
		new(() => new FofBladeOfValorGcd(), 4, 0, 0, 3),
		new(() => new DumpAtonementGcd(), 5, 0, 0, 0),
		new(() => new DumpSupplicationGcd(), 6, 0, 0, 0),
		new(() => new DumpSepulchreGcd(), 7, 0, 0, 0),
		new(() => new DumpHolySpiritGcd(), 8, 0, 0, 0),
		new(() => new FofAoeConfiteorGcd(), 9, 0, 0, 0),
		new(() => new FofHolyCircleRequiescatGcd(), 10, 0, 0, 0),
		new(() => new HolyCircleGcd(), 11, 0, 0, 0),
		new(() => new ProminenceGcd(), 12, 0, 0, 0),
		new(() => new TotalEclipseGcd(), 13, 0, 0, 0),
		new(() => new PldLevel70Gcd(), 14, 0, 0, 0),
		new(() => new PldLevel80Gcd(), 15, 0, 0, 0),
		new(() => new PldLevel90Gcd(), 16, 0, 0, 0),
		new(() => new FofGoringBladeGcd(), 17, 1, 1, 4),
		new(() => new FofConfiteorGcd(), 18, 0, 0, 5),
		new(() => new FofSpiritRequiescatGcd(), 19, 2, 3, 6),
		new(() => new FofAtonementGcd(), 20, 0, 4, 7),
		new(() => new BaseComboAfter76AtonementGcd(), 21, 0, 17, 20),
		new(() => new BaseComboAfter76SupplicationGcd(), 22, 0, 5, 8),
		new(() => new BaseComboAfter76SepulchreGcd(), 23, 0, 6, 9),
		new(() => new FofSpiritEmpoweredGcd(), 24, 3, 7, 10),
		new(() => new FofRoyalAuthorityGcd(), 25, 4, 8, 11),
		new(() => new FofRiotBladeGcd(), 26, 5, 9, 12),
		new(() => new FofFastBladeGcd(), 27, 6, 10, 13),
		new(() => new FofFillerSpiritGcd(), 28, 7, 11, 14),
		new(() => new FofRequiescatSpiritRangedGcd(), 29, 8, 12, 15),
		new(() => new FofRangedSpiritGcd(), 30, 9, 13, 16),
		new(() => new FofSlideCastSpiritGcd(), 31, 10, 14, 17),
		new(() => new FofStillRangedSpiritGcd(), 32, 11, 15, 18),
		new(() => new FofShieldLobGcd(), 33, 12, 16, 19),
		new(() => new BaseComboHaloneGcd(), 34, 0, 0, 0),
		new(() => new BaseComboBefore64Gcd(), 35, 0, 0, 0),
		new(() => new RiotBladeGcd(), 36, 13, 18, 21),
		new(() => new BaseComboAfter64Gcd(), 37, 14, 19, 22),
		new(() => new RoyalAuthorityGcd(), 38, 15, 20, 23),
		new(() => new FastBladeGcd(), 39, 16, 21, 24),
		new(() => new HolySpiritRangedGcd(), 40, 17, 22, 25),
		new(() => new SlideCastHolySpiritGcd(), 41, 18, 23, 26),
		new(() => new ReadCastHolySpiritRangedGcd(), 42, 19, 24, 27),
		new(() => new ShieldLobGcd(), 43, 20, 25, 28),
		new(() => new FofConfiteorSoloGcd(), 0, 0, 2, 0),
	];

	
	public static List<IDecisionResolver> BuildTopLevel()
	{
		return Entries.Where(e => e.Top > 0).OrderBy(e => e.Top).Select(e => (IDecisionResolver)e.Create()).ToList();
	}

	
	public static PldResolverBase[] BuildBracket(int level)
	{
		return level switch
		{
			70 => Entries.Where(e => e.L70 > 0).OrderBy(e => e.L70).Select(e => e.Create()).ToArray(),
			80 => Entries.Where(e => e.L80 > 0).OrderBy(e => e.L80).Select(e => e.Create()).ToArray(),
			90 => Entries.Where(e => e.L90 > 0).OrderBy(e => e.L90).Select(e => e.Create()).ToArray(),
			_ => Array.Empty<PldResolverBase>(),
		};
	}
}
