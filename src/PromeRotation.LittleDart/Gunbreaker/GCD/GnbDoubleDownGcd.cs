using LittleDart.Common;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

public class GnbDoubleDownGcd : GnbResolverBase
{
	private const int AmmoCost = 2;

	protected override string ResolverDescription => "GCD：倍攻（耗 2 晶壤 · 无情内）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.倍攻))
		{
			return new CheckResult(false, "倍攻还没解锁（按 IsUnlocked(倍攻) 判定，不硬编码等级）");
		}
		if (!GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "不在无情窗口内");
		}
		
		
		
		
		
		
		
		
		if (GnbGnashingFangGcd.ShouldStartChainFirst()
			&& (!GnbHelper.ShouldPrioritizeLionHeartChain() || GnbHelper.GetAmmo() <= AmmoCost))
		{
			return new CheckResult(false, "子弹连先起头（社区轴无情窗口：烈牙 → 倍攻 → 音速破 → …）");
		}
		if (GnbHelper.GetAmmo() < AmmoCost)
		{
			return new CheckResult(false, $"晶壤不足（需要 {AmmoCost} 发）");
		}
		if (!AbilityGate.ReadyForNextGcd(GnbSkills.倍攻))
		{
			return new CheckResult(false, "倍攻冷却中或不可施放");
		}
		if (GnbHelper.ShouldUseAoeCombo())
		{
			if (GnbHelper.GetNearestEnemy() == null)
			{
				return new CheckResult(false, $"自身 {GnbHelper.AoeEnemyCountRange} 米内没有能打的怪");
			}
			return new CheckResult(true, "群体轴：倒倍攻");
		}
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new CheckResult(true, "倾泻资源·无情内：倒倍攻");
		}
		
		if (!GnbHelper.IsLevelAtLeast100() && GnbGnashingFangGcd.NextComboStep() == 0 && GnbHelper.IsGnashingAvailable())
		{
			return new CheckResult(false, "88–99 级无情内：先让子弹连 1（烈牙）起头，再打倍攻");
		}
		return new CheckResult(true, "无情内倒倍攻");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(GnbSkills.倍攻, (ActionType)0);
	}
}
