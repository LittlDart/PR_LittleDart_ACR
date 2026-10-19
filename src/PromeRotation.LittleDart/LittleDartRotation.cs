using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Common;
using LittleDart.Paladin;
using LittleDart.Paladin.Always;
using LittleDart.Paladin.GCD;
using LittleDart.Paladin.OffGcd;
using LittleDart.Paladin.Opener;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
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
		
		
		int qtOrder = 0;
		foreach (var (qtName, qtDefaultEnabled) in PldQt.All)
		{
			PromeSettings.Instance.RegisterQt(new QtDefinition(qtName, qtName, qtDefaultEnabled, string.Empty, "常规", qtOrder++));
		}
		PldQt.ApplyModePreset(PldSettings.Instance.Mode == 1);
		
		
		
		
		
		
		
		_alwaysResolvers.Add((IDecisionResolver)(object)new SprintAlways());
		
		
		
		_gcdResolvers.AddRange(PldGcdPriority.BuildTopLevel());
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

	
	
	
	
	
	
	
	
	private static readonly int[] OpenerMinLevels = new int[5] { 100, 100, 90, 80, 70 };

	
	private static (int Requested, int Level) _lastOpenerFallbackLog = (-1, -1);

	public IOpener? GetOpener()
	{
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return null;
		}
		if (!instance.OpenerEnabled || !PromeSettings.Instance.GetQt(PldQt.UseSingleTargetOpener))
		{
			return null;
		}
		if (PldHelper.IsDailyMode())
		{
			return null;
		}
		int index = ResolveOpenerIndexForLevel(instance.OpenerIndex);
		if (index < 0)
		{
			return null;
		}
		return index switch
		{
			0 => (IOpener?)(object)new PldOpener100(), 
			1 => (IOpener?)(object)new PldOpener100Fru(), 
			2 => (IOpener?)(object)new PldOpener90(), 
			3 => (IOpener?)(object)new PldOpener80(), 
			4 => (IOpener?)(object)new PldOpener70(), 
			_ => (IOpener?)null, 
		};
	}

	
	
	
	
	private static int ResolveOpenerIndexForLevel(int requested)
	{
		int level = (Core.Me != null) ? ((ICharacter)Core.Me).Level : 0;
		if (requested < 0 || requested >= OpenerMinLevels.Length)
		{
			
			requested = 0;
		}
		if (level >= OpenerMinLevels[requested])
		{
			return requested;
		}
		for (int candidate = 0; candidate < OpenerMinLevels.Length; candidate++)
		{
			if (level >= OpenerMinLevels[candidate])
			{
				if (_lastOpenerFallbackLog != (requested, level))
				{
					_lastOpenerFallbackLog = (requested, level);
					PldHelper.Log($"[LittleDart-PLD/起手] 当前 {level} 级用不了「{OpenerNames[requested]}」（需要 {OpenerMinLevels[requested]} 级）⇒ 回退到「{OpenerNames[candidate]}」");
				}
				return candidate;
			}
		}
		if (_lastOpenerFallbackLog != (requested, level))
		{
			_lastOpenerFallbackLog = (requested, level);
			PldHelper.Log($"[LittleDart-PLD/起手] 当前 {level} 级低于所有起手档位（最低 {OpenerMinLevels[OpenerMinLevels.Length - 1]} 级）⇒ 本次不排起手");
		}
		return -1;
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
					
					
					if (resolver is PldResolverBase pldResolver)
					{
						Diagnostics.OnHit(pldResolver.Description, result.Message);
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
