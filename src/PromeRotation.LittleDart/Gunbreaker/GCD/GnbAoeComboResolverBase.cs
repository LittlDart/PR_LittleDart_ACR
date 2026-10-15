using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public abstract class GnbAoeComboResolverBase : GnbResolverBase
{
	protected static CheckResult AoeCommonCheck()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!GnbHelper.ShouldUseAoeCombo())
		{
			return new CheckResult(false, $"没有达到群体连招门槛（QT 或敌人数 < {4}，现在 {GnbHelper.GetAoeEnemyCount()} 只）");
		}
		return new CheckResult(true, "群体连招");
	}
}
