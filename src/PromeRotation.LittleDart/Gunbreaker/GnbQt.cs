using System.Collections.Generic;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;

namespace LittleDart.Gunbreaker;

public static class GnbQt
{
	// ★ QT 名 = 全局键名、裸名（不带职业前缀）：装载前框架先 ClearQts() + 同一周期只构造一个 ACR 实例 ⇒ 与骑士同名不串值；
	//   Original 配色下键名就是面板文字。完整依据见 docs\12 §2。
	public const string StopAttack = "停手";

	public const string DailyMode = "日常模式";

	public const string UseSingleTargetOpener = "单体起手";

	public const string UseAoe = "AOE";

	public const string UseNoMercy = "无情";

	public const string UseBloodfest = "血壤";

	public const string DumpResource = "倾泻资源";

	public const string DelayBloodfest = "延后血壤";

	public const string BloodfestFirst = "先打血壤连";

	public const string AllowGnashingDelay = "允许子弹连偏移";

	public const string ContinuationInNoMercy = "续剑进无情";

	public const string DelayContinuation = "续剑后置";

	public const string PotionAmmoDump = "爆发药内卸晶壤";

	public const string UseLightningShot = "远离闪雷弹";

	public const string UseAutoMitigation = "自动减伤";

	public const string ActiveAttack = "主动攻击";

	public const string AutoTrajectoryPull = "突进拉怪";

	public const string AutoSprint = "自动冲刺";

	public const string UsePotion = "开场爆发药";

	public const string DevDebug = "开发测试";

	private static readonly bool _dailyAtStartup = IsDailyModeAtStartup();

	// ★ 槽位顺序 = 面板顺序（Original 配色走 QuickToggles 插入序；非 Original 走 QtDefinition.Order，见构造函数）。
	//   两职业共用同一套骨架，改这里必须同步 PldQt.All：
	//   停手 → 日常模式 → 单体起手 → AOE → 开场爆发药 → 爆发核心 7 项 → 倾泻资源
	//   → 职业专属（远离 / 位移 / 爆发药）→ 突进拉怪 → 自动减伤 → 职业专属 → 自动冲刺 → 主动攻击。
	public static readonly IReadOnlyDictionary<string, bool> All = new Dictionary<string, bool>
	{
		["停手"] = false,
		["日常模式"] = _dailyAtStartup,
		["单体起手"] = true,
		["AOE"] = false,
		["开场爆发药"] = false,
		["无情"] = true,
		["血壤"] = true,
		["先打血壤连"] = true,
		["续剑进无情"] = true,
		["续剑后置"] = true,
		["延后血壤"] = false,
		["允许子弹连偏移"] = false,
		["倾泻资源"] = false,
		["爆发药内卸晶壤"] = false,
		["远离闪雷弹"] = true,
		["突进拉怪"] = _dailyAtStartup,
		["自动减伤"] = _dailyAtStartup,
		["自动冲刺"] = _dailyAtStartup,
		["主动攻击"] = false
	};

	public static readonly HashSet<string> AlwaysHidden = new HashSet<string> { "开发测试", "日常模式" };

	public static readonly HashSet<string> HiddenInHighEnd = new HashSet<string> { "自动冲刺", "自动减伤", "突进拉怪", "主动攻击" };

	public static readonly HashSet<string> HiddenInDaily = new HashSet<string> { "无情", "血壤", "倾泻资源", "开场爆发药", "延后血壤", "先打血壤连", "允许子弹连偏移", "续剑进无情", "续剑后置", "爆发药内卸晶壤" };

	private static IEnumerable<string> OwnQts()
	{
		foreach (string key in All.Keys)
		{
			yield return key;
		}
		yield return "开发测试";
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
			if (dailyMode)
			{
				PromeSettings.Instance.SetQt("单体起手", false);
				PromeSettings.Instance.SetQt("AOE", true);
				PromeSettings.Instance.SetQt("自动冲刺", true);
				// ★ 2026-10-14 玩家拍板：日常模式也把「先打血壤连」默认打开。
				//   该 QT 在日常里仍然隐藏（见 HiddenInDaily），效果是血壤只在无情窗口里开，
				//   从而「无情内血壤连 > 命运之环」这条顺序不依赖玩家拨开关。
				PromeSettings.Instance.SetQt("先打血壤连", true);
				PromeSettings.Instance.SetQt("续剑进无情", false);
				PromeSettings.Instance.SetQt("自动减伤", true);
				PromeSettings.Instance.SetQt("突进拉怪", true);
			}
			else
			{
				PromeSettings.Instance.SetQt("单体起手", true);
				PromeSettings.Instance.SetQt("AOE", false);
				PromeSettings.Instance.SetQt("自动冲刺", false);
				PromeSettings.Instance.SetQt("先打血壤连", true);
				PromeSettings.Instance.SetQt("续剑进无情", true);
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
