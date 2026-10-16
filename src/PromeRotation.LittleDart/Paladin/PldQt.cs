using System.Collections.Generic;
using LittleDart.Paladin.Setting;
using PromeRotation.Data;

namespace LittleDart.Paladin;

public static class PldQt
{
	// ★ QT 名 = 全局键名、裸名（绝枪那份同理）：装载前 ClearQts() ⇒ 同名不串值。见 docs\12 §2。
	public const string UseSingleTargetOpener = "单体起手";

	public const string DailyMode = "日常模式";

	public const string UseAoe = "AOE";

	public const string UseFof = "战逃反应";

	public const string UseRequiescat = "绝对统治";

	public const string DumpResource = "倾泻资源";

	public const string SlideCast = "滑步模式";

	public const string HolySpiritRanged = "远离圣灵";

	public const string ShieldLobRanged = "远离投盾";

	public const string DelayFof = "延后战逃";

	public const string SmartFof = "智能远战逃";

	public const string TwinDamageDirect = "双伤直出";

	public const string DelayConfiteor = "延后悔罪";

	public const string UseIntervene = "自动调停";

	public const string IntervenePull = "突进拉怪";

	public const string NoBurst = "不打爆发";

	public const string ActiveAttack = "主动攻击";

	public const string AutoSprint = "自动冲刺";

	public const string StopAttack = "停手";

	public const string UseAutoMitigation = "自动减伤";

	public const string MitigationClemency = "深仁厚泽";

	public const string DevDebug = "开发测试，别按";

	public const string UsePotion = "开场爆发药";

	private static readonly bool _dailyAtStartup = IsDailyModeAtStartup();

	// ★ 槽位顺序 = 面板顺序（Original 配色走 QuickToggles 插入序；非 Original 走 QtDefinition.Order，见构造函数）。
	//   与 GnbQt.All 共用同一套骨架；改一边必须同步另一边（见 docs\21 §四）。
	public static readonly IReadOnlyDictionary<string, bool> All = new Dictionary<string, bool>
	{
		["停手"] = false,
		["日常模式"] = _dailyAtStartup,
		["单体起手"] = true,
		["AOE"] = false,
		["开场爆发药"] = false,
		["战逃反应"] = true,
		["绝对统治"] = true,
		["双伤直出"] = false,
		["延后悔罪"] = false,
		["智能远战逃"] = false,
		["延后战逃"] = false,
		["不打爆发"] = false,
		["倾泻资源"] = false,
		["远离圣灵"] = !_dailyAtStartup,
		["远离投盾"] = _dailyAtStartup,
		["突进拉怪"] = _dailyAtStartup,
		["滑步模式"] = false,
		["自动调停"] = false,
		["自动减伤"] = _dailyAtStartup,
		["深仁厚泽"] = _dailyAtStartup,
		["自动冲刺"] = _dailyAtStartup,
		["主动攻击"] = false
	};

	public static readonly HashSet<string> AlwaysHidden = new HashSet<string> { "开发测试，别按", "日常模式" };

	public static readonly HashSet<string> HiddenInHighEnd = new HashSet<string> { "自动减伤", "深仁厚泽", "自动冲刺", "突进拉怪", "主动攻击" };

	public static readonly HashSet<string> HiddenInDaily = new HashSet<string> { "战逃反应", "绝对统治", "倾泻资源", "延后战逃", "智能远战逃", "双伤直出", "延后悔罪", "滑步模式", "开场爆发药" };

	private static IEnumerable<string> OwnQts()
	{
		foreach (string key in All.Keys)
		{
			yield return key;
		}
		yield return "开发测试，别按";
	}

	public static void ApplyQtVisibility(bool dailyMode)
	{
		HashSet<string> hiddenQts = PromeSettings.Instance.HiddenQts;
		foreach (string item in OwnQts())
		{
			hiddenQts.Remove(item);
		}
		HashSet<string> hashSet = new HashSet<string>(AlwaysHidden);
		hashSet.UnionWith(dailyMode ? HiddenInDaily : HiddenInHighEnd);
		foreach (string qtName in hashSet)
		{
			hiddenQts.Add(qtName);
		}
	}

	public static void ApplyModePreset(bool dailyMode)
	{
		ApplyQtVisibility(dailyMode);
		PromeSettings.Instance.SetQt("日常模式", dailyMode);
		PldSettings instance = PldSettings.Instance;
		if (instance != null && !instance.AutoAdjustQtOnModeSwitch)
		{
			return;
		}
		if (dailyMode)
		{
			PromeSettings.Instance.SetQt("单体起手", false);
			PromeSettings.Instance.SetQt("AOE", true);
			PromeSettings.Instance.SetQt("战逃反应", true);
			PromeSettings.Instance.SetQt("开场爆发药", false);
			PromeSettings.Instance.SetQt("自动减伤", true);
			PromeSettings.Instance.SetQt("自动冲刺", true);
			PromeSettings.Instance.SetQt("深仁厚泽", true);
			PromeSettings.Instance.SetQt("远离投盾", true);
			PromeSettings.Instance.SetQt("突进拉怪", true);
			return;
		}
		PromeSettings.Instance.SetQt("单体起手", true);
		PromeSettings.Instance.SetQt("AOE", false);
		PromeSettings.Instance.SetQt("战逃反应", true);
		PromeSettings.Instance.SetQt("开场爆发药", false);
		PromeSettings.Instance.SetQt("自动减伤", false);
		PromeSettings.Instance.SetQt("深仁厚泽", false);
		PromeSettings.Instance.SetQt("自动冲刺", false);
		PromeSettings.Instance.SetQt("远离圣灵", true);
		PromeSettings.Instance.SetQt("远离投盾", false);
		PromeSettings.Instance.SetQt("突进拉怪", false);
		PromeSettings.Instance.SetQt("主动攻击", false);
		if (instance != null)
		{
			instance.InterveneKeepDistance = 0f;
			instance.Save();
		}
	}

	private static bool IsDailyModeAtStartup()
	{
		try
		{
			PldSettings instance = PldSettings.Instance;
			return instance != null && instance.Mode == 1;
		}
		catch
		{
			return false;
		}
	}
}
