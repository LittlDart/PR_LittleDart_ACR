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

		int qtOrder = 0;
		foreach (var (qtName, qtDefaultEnabled) in GnbQt.All)
		{
			PromeSettings.Instance.RegisterQt(new QtDefinition(qtName, qtName, qtDefaultEnabled, string.Empty, "常规", qtOrder++));
		}
		GnbQt.ApplyModePreset(GnbSettings.Instance.Mode == 1);

		_alwaysResolvers.Add((IDecisionResolver)(object)new GnbSprintAlways());
		_gcdResolvers.Insert(0, (IDecisionResolver)(object)new GnbPlanGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpDoubleDownGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpLionHeartGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpGnashingFangGcd(insideNoMercy: true));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDumpGnashingFangGcd(insideNoMercy: false));

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

		_gcdResolvers.Add((IDecisionResolver)(object)new GnbDoubleDownGcd(aoeOnly: true));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbLionHeartGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbFatedCircleInsideNoMercyGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbFatedCircleOutsideNoMercyGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbSonicBreakGcd(aoeOnly: true));

		_gcdResolvers.Add((IDecisionResolver)(object)new GnbAoeComboGcd(comboThreshold: true, continueOnly: true));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbAoeComboGcd(comboThreshold: true));
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbFatedCircleGcd());
		_gcdResolvers.Add((IDecisionResolver)(object)new GnbAoeComboGcd());

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
