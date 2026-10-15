using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbBowShockOffGcd : GnbResolverBase
{
	protected override string ResolverDescription => "oGCD：弓形冲波（只在无情内）";

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(16159u))
		{
			return new CheckResult(false, "弓形冲波还没解锁（62 级）");
		}
		if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "只能在无情窗口内开");
		}
		if (!ActionHelper.CanCast(16159u, (IBattleChara)null))
		{
			return new CheckResult(false, "冷却中或不可施放");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, "弓形冲波（无情内）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16159u, (ActionType)1);
	}
}
