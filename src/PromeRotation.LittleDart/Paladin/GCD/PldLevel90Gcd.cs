namespace LittleDart.Paladin.GCD;

public class PldLevel90Gcd : PldBracketQueueGcd
{
	public PldLevel90Gcd() : base(PldGcdPriority.BuildBracket(90))
	{
	}

	protected override string ResolverDescription => "等级档 90：整条 GCD 队列";

	protected override int BracketLevel => 90;
}
