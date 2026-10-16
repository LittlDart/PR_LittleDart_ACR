using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbLionHeartGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：狮心连（崛起之心/支配之心/终结之心）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(16164u))
		{
			return new CheckResult(false, "血壤还没解锁（76 级）");
		}
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(16164u);
		if (adjustedActionId == 16164)
		{
			return new CheckResult(false, "血壤没有被替换成狮心连");
		}
		if (GnbHelper.IsLionHeartHeld())
		{
			return new CheckResult(false, $"压着等无情窗口（心有灵狮还剩 {GnbHelper.GetAuraTimeLeft(3840u):F1}s，无情 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
		}
		if (!PromeSettings.Instance.GetQt("先打血壤连") && !GnbHelper.ShouldUseAoeCombo() && GnbHelper.IsNoMercyInside() && GnbHelper.IsGnashingAvailable() && GnbHelper.GetAuraTimeLeft(3840u) > 4.0 * (double)GnbHelper.GetGcdSeconds())
		{
			return new CheckResult(false, "「先打血壤连」关着：窗口里让位给子弹连");
		}
		IBattleChara? castTarget = GnbHelper.GetCastTarget(adjustedActionId);
		if (castTarget == null)
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但 {GnbHelper.GetMeleeRange():F0} 米内没有能打的怪"
				: "没有有效目标 / 不在近战射程内");
		}
		string targetName = ((object)((IGameObject)castTarget).Name).ToString();
		return new CheckResult(true, $"狮心连（adjusted={adjustedActionId}，打 {targetName}）");
	}

	public override PAction GetAction()
	{
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(16164u);
		uint actionId = adjustedActionId switch
		{
			36938u => 36938u, 
			36939u => 36939u, 
			_ => 36937u, 
		};
		return GnbAction.Aoe(actionId, (ActionType)0, GnbHelper.GetCastTarget(actionId));
	}
}
