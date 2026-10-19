namespace LittleDart.Paladin.GCD;

public class PldLevel80Gcd : PldBracketQueueGcd
{
	public PldLevel80Gcd() : base(PldGcdPriority.BuildBracket(80))
	{
	}

	protected override string ResolverDescription => "等级档 80：整条 GCD 队列";

	protected override int BracketLevel => 80;
}
