using System.Collections.Generic;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;

namespace LittleDart.Gunbreaker;

public static class GnbQt
{
	public const string StopAttack = "停手";

	public const string DailyMode = "日常模式";

	public const string UseSingleTargetOpener = "单体起手";

	public const string UseAoe = "AOE";

	public const string UseNoMercy = "无情";

	public const string LandingNoMercy = "落地无情";

	public const string UseBloodfest = "血壤";

	public const string DumpResource = "倾泻资源";

	public const string DelayBloodfest = "延后血壤";

	public const string BloodfestFirst = "先打血壤连";

	public const string DelayContinuation = "续剑后置";

	public const string PotionAmmoDump = "爆发药内卸晶壤";

	public const string UseLightningShot = "远离闪雷弹";

	public const string UseAutoMitigation = "自动减伤";

	public const string ActiveAttack = "主动攻击";

	public const string AutoTrajectoryPull = "突进拉怪";

	public const string AutoSprint = "自动冲刺";

	public const string UsePotion = "开场爆发药";

	private static readonly bool _dailyAtStartup = IsDailyModeAtStartup();

	public static readonly IReadOnlyDictionary<string, bool> All = new Dictionary<string, bool>
	{
		["停手"] = false,
		["日常模式"] = _dailyAtStartup,
		["单体起手"] = true,
		["AOE"] = false,
		["开场爆发药"] = false,
		["无情"] = true,
		["落地无情"] = false,
		["血壤"] = true,
		
		
		
		
		
		["先打血壤连"] = true,
		["续剑后置"] = true,
		["延后血壤"] = false,
		["倾泻资源"] = false,
		["爆发药内卸晶壤"] = false,
		["远离闪雷弹"] = true,
		["突进拉怪"] = _dailyAtStartup,
		["自动减伤"] = _dailyAtStartup,
		["自动冲刺"] = _dailyAtStartup,
		["主动攻击"] = false
	};

	public static readonly HashSet<string> AlwaysHidden = new HashSet<string> { "日常模式" };

	public static readonly HashSet<string> HiddenInHighEnd = new HashSet<string> { "自动冲刺", "自动减伤", "突进拉怪", "主动攻击" };

	public static readonly HashSet<string> HiddenInDaily = new HashSet<string> { "无情", "落地无情", "血壤", "倾泻资源", "开场爆发药", "延后血壤", "先打血壤连", "续剑后置", "爆发药内卸晶壤" };

	private static IEnumerable<string> OwnQts()
	{
		foreach (string key in All.Keys)
		{
			yield return key;
		}
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
		GnbSettings instance = GnbSettings.Instance;
		if (instance == null || instance.AutoAdjustQtOnModeSwitch)
		{
			PromeSettings.Instance.SetQt("落地无情", false);
			if (dailyMode)
			{
				PromeSettings.Instance.SetQt("单体起手", false);
				PromeSettings.Instance.SetQt("AOE", true);
				PromeSettings.Instance.SetQt("自动冲刺", true);
				PromeSettings.Instance.SetQt("先打血壤连", true);
				PromeSettings.Instance.SetQt("自动减伤", true);
				PromeSettings.Instance.SetQt("突进拉怪", true);
			}
			else
			{
				PromeSettings.Instance.SetQt("单体起手", true);
				PromeSettings.Instance.SetQt("AOE", false);
				PromeSettings.Instance.SetQt("自动冲刺", false);
				PromeSettings.Instance.SetQt("先打血壤连", true);
				PromeSettings.Instance.SetQt("自动减伤", false);
				PromeSettings.Instance.SetQt("突进拉怪", false);
				PromeSettings.Instance.SetQt("主动攻击", false);
			}
		}
	}

	private static bool IsDailyModeAtStartup()
	{
		try
		{
			GnbSettings instance = GnbSettings.Instance;
			return instance != null && instance.Mode == 1;
		}
		catch
		{
			return false;
		}
	}
}
