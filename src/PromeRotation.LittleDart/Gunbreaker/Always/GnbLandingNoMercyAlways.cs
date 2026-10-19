using LittleDart.Gunbreaker.Skill;
using PromeRotation.Data;
using PromeRotation.Managers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.Always;

/// <summary>
/// Always：落地无情（P1-1 状态机加固版）——
/// <list type="bullet">
/// <item>仅在非日常模式（<c>!IsDailyMode()</c>）且已进战（<c>EngageManager.GetBattleTime() &gt; 0</c>）时生效，防止脱战误开或绕过日常垂死锁；</item>
/// <item>使用 <see cref="GnbHelper.CanCastNow"/> 硬闸门，避免 0.3s 提前量假就绪；</item>
/// <item>挂 <c>RequiresVerification = true</c>，不在 <c>GetAction()</c> 内提前卸载，交由 <c>OnActionEffect</c> 确认 16138 真正出手或超时（3s）后再解除，被拒可重试。</item>
/// </list>
/// </summary>
public class GnbLandingNoMercyAlways : GnbResolverBase
{
	protected override string ResolverDescription => "Always：落地无情（Boss 落地即刻开启无情）";

	protected override CheckResult CheckReal()
	{
		if (!PromeSettings.Instance.GetQt(GnbQt.LandingNoMercy))
		{
			return new CheckResult(false, "QT「落地无情」未开启");
		}
		if (GnbHelper.IsDailyMode())
		{
			return new CheckResult(false, "日常模式禁用落地无情（防止绕过 15 秒垂死锁）");
		}
		if (EngageManager.GetBattleTime() <= 0.0)
		{
			return new CheckResult(false, "尚未进入战斗（防止脱战提前空开无情）");
		}
		if (!PromeSettings.Instance.GetQt(GnbQt.UseNoMercy) && !GnbHelper.IsDumpResource())
		{
			return new CheckResult(false, "QT「无情」未开启");
		}
		if (!GnbHelper.CanCastNow(GnbSkills.无情))
		{
			return new CheckResult(false, $"无情未就绪（冷却 {GnbHelper.GetNoMercyCooldownSeconds():F1}s）");
		}
		if (!GnbHelper.HasEnemyToFight())
		{
			return new CheckResult(false, "目标尚未落地或不在近战射程内");
		}
		return new CheckResult(true, "Boss 落地且无情就绪：立即释放落地无情（待 ActionEffect 确认后卸载）");
	}

	public override PAction GetAction()
	{
		PAction action = GnbAction.Self(GnbSkills.无情, ActionType.Always);
		action.RequiresVerification = true;
		return action;
	}
}
