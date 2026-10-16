using System;
using System.Collections.Generic;
using LittleDart.Common;
using LittleDart.Paladin;
using LittleDart.Paladin.Always;
using LittleDart.Paladin.GCD;
using LittleDart.Paladin.OffGcd;
using LittleDart.Paladin.Opener;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.Resolvers;
using PromeRotation.Rotation;
using PromeRotation.UI.QuickToggles;

namespace LittleDart;

[RotationMetadata(RotationInfo.JobId, RotationInfo.Name, RotationInfo.Author, RotationInfo.Version,
    ContentScope = AcrContentScope.Routine | AcrContentScope.HighEnd)]
public sealed class LittleDartRotation : IRotation, IRotationLifecycle
{
	public static readonly string[] OpenerNames = new string[5] { "100级标准起手", "100级绝伊甸起手", "90级起手", "80级起手", "70级起手" };

	private readonly PldRotationEventHandler _eventHandler = new PldRotationEventHandler();

	private readonly List<IDecisionResolver> _alwaysResolvers = new List<IDecisionResolver>();

	private readonly List<IDecisionResolver> _gcdResolvers = new List<IDecisionResolver>();

	private readonly List<IDecisionResolver> _offGcdResolvers = new List<IDecisionResolver>();

	public static IReadOnlyDictionary<string, bool> QtList { get; } = PldQt.All;

	public static IReadOnlyDictionary<string, Type> Openers { get; } = new Dictionary<string, Type>
	{
		[OpenerNames[0]] = typeof(PldOpener100),
		[OpenerNames[1]] = typeof(PldOpener100Fru),
		[OpenerNames[2]] = typeof(PldOpener90),
		[OpenerNames[3]] = typeof(PldOpener80),
		[OpenerNames[4]] = typeof(PldOpener70)
	};

	public LittleDartRotation()
	{
		PldSettings.Build();
		// QT 面板顺序 = PldQt.All 的槽位序：显式写 QtDefinition.Order，
		// 这样非 Original 配色也按同一顺序排（AddQt 给不了 Order，只能落回「按 Label 排序」）。
		int qtOrder = 0;
		foreach (var (qtName, qtDefaultEnabled) in PldQt.All)
		{
			PromeSettings.Instance.RegisterQt(new QtDefinition(qtName, qtName, qtDefaultEnabled, string.Empty, "常规", qtOrder++));
		}
		PldQt.ApplyModePreset(PldSettings.Instance.Mode == 1);
		// ★ 2026-10-15 玩家需求：自动冲刺改挂 **Always 槽**（原来挂在 oGCD 列表末尾）。
		//   理由（PR 1.5.10.4 反编译核对，见 Common\Sprint\SprintResolverCore.cs 类注释）：
		//   ① 决策侧 oGCD 列表只在「有 GCD 在转」时才会被问到 ⇒ 脱战永远问不到；
		//   ② 执行侧 ActionQueueManager 按 pAction.Type 选队列，Always 队列由 ExecuteCombat
		//      无条件取用、且 GetIsTimely 对 Always 直接放行。
		//   ⚠️ 「注册在 Always 列表」与「出手返回 ActionType.Always」必须**同时**成立，
		//   少一边就是「入队后烂在队列里」。
		_alwaysResolvers.Add((IDecisionResolver)(object)new SprintAlways());
		_gcdResolvers.Add((IDecisionResolver)(object)new ClemencyGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofBladeOfFaithGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofBladeOfTruthGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofBladeOfValorGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new DumpAtonementGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new DumpSupplicationGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new DumpSepulchreGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new DumpHolySpiritGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofAoeConfiteorGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofHolyCircleRequiescatGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new HolyCircleGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new ProminenceGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new TotalEclipseGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new PldLevel70Gcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new PldLevel80Gcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new PldLevel90Gcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofGoringBladeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofConfiteorGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofSpiritRequiescatGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofAtonementGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new BaseComboAfter76AtonementGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new BaseComboAfter76SupplicationGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new BaseComboAfter76SepulchreGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofSpiritEmpoweredGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofRoyalAuthorityGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofRiotBladeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofFastBladeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofFillerSpiritGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofRequiescatSpiritRangedGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofRangedSpiritGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofSlideCastSpiritGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofStillRangedSpiritGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FofShieldLobGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new BaseComboHaloneGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new BaseComboBefore64Gcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new RiotBladeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new BaseComboAfter64Gcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new RoyalAuthorityGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new FastBladeGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new HolySpiritRangedGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new SlideCastHolySpiritGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new ReadCastHolySpiritRangedGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new ShieldLobGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new FofOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new RequiescatOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new BladeOfHonorOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new HallowedGroundOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new RampartOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new GuardianOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new SentinelOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new HolySheltronOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new SheltronOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new BulwarkOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new ReprisalOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new ArmsLengthOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new DivineVeilOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new CircleOfScornOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new ExpiacionOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new SpiritsWithinOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new IntervenePullOffGcd());
		_offGcdResolvers.Add((IDecisionResolver)(object)new InterveneOffGcd());
	}

	public IOpener? GetOpener()
	{
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return null;
		}
		if (!instance.OpenerEnabled || !PromeSettings.Instance.GetQt("单体起手"))
		{
			return null;
		}
		if (PldHelper.IsDailyMode())
		{
			return null;
		}
		return instance.OpenerIndex switch
		{
			0 => (IOpener?)(object)new PldOpener100(), 
			1 => (IOpener?)(object)new PldOpener100Fru(), 
			2 => (IOpener?)(object)new PldOpener90(), 
			3 => (IOpener?)(object)new PldOpener80(), 
			4 => (IOpener?)(object)new PldOpener70(), 
			_ => (IOpener?)null, 
		};
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
		PldSettingUi.Draw();
	}

	public void DrawQTs()
	{
	}

	public void OnEnterAcr()
	{
		PldSettingWindow.Register();
		PldSettings instance = PldSettings.Instance;
		if (instance != null && !instance.SettingsWindowAutoOpened)
		{
			instance.SettingsWindowAutoOpened = true;
			instance.Save();
			PldSettingWindow.ToggleWindow();
		}
		_eventHandler.OnEnterRotation();
	}

	public void OnExitAcr()
	{
		PldSettingWindow.Unregister();
		_eventHandler.OnExitRotation();
		PldSlideCast.OnDispose();
		PldSettings.Instance?.Save();
	}
}
