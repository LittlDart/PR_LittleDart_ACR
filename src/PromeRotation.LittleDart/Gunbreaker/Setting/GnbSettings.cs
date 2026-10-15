using System;
using System.IO;
using System.Text.Json;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace LittleDart.Gunbreaker.Setting;

public class GnbSettings
{
	public static GnbSettings Instance = null;

	private static string _path = string.Empty;

	private const string SettingsFolderName = "LittleDart";

	private const string SettingsFileName = "GnbSettings.json";

	public const int DailyModeValue = 1;

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		IncludeFields = true
	};

	public int Mode;

	public bool AutoAdjustQtOnModeSwitch = true;

	public bool OpenerEnabled = true;

	public int OpenerIndex;

	public bool TrajectoryOpener = true;

	public const int PullActionTrajectory = 0;

	public const int PullActionLightningShot = 1;

	public const int PullActionNone = 2;

	public int PullAction;

	public int PullMinAggro = 3;

	public int AoeEnemyCount = 3;

	public int AoeComboEnemyCount = 4;

	public bool MitigationRequireTankStance = true;

	public float MitigationInvulnThresholdPercent = 70f;

	public float MitigationDefaultNextPullSeconds = 45f;

	public float MitigationEnemyCountRadius = 10f;

	public int MitigationDamageWindowMs = 2500;

	public int MitigationPhase1EndMs = 1000;

	public int MitigationPhase2StartMs = 2500;

	public int MitigationWaveEndDebounceMs = 3000;

	public int MitigationReprisalMinEnemiesIn5m = 3;

	public int MitigationAoeWarnMs = 1500;

	public int MitigationArmsLengthAggroOver = 3;

	public float MitigationTargetDeathSkipSeconds = 10f;

	public float MitigationTargetDeathSkipCooldownSeconds = 60f;

	public int MitigationReserveMinCooldownSeconds = 20;

	public int MitigationMultiTargetEnemies = 3;

	public float MitigationStressEnemyWeight = 1f;

	public float MitigationStressDamageWeight = 2f;

	public float MitigationStressMidThreshold = 4f;

	public float MitigationStressHighThreshold = 8f;

		public bool AutoTankStanceIn4Man = true;

	public int MitigationScarceBigThresholdPercent = 25;

	public int MitigationBigThresholdPercent = 35;

	public int MitigationInvulnHpPercent = 25;

	public bool SettingsWindowAutoOpened;

	public bool UseToast = true;

	public bool AutoPullActive;

	public int AutoPullSelectorMode = 4;

	public string DutyState = string.Empty;

	public int LastAppliedMode = -1;

	public static void Build(string? settingFolder = null)
	{
		_path = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "LittleDart", "GnbSettings.json");
		Load();
	}

	public static void Load()
	{
		if (string.IsNullOrEmpty(_path))
		{
			_path = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "LittleDart", "GnbSettings.json");
		}
		if (!File.Exists(_path))
		{
			Instance = new GnbSettings();
			Instance.Save();
			return;
		}
		try
		{
			Instance = JsonSerializer.Deserialize<GnbSettings>(File.ReadAllText(_path), JsonOptions) ?? new GnbSettings();
		}
		catch (Exception exception)
		{
			Instance = new GnbSettings();
			PluginLog.Error($"{"[LittleDart-GNB]"} GnbSettings 读取失败，已重置为默认值：{exception}");
		}
	}

	public void Save()
	{
		try
		{
			string text = Path.GetDirectoryName(_path) ?? string.Empty;
			if (!string.IsNullOrEmpty(text))
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllText(_path, JsonSerializer.Serialize(this, JsonOptions));
		}
		catch (Exception exception)
		{
			PluginLog.Error($"{"[LittleDart-GNB]"} GnbSettings 保存失败：{exception}");
		}
	}
}
