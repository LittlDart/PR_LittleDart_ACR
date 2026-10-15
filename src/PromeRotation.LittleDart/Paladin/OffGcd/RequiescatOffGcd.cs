using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin.OffGcd;

public class RequiescatOffGcd : PldResolverBase
{
	protected override string ResolverDescription => "oGCD：安魂祈祷/绝对统治（强绑定）";

	protected override CheckResult CheckReal()
	{
		if (!PromeSettings.Instance.GetQt("绝对统治"))
		{
			return new CheckResult(false, "QT「绝对统治」没开");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!PldHelper.IsFofActive())
		{
			return new CheckResult(false, "不在战逃反应窗口内（要先打战逃反应）");
		}
		if (TwinDamage.IsRequiescatCastable())
		{
			return new CheckResult(true, "战逃窗口内，交安魂祈祷 / 绝对统治");
		}
		return new CheckResult(false, "安魂祈祷 / 绝对统治 还没冷却好");
	}

	public override PAction GetAction()
	{
		if (ActionHelper.IsUnlocked(36921u) && PldHelper.IsReadyWithCanCast(36921u) && !ActionHelper.RecentlyUsed(36921u, 500))
		{
			return PldAction.Of(36921u, (ActionType)1);
		}
		return PldAction.Of(7383u, (ActionType)1);
	}
}
