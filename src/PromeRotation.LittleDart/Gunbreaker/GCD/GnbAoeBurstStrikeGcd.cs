using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// 群体轴·爆发击兜底 —— <b>2026-10-14 已退役，不再注册进 <c>_gcdResolvers</c></b>。
///
/// 玩家口径：「删除爆发击，这是单体技能」。游戏内 Action 表实测 16162 爆发击的
/// <c>EffectRange</c> 字段为空（只打当前目标）⇒ 确实是单体，与「群怪里只打群体技能」冲突。
/// 群体里晶壤的泄压阀改成**命运之环**（它吃 1 发）。
///
/// <para>
/// 保留这个类而不是删掉，是为了「想回滚只需在 <c>GunbreakerRotation</c> 的群体段里
/// 加回一行 <c>new GnbAoeBurstStrikeGcd()</c>」。判据与改动记录见 <c>docs\23</c> §11。
/// </para>
/// </summary>
public class GnbAoeBurstStrikeGcd : GnbAoeComboResolverBase
{
	protected override string ResolverDescription => "群体轴：爆发击（命运之环花不出晶壤时兜底）";

	protected override CheckResult CheckReal()
	{
		CheckResult commonCheck = GnbAoeComboResolverBase.AoeCommonCheck();
		if (!commonCheck.Success)
		{
			return commonCheck;
		}
		if (!ActionHelper.IsUnlocked(16162u))
		{
			return new CheckResult(false, "爆发击还没解锁（30 级）");
		}
		if (GnbHelper.GetAmmo() <= 0)
		{
			return new CheckResult(false, "没有晶壤可倒");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		return new CheckResult(true, $"群体轴：倒一发爆发击（晶壤 {GnbHelper.GetAmmo()}）");
	}

	public override PAction GetAction()
	{
		return GnbAction.Of(16162u, (ActionType)0);
	}
}
