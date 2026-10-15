using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.GCD;

public class DumpHolySpiritGcd : PldResolverBase
{
	protected override string ResolverDescription => "倾泻资源：圣灵（强化/读条兜底）";

	protected override CheckResult CheckReal()
	{
		if (!DumpQueue.IsEnabled())
		{
			return new CheckResult(false, "倾泻资源未开启 / 没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(7384u))
		{
			return new CheckResult(false, "圣灵尚未解锁");
		}
		if (PldHelper.IsAtonementChainRunning())
		{
			return new CheckResult(false, "赎罪剑连还没打完（先让前三档）");
		}
		if (!PldSettings.Instance.DumpHardCastHolySpirit)
		{
			return new CheckResult(false, "未开启「硬读圣灵」（只打有强化的圣灵）");
		}
		if (!DumpQueue.HasMpForHolySpirit())
		{
			return new CheckResult(false, "蓝量不够读条圣灵");
		}
		if (PldHelper.IsMovingForCast())
		{
			return new CheckResult(false, "正在移动，读条会被打断");
		}
		return new CheckResult(true, "倾泻资源：打圣灵（强化优先，读条兜底）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(7384u, (ActionType)0);
	}
}
