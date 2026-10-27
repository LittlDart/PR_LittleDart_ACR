using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using LittleDart.Common.Ui;
using PromeRotation.UI.Components;

namespace LittleDart.Gunbreaker.Setting;

public class GnbSettingUi
{
	public static readonly GnbSettingUi Instance = new GnbSettingUi();

	
	
	private static ModeSwitchAnim _modeAnim;

	private static readonly string[] TabNames = new string[5] { "战斗", "起手", "减伤", "通用", "开发" };

	
	
	
	
	internal static readonly string[] ShellPageNames = new List<string>(TabNames) { "主题" }.ToArray();

	
	
	
	
	
	internal static readonly string[] PromeTabNames = new List<string>(TabNames) { "主题" }.ToArray();

	
	internal static int PageCount => TabNames.Length;

	private static int _tab;

	private static readonly string[] PullActionNames = new string[3] { "弹道（默认）", "闪雷弹", "不主动开怪" };

	public static void Draw()
	{
		UiKit.Surface = UiSurface.Prome;
		if (GnbSettings.Instance == null)
		{
			ImGui.TextDisabled("LittleDart 设置尚未初始化，稍后再打开这个页签");
			return;
		}
		DrawModeSection();
		ImGui.Spacing();
		float x = MathF.Max(120f, ImGui.GetContentRegionAvail().X);
		if (UiKit.PrimaryButton("gnb_open_settings_window", "打开 LittleDart 设置窗口", new Vector2(x, 34f)))
		{
			GnbWindowSwitcher.ShowSelected();
		}
	}

	internal static void DrawWindowBody()
	{
		UiKit.Surface = UiSurface.Prome;
		GnbSettings instance = GnbSettings.Instance;
		if (instance == null)
		{
			ImGui.TextDisabled("LittleDart 设置尚未初始化，稍后再打开这个窗口");
			return;
		}
		DrawModeSection();
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		DrawTabs();
	}

	private static void DrawTabs()
	{
		if (_tab < 0 || _tab >= PromeTabNames.Length)
		{
			_tab = 0;
		}
		float x = ImGui.GetContentRegionAvail().X;
		float tabWidth = MathF.Max(56f, (x - 8f * (float)(PromeTabNames.Length - 1)) / (float)PromeTabNames.Length);
		for (int i = 0; i < PromeTabNames.Length; i++)
		{
			if (i > 0)
			{
				ImGui.SameLine(0f, 8f);
			}
			if (UiKit.TabButton("gnb_tab_" + PromeTabNames[i], PromeTabNames[i], _tab == i, tabWidth))
			{
				_tab = i;
			}
		}
		ImGui.Spacing();
		if (_tab >= TabNames.Length)
		{
			ThemeSection.Draw(GnbWindowSwitcher.SwitchTo);
			return;
		}
		DrawPage(_tab);
	}

	
	
	
	
	internal static void DrawPage(int index)
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance == null)
		{
			ImGui.TextDisabled("LittleDart 设置尚未初始化，稍后再打开这个页签");
			return;
		}
		bool isDaily = IsDailyMode();
		switch (index)
		{
		case 0:
			if (isDaily)
			{
				DrawDailyBattleSection(instance);
			}
			else
			{
				DrawHardBattleSection(instance);
			}
			ImGui.Spacing();
			
			
			
			DrawAoeSection(instance);
			ImGui.Spacing();
			DrawRangedSection(instance);
			break;
		case 1:
			if (isDaily)
			{
				UiKit.SectionHeader("起手设置", (string)null, false);
			}
			else
			{
				DrawOpenerSection(instance);
			}
			break;
		case 2:
			if (isDaily)
			{
				DrawDailyMitigationSection(instance);
			}
			else
			{
				UiKit.SectionHeader("减伤设置", (string)null, false);
			}
			break;
		case 3:
			DrawCommonSection(instance);
			break;
		default:
			DrawDevSection();
			break;
		}
	}

	internal static void DrawModeSection()
	{
		UiKit.SectionHeader("战斗模式", (string)null, false);
		DrawModeSwitch();
	}

	
	
	
	
	internal static void DrawModeSwitchOnly()
	{
		DrawModeSwitch();
	}

	
	internal static void ToggleMode()
	{
		ApplyMode(IsDailyMode() ? 0 : 1);
	}

	private static void DrawModeSwitch()
	{
		bool daily = IsDailyMode();
		
		if (UiKit.ModeSwitch("###gnb_mode_switch", daily, ref _modeAnim))
		{
			ApplyMode((!daily) ? 1 : 0);
		}
	}

	private static bool IsDailyMode()
	{
		return GnbHelper.IsDailyMode();
	}

	private static void ApplyMode(int mode)
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance == null)
		{
			return;
		}
		instance.Mode = ((mode == 1) ? 1 : 0);
		bool appliedModeIsDaily = instance.Mode == 1;
		instance.LastAppliedMode = instance.Mode;
		
		
		
		if (!GnbRotationEventHandler.ApplyModePreset())
		{
			instance.Save();
			GnbHelper.ToastModeBanner(appliedModeIsDaily);
		}
		GnbHelper.Log("已切换到" + (appliedModeIsDaily ? "日常" : "高难") + "模式");
	}

	private static void DrawHardBattleSection(GnbSettings s)
	{
		UiKit.SectionHeader("战斗设置（高难）", (string)null, true);
	}

	private static void DrawDailyBattleSection(GnbSettings s)
	{
		UiKit.SectionHeader("战斗设置（日常）", (string)null, true);
		ImGui.Spacing();
		GnbSettingWidgets.FieldCheckbox("四人本自动盾姿", ref s.AutoTankStanceIn4Man);
		GnbSettingWidgets.BoundCheckbox("gnb_auto_pull", "自动拉怪", GnbAutoPull.IsEnabled, GnbAutoPull.SetEnabled);
		int selectorIndex = Array.IndexOf(GnbAutoPull.Modes, GnbAutoPull.CurrentMode);
		GnbSettingWidgets.BoundComboRow("gnb_auto_pull_mode", "目标选择器模式", (selectorIndex >= 0) ? selectorIndex : 0, GnbAutoPull.ModeLabels, (int index) =>
		{
			GnbAutoPull.SetSelectorMode(GnbAutoPull.Modes[index]);
		});
		ImGui.Spacing();
		ImGui.TextDisabled(GnbAutoPull.Describe());
		ImGui.Spacing();
		GnbSettingWidgets.IntStepper("突进拉怪数量门槛", ref s.PullMinAggro, 1, 6);
	}

	private static void DrawRangedSection(GnbSettings s)
	{
		UiKit.SectionHeader("远离档", (string)null, true);
		GnbSettingWidgets.QtCheckbox(GnbQt.UseLightningShot, "远离闪雷弹");
		GnbSettingWidgets.IntStepper("闪雷弹距离", ref s.RangedLightningShotMinDistance, 3, 20);
	}

	private static void DrawOpenerSection(GnbSettings s)
	{
		UiKit.SectionHeader("起手设置", (string)null, true);
		GnbSettingWidgets.FieldCheckbox("启用起手", ref s.OpenerEnabled);
		if (s.OpenerEnabled)
		{
			GnbSettingWidgets.ComboRow("起手方案", ref s.OpenerIndex, GunbreakerRotation.OpenerNames);
			GnbSettingWidgets.FieldCheckbox("弹道起手", ref s.TrajectoryOpener);
			GnbSettingWidgets.ComboRow("倒计时开怪技能", ref s.PullAction, PullActionNames);
			GnbSettingWidgets.QtCheckbox(GnbQt.UseSingleTargetOpener, "单体起手");
			GnbSettingWidgets.QtCheckbox(GnbQt.UsePotion, "开场爆发药");
			ImGui.Spacing();
			UiKit.SectionHeader("高难轴开关（默认开）", (string)null, false);
			GnbSettingWidgets.QtCheckbox(GnbQt.BloodfestFirst, "先打血壤连");
		}
	}

	private static void DrawDailyMitigationSection(GnbSettings s)
	{
		UiKit.SectionHeader("减伤设置", (string)null, true);
		GnbSettingWidgets.QtCheckbox(GnbQt.UseAutoMitigation, "自动减伤");
		GnbSettingWidgets.IntStepper("无敌血量", ref s.MitigationInvulnHpPercent, 20, 50);
	}

	private static void DrawCommonSection(GnbSettings s)
	{
		UiKit.SectionHeader("通用", (string)null, false);
		GnbSettingWidgets.FieldCheckbox("切换模式时自动调整 QT", ref s.AutoAdjustQtOnModeSwitch);
		GnbSettingWidgets.FieldCheckbox("使用游戏内提示", ref s.UseToast);
	}

	
	
	
	
	private static void DrawAoeSection(GnbSettings s)
	{
		UiKit.SectionHeader("AOE", (string)null, false);
		
		
		
		GnbSettingWidgets.IntStepper("AOE 敌人数阈值", ref s.AoeEnemyCount, 1, 10);
		GnbSettingWidgets.IntStepper("群体连招敌人数阈值", ref s.AoeComboEnemyCount, 1, 10);
	}


	private static void DrawDevSection()
	{
		UiKit.SectionHeader("开发测试", (string)null, false);
		bool enabled = GnbDiagnostics.Enabled;
		if (UiKit.ToggleRow("gnb_dev_debug", "开发测试", string.Empty, ref enabled))
		{
			GnbDiagnostics.Enabled = enabled;
		}
		ImGui.Spacing();
		
		
		ChangelogSection.Draw();
	}
}
