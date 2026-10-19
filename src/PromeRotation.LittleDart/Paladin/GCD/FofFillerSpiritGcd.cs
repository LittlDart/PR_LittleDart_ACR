using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Paladin.Skill;

namespace LittleDart.Paladin.GCD;

public class FofFillerSpiritGcd : PldResolverBase
{
	protected override string ResolverDescription => "战逃内：兜底圣灵";

	protected override CheckResult CheckReal()
	{
		if (!PldHelper.IsFofInside())
		{
			return new CheckResult(false, "不在战逃反应内");
		}
		if (!ActionHelper.IsUnlocked(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵尚未解锁");
		}
		if (!PldHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "距离太远（兜底圣灵只管近战档）");
		}
		IBattleChara me = Core.Me;
		if (me == null || ((ICharacter)me).CurrentMp < PldSettings.Instance.HolySpiritMinMp)
		{
			return new CheckResult(false, "蓝量不够读条圣灵");
		}
		if (PldHelper.IsMovingForCast())
		{
			return new CheckResult(false, "正在移动，读条会被打断");
		}
		if (!PldHelper.IsReadyAndInRangeForNextGcd(PldSkills.圣灵))
		{
			return new CheckResult(false, "圣灵不在可施放状态（冷却中或被占用）");
		}
		return new CheckResult(true, "战逃内：兜底圣灵（链尾读条圣灵）");
	}

	public override PAction GetAction()
	{
		return PldAction.Of(PldSkills.圣灵, ActionType.Gcd);
	}
}
