using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common;
using LittleDart.Gunbreaker;
using LittleDart.Gunbreaker.Always;
using LittleDart.Gunbreaker.GCD;
using LittleDart.Gunbreaker.OffGcd;
using LittleDart.Gunbreaker.Opener;
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
		_alwaysResolvers.Add((IDecisionResolver)(object)new GnbSprintAlways());
		_gcdResolvers.Insert(0, (IDecisionResolver)(object)new GnbPlanGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpDoubleDownGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpLionHeartGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpGnashingFangGcd(insideNoMercy: true));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpGnashingFangGcd(insideNoMercy: false));
		// ← 这里原来有 `GnbDumpPotionGcd`（倾泻资源 · 无情外喝爆发药）。
		//   2026-10-15 删除：它唯一的开关是设置里的「爆发药时机 = 无情外倒晶壤时喝」，
		//   而那个下拉框的前两项是死的、整体又与起手 QT「开场爆发药」重复（见 docs\24）。
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpBurstStrikeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDoubleDownGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbGnashingFangGcd(GnashingPolicy.NoMercyContinue));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbSonicBreakGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbLionHeartGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbGnashingFangGcd(GnashingPolicy.Level100Window));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbGnashingFangGcd(GnashingPolicy.SonicFirst));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbGnashingFangGcd(GnashingPolicy.OpenerBridge));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbGnashingFangGcd(GnashingPolicy.NoMercyPrep));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbPotionBurstStrikeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbOvercapBurstStrikeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbWindowBurstStrikeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbPreNoMercyBurstStrikeGcd());
		// ====================================================================
		//  五、群体连招（AOE 轴）
		//  ★★ 2026-10-14 玩家拍板改版。群体轴顺序：
		//     倍攻 > 狮心连 > 命运之环 > 音速破 > 恶魔切/恶魔杀
		//     另外两条硬判据：倍攻**只在无情内**放；血壤**只在无情内**开。
		//  依据（全部用游戏内 Action 表实测过，不是推理）：
		//   · 狮心连 36937/36938/36939：EffectRange=5、PreservesCombo=是、
		//     消耗状态 3840、**不耗晶壤** ⇒ 是**群体**技能，必须在群体轴放行
		//     （本工程上一轮按「单体三连」拦掉它是错的，错的源头是 docs\15:626；
		//      同表实测「倍攻/命运之环 PreservesCombo = 是」，并不会断连击）。
		//   · 恶魔切 16141 / 恶魔杀 16149：PreservesCombo = **否** ⇒ 只有这两格会断连击，
		//     所以「保护连击」那一档只需要紧挨在「恶魔切（起头）」之前，
		//     不必像原来那样排在所有功能技之前（那会让倍攻无谓推迟 1 个 GCD）。
		//   · 爆发击 16162 已从群体轴**删除**：它是单体；群体里晶壤的泄压阀是命运之环。
		//  判据与逐格对照见 docs\23 §11。
		// ====================================================================
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDoubleDownGcd(aoeOnly: true));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbLionHeartGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbFatedCircleInsideNoMercyGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbFatedCircleOutsideNoMercyGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbSonicBreakGcd(aoeOnly: true));
		// 「保护连击」：只判「上一格是不是恶魔切」，排在「恶魔切（起头）」之前。
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbAoeComboGcd(comboThreshold: true, continueOnly: true));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbAoeComboGcd(comboThreshold: true));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbFatedCircleGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbAoeComboGcd());
		// ★ 2026-10-14 追加：高难轴（100 级）无情里的「通用爆发击出口」。
		//   玩家口径「哪怕是高难的轴也是无情里爆发击优先级高于基础连」——
		//   注册在 GnbBaseComboGcd 之前，排在所有功能技之后，只接其它无情技都不成立的那一格。
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbNoMercyBurstStrikeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbBaseComboGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbRangedGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GnbContinuationOffGcd());
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
	}

	public IOpener? GetOpener()
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance == null)
		{
			return null;
		}
		if (GnbHelper.HasTimelineRunning())
		{
			return null;
		}
		if (!instance.OpenerEnabled || !PromeSettings.Instance.GetQt("单体起手"))
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
				if (resolver.Check().Success)
				{
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
