namespace LittleDart.Paladin.GCD;

/// <summary>
/// 80 级等级档：只在本档**精确等级**时接管 GCD，内部顺序由 <see cref="PldGcdPriority"/> 统一生成
/// （S1：不再单独维护一份 25 条的清单）。
/// </summary>
public class PldLevel80Gcd : PldBracketQueueGcd
{
	public PldLevel80Gcd() : base(PldGcdPriority.BuildBracket(80))
	{
	}

	protected override string ResolverDescription => "等级档 80：整条 GCD 队列";

	protected override int BracketLevel => 80;
}
