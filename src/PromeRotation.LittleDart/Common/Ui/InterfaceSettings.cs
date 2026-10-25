using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace LittleDart.Common.Ui;

public sealed class UiWindowLayoutState
{
	public bool HasPosition;

	public float X;

	public float Y;

	public bool HasSize;

	public float W;

	public float H;
}

public sealed class InterfaceSettings
{
	private static string _path = string.Empty;

	private static InterfaceSettings? _instance;

	private const string FolderName = "LittleDart";

	private const string FileName = "InterfaceSettings.json";

	
	private const int CurrentSchemaVersion = 1;

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		IncludeFields = true
	};

	
	
	
	
	
	public string ThemeId = ThemeCatalog.CyberdeckId;

	
	
	
	
	public int WindowKind;

	
	
	
	
	
	public bool DeckCollapsed;

	
	
	
	
	
	public int SchemaVersion;

	public Dictionary<string, UiWindowLayoutState> Layouts = new Dictionary<string, UiWindowLayoutState>(StringComparer.Ordinal);

	public static InterfaceSettings Instance => _instance ??= Load();

	
	public static bool UseShellWindow => Instance.WindowKind == 1;

	
	public static string SelectedThemeId => Instance.ThemeId;

	private static string ResolvePath()
	{
		if (!string.IsNullOrEmpty(_path))
		{
			return _path;
		}
		try
		{
			_path = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, FolderName, FileName);
		}
		catch (Exception exception)
		{
			PluginLog.Warning("[LittleDart/界面] 配置目录不可用，本次界面设置不会落盘：" + exception.Message);
			_path = string.Empty;
		}
		return _path;
	}

	private static InterfaceSettings Load()
	{
		string path = ResolvePath();
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return CreateDefault();
		}
		try
		{
			InterfaceSettings settings = JsonSerializer.Deserialize<InterfaceSettings>(File.ReadAllText(path), JsonOptions) ?? CreateDefault();
			if (Migrate(settings))
			{
				settings.Save();
			}
			return settings;
		}
		catch (Exception exception)
		{
			PluginLog.Error("[LittleDart/界面] InterfaceSettings 读取失败，已重置为默认值：" + exception);
			return CreateDefault();
		}
	}

	
	
	
	
	
	
	private static InterfaceSettings CreateDefault()
	{
		return new InterfaceSettings
		{
			SchemaVersion = CurrentSchemaVersion
		};
	}

	
	
	
	
	
	
	private static bool Migrate(InterfaceSettings settings)
	{
		if (settings.SchemaVersion >= CurrentSchemaVersion)
		{
			return false;
		}
		if (string.Equals(settings.ThemeId, ThemeCatalog.DefaultId, StringComparison.Ordinal))
		{
			settings.ThemeId = ThemeCatalog.CyberdeckId;
		}
		settings.SchemaVersion = CurrentSchemaVersion;
		return true;
	}

	
	public UiWindowLayoutState GetLayout(string key)
	{
		if (Layouts.TryGetValue(key, out UiWindowLayoutState? state) && state != null)
		{
			return state;
		}
		state = new UiWindowLayoutState();
		Layouts[key] = state;
		return state;
	}

	public void Save()
	{
		string path = ResolvePath();
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		try
		{
			
			if (SchemaVersion < CurrentSchemaVersion)
			{
				SchemaVersion = CurrentSchemaVersion;
			}
			string? directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}
			File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
		}
		catch (Exception exception)
		{
			PluginLog.Error("[LittleDart/界面] InterfaceSettings 保存失败：" + exception);
		}
	}
}
