using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public abstract class GnbDumpResolverBase : GnbResolverBase
{
	protected static CheckResult DumpCommonCheck()
	{
		if (!GnbHelper.IsDumpResource())
		{
			return new CheckResult(false, "倾泻资源未开启");
		}
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		// ★ 2026-10-14：群体连招里「倾泻资源」整条让路。
		//   玩家口径「群怪阈值成立时只打群体技能」——倾泻链里的子弹连 / 爆发击 / 狮心连
		//   都是单体技能，而且它排在整个 GCD 列表最前面，会把群体轴（倍攻 / 命运之环 / 恶魔杀）
		//   整条绕开。原先缺这道排他，是本条需求里最大的一个漏洞。判据见 docs\23 §9。
		if (GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, "群体连招中：倾泻资源让位给群体轴（不在群怪里倒单体）");
		}
		return new CheckResult(true, "倾泻资源");
	}
}
