using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// GCD：子弹连（烈牙 16146 / 猛兽爪 16147 / 凶禽爪 16150）——
/// <list type="bullet">
/// <item>合并原 6 档 <c>GnashingPolicy</c> 与 <c>GnbDumpGnashingFangGcd</c> 为单一决策器；</item>
/// <item>无情前准备窗口（<see cref="GnbHelper.IsNoMercyPrepWindow"/>）起头新增充能 &gt; 1.5 校验（待办 #9 · 已完成（docs\35 §8.3）），防止仅 1 层充能时提前打掉导致无情窗口内转不出子弹连。</item>
/// <item>连击护栏（docs\36 T2 第二处 / 2026-10-19 问题 3）：**只在起头前**问 <see cref="GnbComboGuard"/>；
/// 无情外基础连挂在 1/2 段时先让位补完第 3 段，避免「带半截基础连进无情」。
/// ⚠️ 护栏不许放回「续打下一段」之前 —— 烈牙连一旦开始必须连续打完，否则会白丢 凶禽爪 + 穿目
/// （实测 2026-10-19 03:18.945）。</item>
/// </list>
///
/// <para><b>★ 2026-10-18 官方口径审计 P0 修复（S5 / CHECKLIST C2）：</b>
/// 原实现把一次性标志 <c>GnbHelper.ConsumeOpenerBridge()</c> 写在 <see cref="CheckReal"/> 里。
/// PR 每帧调 <c>Check()</c> 刷调试面板（impl:91722，且排在 EnableAcr 判定之前），
/// 而 <c>OpenerHasBeenExecuted</c> 在进战入队那一刻就置 true（impl:91609-91610）⇒
/// 起手还在跑的时候，调试面板那一帧就会把桥接窗口烧掉，「迅连斩 → 烈牙」永远不会触发。
/// 现在拆成**纯判据** <see cref="Evaluate"/>：<c>Check()</c> 只读它，
/// 真正消费一次性标志的动作放到 <see cref="GetAction"/>（每个 GCD 最多一次）。</para>
public class GnbGnashingFangGcd : GnbResolverBase
{
	internal const double ChargeEpsilon = 0.98;

	protected override string ResolverDescription => "GCD：子弹连（烈牙/猛兽爪/凶禽爪）";

	/// <summary>纯判据的返回值：Action == null ⇒ 本格不成立。</summary>
	private readonly record struct GnashingDecision(PAction? Action, bool ConsumeOpenerBridge, string Message);

	protected override CheckResult CheckReal()
	{
		GnashingDecision decision = Evaluate();
		return new CheckResult(decision.Action is not null, decision.Message);
	}

	public override PAction GetAction()
	{
		GnashingDecision decision = Evaluate();
		// Check 刚返回成功、同一调用栈内状态不会再变；真走到这里为空只可能是
		// 毫秒级状态漂移，兜底回「按实时连击段出格」的老路径，绝不空手。
		if (decision.Action is null)
		{
			return BuildGnashingAction();
		}
		// ★ 一次性桥接标志只能在**真要出手**时消费（S5：相位推进/本地记录放 GetAction）。
		if (decision.ConsumeOpenerBridge)
		{
			GnbHelper.ConsumeOpenerBridge();
		}
		return decision.Action;
	}

	/// <summary>
	/// 纯判据：只读游戏状态，**不写任何字段**（成员顺序 = 原 CheckReal 的分支顺序，行为等价）。
	/// </summary>
	private static GnashingDecision Evaluate()
	{
		if (!GnbHelper.HasValidTarget())
		{
			return new GnashingDecision(null, false, "没有有效目标");
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.烈牙))
		{
			return new GnashingDecision(null, false, "烈牙还没解锁（60 级）");
		}
		if (GnbHelper.ShouldUseAoeCombo())
		{
			return new GnashingDecision(null, false, "群体连招中：子弹连是单体三段连，群怪里不打（交给群体轴）");
		}
		uint nextComboStep = NextComboStep();
		if (nextComboStep != 0)
		{
			// ★ 2026-10-19 修正：连击护栏**只在起头前问**，不能放在这里。
			//   烈牙连一旦开始（NextComboStep != 0），必须连续打完 —— 否则会白丢
			//   凶禽爪 + 穿目（实测日志 03:18.945：猛兽爪/裂膛之后被基础连抢走一格，
			//   改打迅连斩，穿目整发丢失）。docs\17 G08 备注①：「一旦启动就必须连续打完」
			//   优先于所有窗口闸门；docs\32 §7.9.2 漏点 C 也指出旧规则来得太晚。
			if (!GnbHelper.IsInMeleeRange())
			{
				return new GnashingDecision(null, false, "不在近战射程内（继续子弹连）");
			}
			return new GnashingDecision(GnbAction.Of(nextComboStep, ActionType.Gcd), false, $"子弹连（续打下一段 = {nextComboStep}）");
		}
		// 连击护栏（docs\32 §7.9.3 / docs\36 T2 / 2026-10-19 问题 3）：**只拦起头**。
		// 无情外基础连挂在 1/2 段 ⇒ 先让 BaseCombo 补完第 3 段，别在「半截基础连」上起
		// 3 个 GCD 的烈牙连（否则进无情时基础连还挂着，爆发结束后接不上）。
		// 判定逻辑只在 GnbComboGuard 一处（S1）；本格只是它的调用点。
		(bool yieldToBaseCombo, string comboGuardReason) = GnbComboGuard.ShouldYieldToBaseCombo(GnbComboGuard.GnashingFangGcdCost);
		if (yieldToBaseCombo)
		{
			return new GnashingDecision(null, false, comboGuardReason);
		}
		double charges = GnbHelper.GetGnashingFangCharges();
		if (charges < ChargeEpsilon)
		{
			return new GnashingDecision(null, false, "烈牙没有可用充能");
		}
		if (GnbHelper.GetAmmo() <= 0)
		{
			return new GnashingDecision(null, false, "没有晶壤，打不出烈牙（留一层进无情）");
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			return new GnashingDecision(null, false, "不在近战射程内");
		}
		if (GnbHelper.IsDumpResource())
		{
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), false, "倾泻资源：打子弹连");
		}
		if (GnbHelper.IsPostOpenerBridgeWindow())
		{
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), true, "起手收尾桥接：接上「迅连斩 → 烈牙」那一发（一次性）");
		}
		if (GnbHelper.IsNoMercyInside())
		{
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), false, "无情内：子弹连起头");
		}
		if (GnbHelper.IsNoMercyPrepWindow())
		{
			if (charges <= GnbHelper.NoMercyPrepMinGnashingCharges)
			{
				return new GnashingDecision(null, false, $"进无情前准备窗口：烈牙充能不足 {GnbHelper.NoMercyPrepMinGnashingCharges:F1} 层（当前 {charges:F2}），留进无情窗口内再起头");
			}
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), false, $"进无情前的准备窗口：烈牙充能 {charges:F2} > {GnbHelper.NoMercyPrepMinGnashingCharges:F1}，直接用这一发起头");
		}
		if (GnbHelper.HasGnashingReserveCharge())
		{
			return new GnashingDecision(GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd), false, "无情外 · 常规：还有两层充能，这一发起掉、留一层给穿目进无情");
		}
		return new GnashingDecision(null, false, $"只剩一层充能，留着给「穿目进无情」（还差 {2.0 - charges:F2} 层满）");
	}

	internal static PAction BuildGnashingAction()
	{
		uint nextComboStep = NextComboStep();
		if (nextComboStep == 0)
		{
			return GnbAction.Of(GnbSkills.烈牙, ActionType.Gcd);
		}
		return GnbAction.Of(nextComboStep, ActionType.Gcd);
	}

	internal static uint NextComboStep()
	{
		if (GnbHelper.HasAura(GnbSkills.撕喉预备))
		{
			return GnbSkills.猛兽爪;
		}
		if (GnbHelper.HasAura(GnbSkills.裂膛预备))
		{
			return GnbSkills.凶禽爪;
		}
		return GnbHelper.GetLastComboSpellId() switch
		{
			GnbSkills.烈牙 => GnbSkills.猛兽爪,
			GnbSkills.猛兽爪 => GnbSkills.凶禽爪,
			_ => GnbHelper.GetAmmoComboStep() switch
			{
				1 => GnbSkills.猛兽爪,
				2 => GnbSkills.凶禽爪,
				_ => 0u,
			},
		};
	}

	/// <summary>
	/// 无情内「本格该先起头子弹连」—— 社区轴（docs\14 §二 / docs\gnb-7.4-timeline.json）的
	/// 无情窗口 GCD 顺序是 **烈牙 → 倍攻 → 音速破 → 猛兽爪 → 凶禽爪 → 狮心连×3**：
	/// 子弹连排最前（它的三发续剑 撕喉/裂膛/穿目 都要落进 20s 窗口），狮心连排最后。
	/// <para>条件 = 无情内 + 单体轴 + 没有进行中的子弹连 + 近战打得到 + 烈牙已解锁 +
	/// 起头最低条件（充能 ≥ 0.98 + 晶壤 &gt; 0）。倍攻 / 音速破 / 狮心连三格调它让位；
	/// 判定只此一处（docs\18 S1）。</para>
	/// <para>⚠️ 这是 2026-10-19 按玩家口径「社区轴的处理应该是完美的」补的：实测
	/// PRLog-20261019-011841-149.log 里无情窗口打成 音速破 → 倍攻 → 狮心连×3 → 烈牙…
	/// 结果 凶禽爪 后面的 穿目 落在无情结束之后 0.12s（39.720 vs 窗口 ~39.6），
	/// 就是因为子弹连被排到了窗口最后。</para>
	/// <para>⚠️ 顺序**不受任何 QT 控制**（玩家 2026-10-19 口径：窗口里打的东西是固定那些，
	/// 只是先后顺序；和其他轴顺序无关）。QT「先打血壤连」默认值/显示名/注册一律保持原样。</para>
	/// </summary>
	internal static bool ShouldStartChainFirst()
	{
		if (!GnbHelper.IsNoMercyInside())
		{
			return false;
		}
		if (!ActionHelper.IsUnlocked(GnbSkills.烈牙))
		{
			// 54–59 级：音速破已解锁但烈牙（60 级）还没学，`GetGnashingFangCharges()` 会算出 2.0，
			// 不判解锁就会出现「倍攻/音速破/狮心连都让位、子弹连却起不来」的白让格。
			return false;
		}
		if (GnbHelper.ShouldUseAoeCombo())
		{
			// 群体轴不适用：子弹连是单体三段连（起头路径自己也会因群体闸门失败）。
			return false;
		}
		if (NextComboStep() != 0)
		{
			// 已经在烈牙连里：由本类「续打」分支负责，不需要其它格让位。
			return false;
		}
		if (!GnbHelper.IsInMeleeRange())
		{
			// 起头真的打得到才让位（否则 DD/SB/狮心连白让一格，最后还得交给远程兜底）。
			return false;
		}
		return GnbHelper.GetGnashingFangCharges() >= ChargeEpsilon && GnbHelper.GetAmmo() > 0;
	}
}
