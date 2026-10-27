using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofShieldLobGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内远离：投盾";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.IsFofInside())
		{
			return new CheckResult(false, "不在战逃反应内");
		}
		if (!PromeSettings.Instance.GetQt(PldQt.ShieldLobRanged))
		{
			return new CheckResult(false, "QT「远离投盾」没开");
		}
		if (!PldHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.投盾))
		{
			return new CheckResult(false, "投盾尚未解锁");
		}
		if (PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "目标在近战范围内（该走近战档）");
		}
		if (!PldHelper.PassesRangedShieldLobGate())
		{
			return new CheckResult(false, "距离未达「飞盾释放阈值」");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.投盾))
		{
			return new CheckResult(false, "投盾不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内远离：投盾（兜底远程）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.投盾, ActionType.Gcd);
	}
}
