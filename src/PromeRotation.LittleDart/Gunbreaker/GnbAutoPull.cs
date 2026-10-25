using System;
using ECommons.Logging;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;
using PromeRotation.TargetSelector;

namespace LittleDart.Gunbreaker;

public static class GnbAutoPull
{
	public static readonly SelectorModeType[] Modes = Enum.GetValues<SelectorModeType>();

	public static readonly string[] ModeLabels = new string[9] { "关闭（不自动选目标）", "最近", "最远", "血量最低", "血量最高", "3m 内血量最低", "3m 内血量最高", "6m 内血量最低", "6m 内血量最高" };

	private const SelectorModeType DisabledMode = (SelectorModeType)0;

	private const SelectorModeType FallbackMode = (SelectorModeType)4;

	private static bool _mirroredQt;

	public static bool IsEnabled => PromeSettings.Instance.AutoPull;

	public static SelectorModeType CurrentMode
	{
		get
		{
			return PromeSettings.Instance.TargetSelectorMode;
		}
	}

	public static bool IsArmed
	{
		get
		{
			if (IsEnabled)
			{
				return (int)CurrentMode > 0;
			}
			return false;
		}
	}

	public static SelectorModeType PreferredMode
	{
		get
		{
			int configuredMode = GnbSettings.Instance?.AutoPullSelectorMode ?? 4;
			if (configuredMode < 0 || configuredMode >= Modes.Length)
			{
				return (SelectorModeType)4;
			}
			SelectorModeType selectorMode = (SelectorModeType)configuredMode;
			if ((int)selectorMode != 0)
			{
				return selectorMode;
			}
			return (SelectorModeType)4;
		}
	}

	public static void SetEnabled(bool on)
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance != null)
		{
			instance.AutoPullActive = on;
			instance.Save();
		}
		if (on)
		{
			SelectorModeType preferredMode = PreferredMode;
			PromeSettings.Instance.AutoPull = true;
			PromeSettings.Instance.TargetSelectorMode = preferredMode;
			GnbHelper.Log("自动拉怪/主动攻击：已开启（目标选择器 = " + LabelOf(preferredMode) + "）");
		}
		else
		{
			PromeSettings.Instance.AutoPull = false;
			PromeSettings.Instance.TargetSelectorMode = (SelectorModeType)0;
			GnbHelper.Log("自动拉怪/主动攻击：已关闭（目标选择器一并关掉）");
		}
		PromeSettings.Instance.SetQt(GnbQt.ActiveAttack, on);
		_mirroredQt = on;
	}

	public static void SetSelectorMode(SelectorModeType mode)
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance != null && (int)mode != 0)
		{
			instance.AutoPullSelectorMode = (int)mode;
			instance.Save();
		}
		PromeSettings.Instance.TargetSelectorMode = mode;
		if ((int)mode == 0 && PromeSettings.Instance.AutoPull)
		{
			if (instance != null)
			{
				instance.AutoPullActive = false;
				instance.Save();
			}
			PromeSettings.Instance.AutoPull = false;
			GnbHelper.Log("目标选择器已关：自动拉怪一并关闭（只开自动拉怪而不选目标是「假开」）");
		}
		SyncMirrorFromHost();
	}

	public static void ApplyFromSettings()
	{
		try
		{
			GnbSettings instance = GnbSettings.Instance;
			if (instance != null)
			{
				if (!GnbHelper.IsDailyMode())
				{
					ClearAll();
				}
				else if (instance.AutoPullActive)
				{
					SelectorModeType preferredMode = PreferredMode;
					PromeSettings.Instance.AutoPull = true;
					PromeSettings.Instance.TargetSelectorMode = preferredMode;
					PromeSettings.Instance.SetQt(GnbQt.ActiveAttack, true);
					_mirroredQt = true;
					GnbHelper.Log("自动拉怪/主动攻击：按本 ACR 的记忆恢复为开启（目标选择器 = " + LabelOf(preferredMode) + "）");
				}
				else
				{
					PromeSettings.Instance.AutoPull = false;
					PromeSettings.Instance.SetQt(GnbQt.ActiveAttack, false);
					_mirroredQt = false;
				}
			}
		}
		catch (Exception exception)
		{
			PluginLog.Error($"{"[LittleDart-GNB]"} 自动拉怪/主动攻击：恢复设置失败：{exception}");
		}
	}

	public static void Reconcile()
	{
		bool qt = PromeSettings.Instance.GetQt(GnbQt.ActiveAttack);
		if (!GnbHelper.IsDailyMode())
		{
			if (qt || _mirroredQt || PromeSettings.Instance.AutoPull)
			{
				ClearAll();
			}
			return;
		}
		if (qt != _mirroredQt)
		{
			SetEnabled(qt);
			return;
		}
		bool autoPull = PromeSettings.Instance.AutoPull;
		if (autoPull != _mirroredQt)
		{
			PromeSettings.Instance.SetQt(GnbQt.ActiveAttack, autoPull);
			_mirroredQt = autoPull;
		}
	}

	private static void ClearAll()
	{
		PromeSettings.Instance.AutoPull = false;
		PromeSettings.Instance.TargetSelectorMode = (SelectorModeType)0;
		PromeSettings.Instance.SetQt(GnbQt.ActiveAttack, false);
		_mirroredQt = false;
	}

	private static void SyncMirrorFromHost()
	{
		bool autoPull = PromeSettings.Instance.AutoPull;
		PromeSettings.Instance.SetQt(GnbQt.ActiveAttack, autoPull);
		_mirroredQt = autoPull;
	}

	public static string LabelOf(SelectorModeType mode)
	{
		int modeIndex = Array.IndexOf(Modes, mode);
		if (modeIndex < 0 || modeIndex >= ModeLabels.Length)
		{
			return ((object)mode).ToString();
		}
		return ModeLabels[modeIndex];
	}

	public static bool TryParseSelectorMode(string? text, out SelectorModeType mode)
	{
		mode = (SelectorModeType)0;
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		text = text.Trim();
		if (int.TryParse(text, out var selectorIndex))
		{
			if (selectorIndex >= 0 && selectorIndex < Modes.Length)
			{
				mode = Modes[selectorIndex];
				return true;
			}
			return false;
		}
		if (Enum.TryParse<SelectorModeType>(text, true, out SelectorModeType parsedMode))
		{
			mode = (SelectorModeType)(int)parsedMode;
			return true;
		}
		for (int i = 0; i < ModeLabels.Length && i < Modes.Length; i++)
		{
			if (string.Equals(ModeLabels[i], text, StringComparison.Ordinal))
			{
				mode = Modes[i];
				return true;
			}
		}
		return false;
	}

	public static string Describe()
	{
		string dutyModeLabel = (GnbHelper.IsDailyMode() ? "日常" : "高难");
		string statusSuffix;
		if (IsArmed)
		{
			statusSuffix = "（生效中）";
		}
		else
		{
			statusSuffix = (IsEnabled ? "★假开：选择器是「关」，脱战时不会真的去拉" : "（未生效）");
		}
		return $"主动攻击/自动拉怪：开关={(IsEnabled ? "开" : "关")} 目标选择器={LabelOf(CurrentMode)} 模式={dutyModeLabel} {statusSuffix}" + (GnbHelper.IsDailyMode() ? string.Empty : " ⚠\ufe0f 高难模式下本功能不生效（交给时间轴）");
	}
}
