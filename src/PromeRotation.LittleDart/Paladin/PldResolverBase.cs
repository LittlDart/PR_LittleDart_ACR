using PromeRotation.Data;
using PromeRotation.Resolvers;

namespace LittleDart.Paladin;

public abstract class PldResolverBase : IDecisionResolver
{
	protected abstract string ResolverDescription { get; }

	/// <summary>
	/// 诊断用的可读名字。★ 2026-10-18 审计 P1：命中日志从 <c>Check()</c> 挪到真正派发处
	/// （<c>LittleDartRotation.Resolve()</c>）。
	/// </summary>
	public string Description => ResolverDescription;

	protected abstract CheckResult CheckReal();

	public abstract PAction GetAction();

	/// <summary>
	/// ★ 必须纯（PR 每帧调它刷调试面板，impl:91722）：只读状态、不写缓存、不打日志。
	/// 命中日志原来在这里调 <c>Diagnostics.OnHit</c>，会写节流字典并制造
	/// 「面板问了一次 = 命中」的假象（docs\18 记载的误导源），已移到 Resolve()。
	/// </summary>
	public CheckResult Check()
	{
		if (PromeSettings.Instance.GetQt(PldQt.StopAttack))
		{
			return new CheckResult(false, "停手");
		}
		return CheckReal();
	}
}
