using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using LittleDart.Common.Ui;
using LittleDart.Paladin.Mitigation;
using PromeRotation.Data;
using PromeRotation.UI.Components;

namespace LittleDart.Paladin.Setting;

public class PldSettingUi
{
	public static readonly PldSettingUi Instance = new PldSettingUi();

	
	
	private static ModeSwitchAnim _modeAnim;

	private static readonly string[] TabNames = new string[5] { "战斗", "起手", "减伤", "通用", "开发" };

	private static int _tab;

	public static void Draw()
	{
		if (PldSettings.Instance == null)
		{
			ImGui.TextDisabled("LittleDart 设置尚未初始化，稍后再打开这个页签");
			return;
		}
		DrawModeSection();
		ImGui.Spacing();
		float x = MathF.Max(120f, ImGui.GetContentRegionAvail().X);
		if (PromeUi.PrimaryButton("pld_open_settings_window", "打开 LittleDart 设置窗口", new Vector2(x, 34f)))
		{
			PldSettingWindow.ToggleWindow();
		}
	}

	internal static void DrawWindowBody()
	{
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			ImGui.TextDisabled("LittleDart 设置尚未初始化，稍后再打开这个窗口");
			return;
		}
		DrawModeSection();
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		DrawTabs(instance);
	}

	private static void DrawTabs(PldSettings s)
	{
		bool isDaily = PldHelper.IsDailyMode();
		if (_tab < 0 || _tab >= TabNames.Length)
		{
			_tab = 0;
		}
		float x = ImGui.GetContentRegionAvail().X;
		float tabWidth = MathF.Max(56f, (x - 8f * (float)(TabNames.Length - 1)) / (float)TabNames.Length);
		for (int i = 0; i < TabNames.Length; i++)
		{
			if (i > 0)
			{
				ImGui.SameLine(0f, 8f);
			}
			if (PromeUi.TabButton("pld_tab_" + TabNames[i], TabNames[i], _tab == i, tabWidth))
			{
				_tab = i;
			}
		}
		ImGui.Spacing();
		switch (_tab)
		{
		case 0:
			if (isDaily)
			{
				DrawDailyBattleSection(s);
			}
			else
			{
				DrawHardBattleSection(s);
			}
			ImGui.Spacing();
			DrawRangedSection(s, !isDaily);
			break;
		case 1:
			if (isDaily)
			{
				PromeUi.SectionHeader("起手设置", (string)null, false);
			}
			else
			{
				DrawOpenerSection(s);
			}
			break;
		case 2:
			if (isDaily)
			{
				DrawDailyMitigationSection(s);
			}
			else
			{
				PromeUi.SectionHeader("减伤设置", (string)null, false);
			}
			break;
		case 3:
			DrawCommonSection(s);
			break;
		default:
			DrawDevSection();
			break;
		}
	}

	private static void DrawModeSection()
	{
		PromeUi.SectionHeader("战斗模式", (string)null, false);
		DrawModeSwitch();
	}

	private static void DrawModeSwitch()
	{
		bool isDaily = PldHelper.IsDailyMode();
		if (ThemeModeSwitch.Draw("###pld_mode_switch", isDaily, ref _modeAnim))
		{
			ApplyMode((!isDaily) ? 1 : 0);
		}
	}

	private static void ApplyMode(int mode)
	{
		PldSettings instance = PldSettings.Instance;
		if (instance != null)
		{
			bool appliedModeIsDaily = (byte)(instance.Mode = ((mode == 1) ? 1 : 0)) != 0;
			instance.Save();
			PromeSettings.Instance.SetQt(PldQt.DailyMode, appliedModeIsDaily);
			PromeSettings.Instance.SetQt(PldQt.AutoSprint, appliedModeIsDaily);
			if (!PldRotationEventHandler.ApplyModePreset())
			{
				PldHelper.ToastModeBanner(appliedModeIsDaily);
			}
			PldHelper.Log("已切换到" + (appliedModeIsDaily ? "日常" : "高难") + "模式");
		}
	}

	private static void DrawOpenerSection(PldSettings s)
	{
		PromeUi.SectionHeader("起手设置", (string)null, true);
		PldSettingWidgets.FieldCheckbox("启用起手", ref s.OpenerEnabled);
		if (s.OpenerEnabled)
		{
			PldSettingWidgets.ComboRow("起手方案", ref s.OpenerIndex, LittleDartRotation.OpenerNames);
			PldSettingWidgets.IntStepper("预读时间", ref s.OpenerPreReadMs, 0, 5000);
			PldSettingWidgets.QtCheckbox("开场爆发药", "开场爆发药");
			PldSettingWidgets.FieldCheckbox("倒计时使用圣光幕帘", ref s.CurtainCallInCountdown);
		}
	}

	private static void DrawHardBattleSection(PldSettings s)
	{
		PromeUi.SectionHeader("战斗设置", (string)null, true);
		PldSettingWidgets.IntSlider("调停保留层数", ref s.InterveneKeepCharges, 0, 2);
		PldSettingWidgets.IntStepperFloat("调停安全距离", ref s.InterveneKeepDistance, 0, 25);
		PldSettingWidgets.FieldCheckbox("爆发内不保留调停", ref s.InterveneKeepInBurst);
	}

	private static void DrawRangedSection(PldSettings s, bool includeSlideCast)
	{
		PromeUi.SectionHeader("远离设置", (string)null, true);
		PldSettingWidgets.IntSlider("圣灵释放阈值", ref s.RangedHolySpiritMinDistance, 3, 25);
		PldSettingWidgets.IntSlider("飞盾释放阈值", ref s.RangedShieldLobMinDistance, 3, 25);
		if (includeSlideCast)
		{
			PldSettingWidgets.RedQtCheckbox("滑步模式", "圣灵强制滑步，慎用");
			PldSettingWidgets.IntStepper("滑步放行阈值", ref s.SlideCastReleaseMs, 0, 1000);
		}
	}

	private static void DrawDailyBattleSection(PldSettings s)
	{
		PromeUi.SectionHeader("战斗设置", (string)null, true);
		PldSettingWidgets.FieldCheckbox("四人本自动盾姿", ref s.AutoTankStanceIn4Man);
		PldSettingWidgets.BoundCheckbox("pld_auto_pull", "自动拉怪", PldAutoPull.IsEnabled, PldAutoPull.SetEnabled);
		int selectorIndex = Array.IndexOf(PldAutoPull.Modes, PldAutoPull.CurrentMode);
		PldSettingWidgets.BoundComboRow("pld_auto_pull_mode", "目标选择器模式", (selectorIndex >= 0) ? selectorIndex : 0, PldAutoPull.ModeLabels, (int index) =>
		{
			PldAutoPull.SetSelectorMode(PldAutoPull.Modes[index]);
		});
		PldSettingWidgets.IntSlider("突进拉怪数量门槛", ref s.IntervenePullMinAggro, 1, 6);
	}

	private static void DrawDailyMitigationSection(PldSettings s)
	{
		PromeUi.SectionHeader("减伤设置", (string)null, true);
		PldSettingWidgets.QtCheckbox("自动减伤", "自动减伤");
		PldSettingWidgets.IntSlider("无敌血量", ref s.MitigationInvulnHpPercent, 20, 50);
		PldSettingWidgets.QtCheckbox("深仁厚泽", "深仁厚泽保命");
	}

	private static void DrawCommonSection(PldSettings s)
	{
		PromeUi.SectionHeader("通用", (string)null, false);
		PldSettingWidgets.IntSlider("AOE 敌人数阈值", ref s.AoeEnemyCount, 1, 10);
		PldSettingWidgets.FieldCheckbox("切换模式时自动调整 QT", ref s.AutoAdjustQtOnModeSwitch);
		PldSettingWidgets.FieldCheckbox("使用游戏内提示", ref s.UseToast);
	}

	private static void DrawDevSection()
	{
		PromeUi.SectionHeader("开发 / 排查", (string)null, false);
		bool enabled = Diagnostics.Enabled;
		if (PromeUi.ToggleRow("pld_dev_debug", "开发测试，别按", string.Empty, ref enabled))
		{
			Diagnostics.Enabled = enabled;
		}
		ImGui.Spacing();
		if (ImGui.CollapsingHeader("减伤调度器状态（诊断）###pld_mit_status", (ImGuiTreeNodeFlags)0))
		{
			PromeUi.StatusPill("pld_mit_active", "生效中：" + MitigationScheduler.Instance.DescribeActiveMits(), PromeUiTheme.Success);
			ImGui.Spacing();
			ImGui.TextUnformatted(MitigationScheduler.Instance.DescribeStatus());
		}
	}
}
