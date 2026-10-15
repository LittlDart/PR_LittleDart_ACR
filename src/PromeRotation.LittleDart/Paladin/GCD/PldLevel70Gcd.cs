namespace LittleDart.Paladin.GCD;

public class PldLevel70Gcd : PldBracketQueueGcd
{
	protected override string ResolverDescription => "等级档 70：整条 GCD 队列";

	protected override int BracketLevel => 70;

	protected override PldResolverBase[] CreateQueue()
	{
		return new PldResolverBase[20]
		{
			new FofGoringBladeGcd(),
			new FofSpiritRequiescatGcd(),
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
