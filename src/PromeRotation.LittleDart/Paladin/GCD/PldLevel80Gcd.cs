namespace LittleDart.Paladin.GCD;

public class PldLevel80Gcd : PldBracketQueueGcd
{
	protected override string ResolverDescription => "等级档 80：整条 GCD 队列";

	protected override int BracketLevel => 80;

	protected override PldResolverBase[] CreateQueue()
	{
		return new PldResolverBase[25]
		{
			new FofGoringBladeGcd(),
			new FofConfiteorSoloGcd(),
			new FofSpiritRequiescatGcd(),
			new FofAtonementGcd(),
			new BaseComboAfter76SupplicationGcd(),
			new BaseComboAfter76SepulchreGcd(),
			new FofSpiritEmpoweredGcd(),
			new FofRoyalAuthorityGcd(),
			new FofRiotBladeGcd(),
			new FofFastBladeGcd(),
			new FofFillerSpiritGcd(),
			new FofRequiescatSpiritRangedGcd(),
			new FofRangedSpiritGcd(),
			new FofSlideCastSpiritGcd(),
			new FofStillRangedSpiritGcd(),
			new FofShieldLobGcd(),
			new BaseComboAfter76AtonementGcd(),
			new RiotBladeGcd(),
			new BaseComboAfter64Gcd(),
			new RoyalAuthorityGcd(),
			new FastBladeGcd(),
			new HolySpiritRangedGcd(),
			new SlideCastHolySpiritGcd(),
			new ReadCastHolySpiritRangedGcd(),
			new ShieldLobGcd()
		};
	}
}
