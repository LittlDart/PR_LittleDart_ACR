using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbAoeComboGcd : GnbResolverBase
{
	private readonly bool _comboThreshold;

	private readonly bool _continueOnly;

	protected override string ResolverDescription
	{
		get
		{
			if (!_continueOnly)
			{
				if (!_comboThreshold)
				{
					return "GCD：AOE 连（恶魔切/恶魔杀）";
				}
				return "群体轴：恶魔切 / 恶魔杀（起头）";
			}
			return "群体轴：恶魔杀（有连击 —— 保护连击，排在功能技之前）";
		}
	}

	public GnbAoeComboGcd(bool comboThreshold = false, bool continueOnly = false)
	{
		_comboThreshold = comboThreshold;
		_continueOnly = continueOnly;
	}

	protected override CheckResult CheckReal()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new CheckResult(false, "没有有效目标");
		}
		if (_continueOnly)
		{
			if (GnbHelper.GetLastComboSpellId() != 16141)
			{
				return new CheckResult(false, "没有待续的恶魔切连击");
			}
			if (!ActionHelper.IsUnlocked(16149u))
			{
				return new CheckResult(false, "恶魔杀还没解锁（40 级）");
			}
			if (!GnbHelper.IsInMeleeRange())
			{
				return new CheckResult(false, "不在近战射程内");
			}
			return new CheckResult(true, "恶魔切 → 恶魔杀（保护连击）");
		}
		if (!(_comboThreshold ? GnbHelper.ShouldUseAoeCombo() : GnbHelper.ShouldUseAoe(GnbBaseComboGcd.CurrentAoeThreshold)))
		{
			return new CheckResult(false, "没有达到 AOE 门槛（开关或敌人数不够）");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new CheckResult(false, "不在近战射程内");
		}
		if (!ActionHelper.IsUnlocked(16141u))
		{
			return new CheckResult(false, "恶魔切还没解锁");
		}
		return new CheckResult(true, "AOE 连");
	}

	public override PAction GetAction()
	{
		if (GnbHelper.GetLastComboSpellId() == 16141 && ActionHelper.IsUnlocked(16149u))
		{
			return GnbAction.Of(16149u, (ActionType)0);
		}
		return GnbAction.Of(16141u, (ActionType)0);
	}
}
