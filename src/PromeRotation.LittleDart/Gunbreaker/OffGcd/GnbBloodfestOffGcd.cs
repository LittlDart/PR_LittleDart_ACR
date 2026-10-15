using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbBloodfestOffGcd : GnbResolverBase
{
	protected override string ResolverDescription => "oGCD：血壤（补晶壤 + 开狮心）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(16164u))
		{
			return new CheckResult(false, "血壤还没解锁（76 级）");
		}
		if (!PromeSettings.Instance.GetQt("血壤"))
		{
			return new CheckResult(false, "QT「血壤」关着");
		}
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标（血壤必须选中一个敌人才能放）");
		}
		if (ActionHelper.GetAdjustedActionId(16164u) != 16164)
		{
			return new CheckResult(false, "血壤技能位已被替换成狮心连");
		}
		if (GnbHelper.ShouldUseAoeCombo() && !GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "群体连招中：血壤只在无情窗口内开（无情外不开，宁可空转）");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new CheckResult(false, "倾泻资源进行中");
		}
		bool qt = PromeSettings.Instance.GetQt("先打血壤连");
		if (qt && !GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "QT「先打血壤连」开着：等无情窗口的第一格");
		}
		if (GnbHelper.DelayBloodfestUntilNoMercy() && !GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "QT「延后血壤」开着：等无情窗口");
		}
		if (!qt && GnbHelper.IsNoMercyInside() && GnbHelper.HasGnashingChainInProgress())
		{
			return new CheckResult(false, "「先打血壤连」关着：无情内让位给子弹连");
		}
		int waitGcdCount = GnbHelper.BloodfestWaitGcdCount();
		if (!GnbHelper.IsBattleStart() && !GnbHelper.IsNoMercyInside() && !GnbHelper.IsNoMercyComingSoon(waitGcdCount))
		{
			return new CheckResult(false, $"无情还没进窗口（等它剩下 {waitGcdCount} 个 GCD 再开血壤）");
		}
		if (!ActionHelper.CanCast(16164u, (IBattleChara)null))
		{
			return new CheckResult(false, "血壤冷却中或不可施放");
		}
		return new CheckResult(true, $"开血壤（无情冷却 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Enemy(16164u, (ActionType)1);
	}
}
