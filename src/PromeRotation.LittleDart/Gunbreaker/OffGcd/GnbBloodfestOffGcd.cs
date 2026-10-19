using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.OffGcd;

public class GnbBloodfestOffGcd : GnbResolverBase
{
	protected override string ResolverDescription => "oGCD：血壤（补晶壤 + 开狮心）";

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.血壤))
		{
			return new CheckResult(false, "血壤还没解锁（76 级）");
		}
		if (!PromeSettings.Instance.GetQt(GnbQt.UseBloodfest))
		{
			return new CheckResult(false, "QT「血壤」关着");
		}
		
		if (GnbHelper.IsAmmoAtCap())
		{
			return new CheckResult(false, $"晶壤已满（{GnbHelper.GetAmmo()}/{GnbHelper.GetAmmoCap()}），不开血壤");
		}
		
		if (GnbHelper.HasAura(GnbSkills.心有灵狮))
		{
			return new CheckResult(false, "已有「心有灵狮」，不开血壤");
		}
		
		
		
		
		
		
		
		
		
		
		
		if (!GnbHelper.IsNoMercyActive() && GnbHelper.WillNoMercyBeReadyInCurrentGcd())
		{
			return new CheckResult(false, "无情本 GCD 可开：血壤让位（不占无情窗口）");
		}
		if (GnbHelper.HasContinuationReadyInMelee())
		{
			string? continuationYieldReason = GnbHelper.GetContinuationNoMercyYieldReason();
			if (continuationYieldReason != null)
			{
				return new CheckResult(false, $"续剑正给无情让位：血壤同步让位（{continuationYieldReason}）");
			}
			float gcdRemainForContinuation = GnbHelper.GetGcdRemainSeconds();
			if (gcdRemainForContinuation < GnbHelper.BloodfestBeforeContinuationMinGcdRemain)
			{
				return new CheckResult(false, $"续剑待打且 GCD 余量不足（{gcdRemainForContinuation:F2}s < {GnbHelper.BloodfestBeforeContinuationMinGcdRemain:F2}s）：血壤让位，避免顶掉续剑");
			}
		}
		
		if (GnbHelper.DelayBloodfestUntilNoMercy() && !GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(false, "QT「延后血壤」开着：等无情窗口");
		}
		
		
		float gcdRemain = GnbHelper.GetGcdRemainSeconds();
		if (gcdRemain <= 0.3f)
		{
			return new CheckResult(false, $"GCD 剩余不足（{gcdRemain:F2}s ≤ 0.30s），避免卡 GCD");
		}
		if (GnbHelper.GetBloodfestCastTarget() == null)
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但自身 {GnbHelper.AoeEnemyCountRange} 米内没有能打的怪"
				: "没有有效目标（血壤必须选中一个敌人才能放）");
		}
		if (ActionHelper.GetAdjustedActionId(GnbSkills.血壤) != GnbSkills.血壤)
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
		if (!ActionHelper.CanCast(GnbSkills.血壤, (IBattleChara)null))
		{
			return new CheckResult(false, "血壤冷却中或不可施放");
		}
		return new CheckResult(true, $"开血壤（无情冷却 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Aoe(GnbSkills.血壤, (ActionType)1, GnbHelper.GetBloodfestCastTarget());
	}
}
