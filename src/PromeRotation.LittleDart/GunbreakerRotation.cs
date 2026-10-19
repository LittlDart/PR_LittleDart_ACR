using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common;
using LittleDart.Gunbreaker;
using LittleDart.Gunbreaker.Always;
using LittleDart.Gunbreaker.GCD;
using LittleDart.Gunbreaker.OffGcd;
using LittleDart.Gunbreaker.Opener;
using LittleDart.Gunbreaker.Planners;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using PromeRotation.Rotation;
using PromeRotation.UI.QuickToggles;

namespace LittleDart;

[RotationMetadata(GnbRotationInfo.JobId, GnbRotationInfo.Name, GnbRotationInfo.Author, GnbRotationInfo.Version,
    ContentScope = AcrContentScope.Routine | AcrContentScope.HighEnd)]
public sealed class GunbreakerRotation : IRotation, IRotationLifecycle
{
	public static readonly string[] OpenerNames = new string[1] { "100级高难起手" };

	private readonly GnbRotationEventHandler _eventHandler = new GnbRotationEventHandler();

	private readonly List<IDecisionResolver> _alwaysResolvers = new List<IDecisionResolver>();

	private readonly List<IDecisionResolver> _gcdResolvers = new List<IDecisionResolver>();

	private readonly List<IDecisionResolver> _offGcdResolvers = new List<IDecisionResolver>();

	public static IReadOnlyDictionary<string, bool> QtList { get; } = GnbQt.All;

	public static IReadOnlyDictionary<string, Type> Openers { get; } = new Dictionary<string, Type> { [OpenerNames[0]] = typeof(GnbOpener100) };

	public GunbreakerRotation()
	{
		GnbSettings.Build();
		// QT 面板顺序 = GnbQt.All 的槽位序：显式写 QtDefinition.Order，
		// 这样非 Original 配色也按同一顺序排（AddQt 给不了 Order，只能落回「按 Label 排序」）。
		int qtOrder = 0;
		foreach (var (qtName, qtDefaultEnabled) in GnbQt.All)
		{
			PromeSettings.Instance.RegisterQt(new QtDefinition(qtName, qtName, qtDefaultEnabled, string.Empty, "常规", qtOrder++));
		}
		GnbQt.ApplyModePreset(GnbSettings.Instance.Mode == 1);
		// ★ 2026-10-15 玩家需求：自动冲刺（原来在 oGCD 列表末尾、且是**占位**：
		//   CheckReal() 恒返回「尚未实现」）改成挂 **Always 槽** + 接公共层判据
		//   Common\Sprint\SprintResolverCore（与骑士同一份判据）。
		//   为什么必须是 Always 槽（脱战也要能冲）：见该文件类注释。
		//   ⚠️「注册在 Always 列表」与「出手返回 ActionType.Always」必须**同时**成立。
		// ★ 2026-10-18：Always 顺序 = 落地无情 → 续剑 → 冲刺（docs\32 T9）。
		//   ⚠️ 续剑已从 _offGcdResolvers 移出：注册在 Always 列表与出手返回 ActionType.Always 必须同时成立。
		_alwaysResolvers.Add((IDecisionResolver)(object)new GnbLandingNoMercyAlways());
		_alwaysResolvers.Add((IDecisionResolver)(object)new GnbContinuationAlways());
		_alwaysResolvers.Add((IDecisionResolver)(object)new GnbSprintAlways());
		// ====================================================================
		//  四、GCD 注册表（2026-10-17 合并版：一个技能一格）
		//  旧版同一技能被拆成多格（狮心连 3 / 爆发击 6 / 子弹连 7 / 命运之环 3 …，共 30 格），
		//  合并后每个技能只有一个决策器，技能**内部**的先后由类里的 if 链决定；
		//  技能**之间**的先后仍然只由这里的顺序决定（docs\18 铁律 1：位置 = 优先级）。
		//  本顺序 = 旧轴（30 格）的等效还原，逐条理由写在各类文件头；群体验收口径见 docs\23 §9.1 / §11。
		// ====================================================================
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbPlanGcd());          // 起手相位尾巴（必须最先问）
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDoubleDownGcd());    // 倍攻（只在无情内）
		// ★ 单体轴是「音速破 :67 → 狮心连 :68」，群体轴是「狮心连 → 命运之环 → 音速破」（docs\23 §11），
		//   两条轴对这两个技能的先后要求正好相反 ⇒ 单一位置表达不了：这里按**群体轴**排，
		//   单体轴由 GnbLionHeartGcd 在**起头**那一格内部让位给音速破（支配之心 / 终结之心 连打不受影响）。
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbLionHeartGcd());     // 狮心连（起头会让位给音速破 / 子弹连续打）
		// 命运之环在单体轴恒失败（AoeCluster 闸门不过），放这里不影响单体；放这里是为了群体轴：
		// 旧群体档 命运之环 :95/:96 必须排在 音速破 :97 之前。
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbFatedCircleGcd());   // 命运之环（群体轴）
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbSonicBreakGcd());    // 音速破（倾泻链 / 子弹连续打会让位）
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbGnashingFangGcd());  // 子弹连（续打 → 倾泻 → 桥接 → 无情内 → 准备窗 → 两层充能）
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbBurstStrikeGcd());   // 爆发击（腾位 → 群体兜底 → 倾泻 → 无情内 → 无情前 → 药效）
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbAoeComboGcd());      // 恶魔切 / 恶魔杀
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbBaseComboGcd());     // 利刃斩 / 残暴弹 / 迅连斩
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbRangedGcd());        // 闪雷弹（远程兜底）
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbNoMercyOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbSuperbolideOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbRampartOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbCamouflageOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbNebulaOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbGreatNebulaOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbHeartOfStoneOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbHeartOfCorundumOffGcd());
		// ★ 2026-10-15：极光从「列表末尾（弓形冲波之后）」挪到这里 —— 与刚玉之心并排成**自保组**。
		//   理由：调度器一次只选中一个技能（`engine.ShouldCast` 是排他的），所以减伤组内部顺序无所谓；
		//   但 Aurora 原来排在血壤 / 爆破领域 / 弓形冲波**之后** ⇒ 血量掉到 70% 以下时，
		//   只要那几个伤害能力技就绪就会先抢走这一格 oGCD，极光要等到下一个窗口（≈2.5s 后）才交。
		//   自保类技能排在伤害能力技之前，才符合「血量触发的自保优先」这条口径。
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbAuroraOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbHeartOfLightOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbReprisalOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbArmsLengthOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbBloodfestOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbTrajectoryPullOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbBlastingZoneOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbBowShockOffGcd());
		// P0-2：注册表自检（错配时打日志 + 告警，不再靠兼容壳静默兜底）。
		GnbRotationEventHandler.VerifyResolverRegistration(_alwaysResolvers, _offGcdResolvers);
	}

	public IOpener? GetOpener()
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance == null)
		{
			return null;
		}
		// ★ 2026-10-18 审计 P1（S4）：轴判定只走 GnbPlanEngine.AxisIsDriving 这一个入口，
		//   不再直接调 GnbHelper.HasTimelineRunning（避免两处判定各自演化）。
		if (GnbPlanEngine.AxisIsDriving())
		{
			return null;
		}
		if (!instance.OpenerEnabled || !PromeSettings.Instance.GetQt(GnbQt.UseSingleTargetOpener))
		{
			return null;
		}
		if (GnbHelper.IsDailyMode())
		{
			return null;
		}
		IBattleChara me = Core.Me;
		if (((me != null) ? ((ICharacter)me).Level : 0) < 100)
		{
			return null;
		}
		bool trajectoryOpener = instance.TrajectoryOpener && !GnbHelper.WasPulledByTrajectory();
		if (instance.OpenerIndex == 0)
		{
			return (IOpener?)(object)new GnbOpener100(trajectoryOpener);
		}
		return null;
	}

	public IRotationEventHandler GetEventHandler()
	{
		return (IRotationEventHandler)(object)_eventHandler;
	}

	public PAction? NextAlways()
	{
		return Resolve(_alwaysResolvers);
	}

	public PAction? NextGcd()
	{
		return Resolve(_gcdResolvers);
	}

	public PAction? NextOffGcd()
	{
		return Resolve(_offGcdResolvers);
	}

	private static PAction? Resolve(List<IDecisionResolver> resolvers)
	{
		AbilityGate.BeginDecisionScope();
		try
		{
			foreach (IDecisionResolver resolver in resolvers)
			{
				CheckResult result = resolver.Check();
				if (result.Success)
				{
					// ★ 2026-10-18 审计 P1：命中日志只在**真正派发**这一条路径上打
					//   （原来在 GnbResolverBase.Check() 里，调试面板每帧都会触发一次）。
					if (resolver is GnbResolverBase gnbResolver)
					{
						GnbDiagnostics.OnHit(gnbResolver.Description, result.Message);
					}
					PAction action = resolver.GetAction();
					if (action != null)
					{
						ActionHelper.RecordAction(action.ActionId);
					}
					return action;
				}
			}
			return null;
		}
		finally
		{
			AbilityGate.EndDecisionScope();
		}
	}

	public void UpdateDebugStatus()
	{
		RotationManager.AlwaysSolverStatus.Clear();
		RotationManager.GcdSolverStatus.Clear();
		RotationManager.OffGcdSolverStatus.Clear();
		Collect(_alwaysResolvers, RotationManager.AlwaysSolverStatus);
		Collect(_gcdResolvers, RotationManager.GcdSolverStatus);
		Collect(_offGcdResolvers, RotationManager.OffGcdSolverStatus);
	}

	private static void Collect(List<IDecisionResolver> resolvers, List<SolverStatus> sink)
	{
		foreach (IDecisionResolver resolver in resolvers)
		{
			CheckResult resolverCheck = resolver.Check();
			sink.Add(new SolverStatus
			{
				Name = ((object)resolver).GetType().Name,
				Success = resolverCheck.Success,
				Message = resolverCheck.Message
			});
		}
	}

	public void DrawSettings()
	{
		GnbSettingUi.Draw();
	}

	public void DrawQTs()
	{
	}

	public void OnEnterAcr()
	{
		GnbSettingWindow.Register();
		GnbSettings instance = GnbSettings.Instance;
		if (instance != null && !instance.SettingsWindowAutoOpened)
		{
			instance.SettingsWindowAutoOpened = true;
			instance.Save();
			GnbSettingWindow.ToggleWindow();
		}
		_eventHandler.OnEnterRotation();
	}

	public void OnExitAcr()
	{
		GnbSettingWindow.Unregister();
		_eventHandler.OnExitRotation();
		GnbSettings.Instance?.Save();
	}
}
