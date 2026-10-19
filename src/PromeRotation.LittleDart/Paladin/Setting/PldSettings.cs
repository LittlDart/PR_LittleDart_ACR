using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace LittleDart.Paladin.Setting;

public class PldSettings
{
	public static PldSettings Instance = null;

	private static string _path = string.Empty;

	private const string SettingsFolderName = "LittleDart";

	private const string SettingsFileName = "PldSettings.json";

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		IncludeFields = true
	};

	public float AoeRadius = 6f;

	public int AoeEnemyCount = 3;

	public float TwinDamageFofLockMin = 12.5f;

	public float TwinDamageFofLockMax = 60f;

	public float TwinDamageReqLockMin = 10f;

	public float TwinDamageReqLockMax = 60f;

	public uint HolySpiritMinMp = 2000u;

	public bool DumpHardCastHolySpirit;

	public float RampartThreshold = 0.6f;

	public float GuardianThreshold = 0.6f;

	public float HolySheltronThreshold = 0.7f;

	public float BulwarkThreshold = 0.6f;

	public float ReprisalThreshold = 0.6f;

	public float SupportThreshold = 0.7f;

	public bool MitigationRequireTankStance = true;

	public float MitigationEnemyCountRadius = 10f;

	public int MitigationDamageWindowMs = 2500;

	public int MitigationWaveEndDebounceMs = 3000;

	public int MitigationReprisalMinEnemiesIn5m = 3;


	public float MitigationTargetDeathSkipSeconds = 10f;

	public float MitigationTargetDeathSkipCooldownSeconds = 60f;

	public float MitigationClemencyDeathSeconds = 5f;

	public float MitigationSprintMinRange = 7f;

	public float MitigationSprintMaxRange = 23f;


	public uint ClemencyMinMp = 2000u;

	public bool OpenerEnabled = true;

	public int OpenerPreReadMs = 1500;

	public int InterveneKeepCharges = 1;

	public float InterveneKeepDistance;

	
	
	
	
	
	
	public bool InterveneKeepInBurst = true;

	public int IntervenePullMinAggro = 3;

	public int RangedHolySpiritMinDistance = 3;

	public int RangedShieldLobMinDistance = 8;

	public int SlideCastReleaseMs = 350;

	public bool AutoTankStanceIn4Man = true;

	public int MitigationInvulnHpPercent = 25;

	public bool AutoDetectBurst;

	public float BurstHpThreshold = 2f;

	public int Mode;

	public bool CurtainCallInCountdown;

	public bool ReprisalInOpener;

	public bool AutoAdjustQtOnModeSwitch = true;

	public bool SettingsWindowAutoOpened;

	public bool UseToast = true;

	public int OpenerIndex;

	public string DutyState = string.Empty;

	public bool AutoPullActive;

	public int AutoPullSelectorMode = 4;

	public bool PartnerPanelShow { get; set; } = true;

	public Vector2 PartnerPanelIconSize { get; set; } = new Vector2(50f, 50f);

	public static void Build(string? settingFolder = null)
	{
		_path = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "LittleDart", "PldSettings.json");
		Load();
	}

	public static void Load()
	{
		if (string.IsNullOrEmpty(_path))
		{
			_path = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "LittleDart", "PldSettings.json");
		}
		if (!File.Exists(_path))
		{
			Instance = new PldSettings();
			Instance.Save();
			return;
		}
		try
		{
			Instance = JsonSerializer.Deserialize<PldSettings>(File.ReadAllText(_path), JsonOptions) ?? new PldSettings();
		}
		catch (Exception exception)
		{
			Instance = new PldSettings();
			PluginLog.Error($"[LittleDart-PLD] PldSettings 读取失败，已重置为默认值：{exception}");
		}
	}

	public void Save()
	{
		try
		{
			string settingsDirectory = Path.GetDirectoryName(_path) ?? string.Empty;
			if (!string.IsNullOrEmpty(settingsDirectory))
			{
				Directory.CreateDirectory(settingsDirectory);
			}
			File.WriteAllText(_path, JsonSerializer.Serialize(this, JsonOptions));
		}
		catch (Exception exception)
		{
			PluginLog.Error($"[LittleDart-PLD] PldSettings 保存失败：{exception}");
		}
	}
}
