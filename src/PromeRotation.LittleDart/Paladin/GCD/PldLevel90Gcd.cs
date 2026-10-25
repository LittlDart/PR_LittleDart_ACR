namespace LittleDart.Paladin.GCD;

/// <summary>
/// 90 级等级档：只在本档**精确等级**时接管 GCD，内部顺序由 <see cref="PldGcdPriority"/> 统一生成
/// （S1：不再单独维护一份 28 条的清单）。
/// </summary>
public class PldLevel90Gcd : PldBracketQueueGcd
{
	public PldLevel90Gcd() : base(PldGcdPriority.BuildBracket(90))
	{
	}

	protected override string ResolverDescription => "等级档 90：整条 GCD 队列";

	protected override int BracketLevel => 90;
}
