using Dalamud.Game.ClientState.Objects.Types;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;
using LittleDart.Gunbreaker.Skill;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// GCD：狮心连（崛起之心 36937 / 支配之心 36938 / 终结之心 36939）—— 合并原 <c>GnbLionHeartGcd</c> 与 <c>GnbDumpLionHeartGcd</c>。
/// </summary>
public class GnbLionHeartGcd : GnbResolverBase
{
	protected override string ResolverDescription => "GCD：狮心连（崛起之心/支配之心/终结之心）";

	protected override CheckResult CheckReal()
	{
		if (!ActionHelper.IsUnlocked(GnbSkills.血壤))
		{
			return new CheckResult(false, "血壤还没解锁（76 级）");
		}
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(GnbSkills.血壤);
		if (adjustedActionId == GnbSkills.血壤)
		{
			return new CheckResult(false, "血壤没有被替换成狮心连");
		}
		if (GnbHelper.IsLionHeartHeld())
		{
			return new CheckResult(false, $"压着等无情窗口（心有灵狮还剩 {GnbHelper.GetAuraTimeLeft(GnbSkills.心有灵狮):F1}s，无情 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
		}

		// ★ 2026-10-17（A 方案 · 恢复旧轴）：
		//   合并后本类注册在「音速破」之前 —— 这是**群体轴**要的顺序（倍攻 > 狮心连 > 命运之环 > 音速破，
		//   docs\23 §11）；但**单体轴**的旧顺序是 音速破 :67 → 狮心连 :68。两条轴对
		//   「狮心连 vs 音速破」的先后要求正好相反，单一注册位置表达不了 ⇒ 单体轴由本类反向让位。
		//   ⚠️ 只在**起头**那一格（adjusted == 36937）让位；支配之心 / 终结之心 必须连打，不许被抢。
		bool lionHeartStart = adjustedActionId == GnbSkills.崛起之心;
		// ① 链保护：子弹连没打完 → 让位（旧版靠 :66 的 NoMercyContinue 档位置保护）
		if (lionHeartStart && !GnbHelper.ShouldUseAoeCombo() && GnbHelper.HasGnashingChainInProgress())
		{
			return new CheckResult(false, "子弹连还没打完：先续打（旧版靠注册位置保护）");
		}
		// ② 单体轴：音速破优先（旧 :67 在狮心连 :68 之前）。
		//   ★ 2026-10-19 追加（玩家拍板：音速破也可优先）：QT「先打血壤连」开着时**保留**这条让位
		//   —— 音速破可以抢在狮心连前（也能抢在未起头的子弹连前，见 GnbSonicBreakGcd ①·2）。
		if (lionHeartStart && !GnbHelper.IsDumpResource() && !GnbHelper.ShouldUseAoeCombo()
			&& GnbSonicBreakGcd.SonicBreakGate().Success)
		{
			return new CheckResult(false, "单体轴：音速破优先（旧 :67 在狮心连 :68 之前）");
		}
		// 社区轴（docs\14 §二）：无情内子弹连还没起头时先让烈牙起头，狮心连排窗口最后
		// （烈牙 → 倍攻 → 音速破 → 猛兽爪 → 凶禽爪 → 狮心连×3）。
		// 窗口内容固定、只按轴排先后（玩家 2026-10-19 口径：和其他轴顺序无关）；
		// 判定只在 GnbGnashingFangGcd.ShouldStartChainFirst 一处（S1）。
		// 心有灵狮剩 ≤ 4 个 GCD 时不再让位（再等就把 buff 等没了，宁可裸打）。
		// ★ 例外（2026-10-19 玩家拍板：倍攻抢在狮心连前可接受）：QT「先打血壤连」开 + 狮心连打得到
		//   ⇒ 不再让位给子弹连，血壤连提前到倍攻之后（倍攻见 GnbDoubleDownGcd 的同一条判据）。
		if (lionHeartStart && !GnbHelper.IsDumpResource()
			&& !GnbHelper.ShouldUseAoeCombo() && !GnbHelper.ShouldPrioritizeLionHeartChain()
			&& GnbGnashingFangGcd.ShouldStartChainFirst()
			&& GnbHelper.GetAuraTimeLeft(GnbSkills.心有灵狮) > 4.0 * (double)GnbHelper.GetGcdSeconds())
		{
			return new CheckResult(false, "社区轴：子弹连先起头、狮心连排窗口最后");
		}
		IBattleChara? castTarget = GnbHelper.GetCastTarget(adjustedActionId);
		if (castTarget == null)
		{
			return new CheckResult(false, GnbHelper.ShouldUseAoeCombo()
				? $"群体成立，但 {GnbHelper.GetMeleeRange():F0} 米内没有能打的怪"
				: "没有有效目标 / 不在近战射程内");
		}
		string targetName = ((object)((IGameObject)castTarget).Name).ToString();
		if (GnbHelper.IsDumpResource() && GnbHelper.IsNoMercyInside())
		{
			return new CheckResult(true, $"倾泻资源·无情内：打狮心连（adjusted={adjustedActionId}，打 {targetName}）");
		}
		return new CheckResult(true, $"狮心连（adjusted={adjustedActionId}，打 {targetName}）");
	}

	public override PAction GetAction()
	{
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(GnbSkills.血壤);
		uint actionId = adjustedActionId switch
		{
			GnbSkills.支配之心 => GnbSkills.支配之心,
			GnbSkills.终结之心 => GnbSkills.终结之心,
			_ => GnbSkills.崛起之心,
		};
		return GnbAction.Aoe(actionId, (ActionType)0, GnbHelper.GetCastTarget(actionId));
	}
}
