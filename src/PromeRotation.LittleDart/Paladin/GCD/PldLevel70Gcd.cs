namespace LittleDart.Paladin.GCD;

/// <summary>
/// 70 级等级档：只在本档**精确等级**时接管 GCD，内部顺序由 <see cref="PldGcdPriority"/> 统一生成
/// （S1：不再单独维护一份 20 条的清单）。
/// </summary>
public class PldLevel70Gcd : PldBracketQueueGcd
{
	public PldLevel70Gcd() : base(PldGcdPriority.BuildBracket(70))
	{
	}

	protected override string ResolverDescription => "等级档 70：整条 GCD 队列";

	protected override int BracketLevel => 70;
}
