using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

internal static class GnbComboGuard
{
	
	internal const int GnashingFangGcdCost = 3;

	
	
	
	
	
	
	internal static (bool Yield, string Reason) ShouldYieldToBaseCombo(int gcdCost)
	{
		if (gcdCost < 1)
		{
			gcdCost = 1;
		}
		
		if (!GnbHelper.IsBaseComboInProgress())
		{
			return (false, string.Empty);
		}
		
		if (GnbHelper.IsNoMercyInside())
		{
			return (false, string.Empty);
		}
		
		
		if (!GnbHelper.IsInMeleeRange())
		{
			return (false, string.Empty);
		}
		uint lastComboId = GnbHelper.GetLastComboSpellId();
		int remainingBaseGcds = lastComboId == GnbSkills.利刃斩 ? 2 : 1;
		return (true, $"基础连挂在 {BaseComboStepName(lastComboId)}（还要 {remainingBaseGcds} 个 GCD）："
			+ $"先补完第 3 段，再起本格 {gcdCost} GCD 的动作（进无情不带基础连击）");
	}

	private static string BaseComboStepName(uint lastComboId)
	{
		return lastComboId == GnbSkills.利刃斩 ? "利刃斩 1/3" : "残暴弹 2/3";
	}
}
