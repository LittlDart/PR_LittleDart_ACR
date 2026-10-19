using ECommons.Logging;
using LittleDart.Gunbreaker.Skill;
using LittleDart.Gunbreaker.Planners;
using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.GCD;

public class GnbPlanGcd : GnbResolverBase
{
	public static readonly GnbPlanEngine Engine = new GnbPlanEngine(GnbPlan100Opener.Steps, GnbSkills.迅连斩);

	protected override string ResolverDescription => "GCD：起手相位尾巴（利刃斩/残暴弹/迅连斩 · 状态驱动）";

	protected override CheckResult CheckReal()
	{
		return Engine.CheckCurrent();
	}

	/// <summary>
	/// 出手。docs\39 §7 批次 G-1：原来这里是 <c>Engine.GetAction() ?? throw new InvalidOperationException(...)</c>，
	/// 属于「防御分支里的错误答案」—— 从 GetAction 里抛异常会打断 PR 整帧的决策派发。
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>为什么 null 确实可达（不是纯防御）</b>：<see cref="GnbPlanEngine.CheckCurrent"/> 与
	/// <see cref="GnbPlanEngine.GetAction"/> 各自独立跑一遍 <c>EvaluateStep</c>，两次之间以下任一条变化都会让
	/// 后者返回 <c>NoStep</c>：目标消失/死亡（<c>Precondition</c> 读实时状态）、GCD 队列被塞入动作、
	/// 轴上/停用状态翻转；另外步的 <c>Precondition</c>/<c>Intent</c> 抛异常时引擎也是返回 null（内部只 Warning）。
	/// 同一调用栈内断言「两次求值必然一致」并不成立。
	/// </para>
	/// <para>
	/// <b>兜底为什么是利刃斩</b>：<c>Check()</c> 已返回成功 ⇒ 按 docs\18 铁律 2 本帧必须交出真技能；
	/// 相位尾巴所有格的起点都是基础三连的第一段（<see cref="GnbPlan100Opener.Steps"/>），
	/// 交回利刃斩既合法、也和「相位窗口关闭后常规决策器会走的下一格」一致。
	/// </para>
	/// </remarks>
	public override PAction GetAction()
	{
		PAction? action = Engine.GetAction();
		if (action is not null)
		{
			return action;
		}
		PluginLog.Warning($"[LittleDart-GNB] 相位尾巴 Check 通过却取不到技能（{Engine.Status}）⇒ 兜底 利刃斩 {GnbSkills.利刃斩}");
		return GnbAction.Of(GnbSkills.利刃斩, (ActionType)0);
	}
}
