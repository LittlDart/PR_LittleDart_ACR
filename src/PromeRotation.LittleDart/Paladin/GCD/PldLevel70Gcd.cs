namespace LittleDart.Paladin.GCD;

public class PldLevel70Gcd : PldBracketQueueGcd
{
	public PldLevel70Gcd() : base(PldGcdPriority.BuildBracket(70))
	{
	}

	protected override string ResolverDescription => "等级档 70：整条 GCD 队列";

	protected override int BracketLevel => 70;
}
