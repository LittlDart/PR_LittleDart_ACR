using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using LittleDart.Common.Ui;
using LittleDart.Gunbreaker.Mitigation;
using PromeRotation.UI.Components;

namespace LittleDart.Gunbreaker.Setting;

public class GnbSettingUi
{
	public static readonly GnbSettingUi Instance = new GnbSettingUi();

	// 模式开关的配色与绘制已抽到 Common\Ui（ThemeColors / ThemeModeSwitch，docs\39 §6 批次 E）。
	// 动画状态仍按职业各持一份 —— 与 PLD 共用一份会让两边旋钮一起动。
	private static ModeSwitchAnim _modeAnim;

	private static readonly string[] TabNames = new string[5] { "战斗", "起手", "减伤", "通用", "开发" };

	private static int _tab;

	private static readonly string[] PullActionNames = new string[3] { "弹道（默认）", "闪雷弹", "不主动开怪" };

	public static void Draw()
	{
		if (GnbSettings.Instance == null)
		{
			ImGui.TextDisabled("LittleDart 设置尚未初始化，稍后再打开这个页签");
			return;
		}
		DrawModeSection();
		ImGui.Spacing();
		float x = MathF.Max(120f, ImGui.GetContentRegionAvail().X);
		if (PromeUi.PrimaryButton("gnb_open_settings_window", "打开 LittleDart 绝枪战士设置窗口", new Vector2(x, 34f)))
		{
			GnbSettingWindow.ToggleWindow();
		}
	}

	internal static void DrawWindowBody()
	{
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
		DrawTabs(instance);
	}

	private static void DrawTabs(GnbSettings s)
	{
		bool isDaily = IsDailyMode();
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
			if (PromeUi.TabButton("gnb_tab_" + TabNames[i], TabNames[i], _tab == i, tabWidth))
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
			DrawRangedSection(s);
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
		bool daily = IsDailyMode();
		if (ThemeModeSwitch.Draw("###gnb_mode_switch", daily, ref _modeAnim))
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
		// ★ 2026-10-19「骑士同款切模式提示」：QT 预设 / 存盘 / 游戏内提示统一走 ApplyModePreset
		//   （与 PldSettingUi.ApplyMode 同一条管道）。玩家把「切换模式时自动调整 QT」关掉时它返回 false
		//   —— 那种情况下 QT 不动，但仍要给一条反馈，否则点了开关像是没反应。
		if (!GnbRotationEventHandler.ApplyModePreset())
		{
			instance.Save();
			GnbHelper.ToastModeBanner(appliedModeIsDaily);
		}
		GnbHelper.Log("已切换到" + (appliedModeIsDaily ? "日常" : "高难") + "模式");
	}

	private static void DrawHardBattleSection(GnbSettings s)
	{
		PromeUi.SectionHeader("战斗设置（高难）", (string)null, true);
	}

	private static void DrawDailyBattleSection(GnbSettings s)
	{
		PromeUi.SectionHeader("战斗设置（日常）", (string)null, true);
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
		PromeUi.SectionHeader("远离档", (string)null, true);
		GnbSettingWidgets.QtCheckbox(GnbQt.UseLightningShot, "远离闪雷弹");
		GnbSettingWidgets.IntStepper("闪雷弹距离", ref s.RangedLightningShotMinDistance, 3, 20);
	}

	private static void DrawOpenerSection(GnbSettings s)
	{
		PromeUi.SectionHeader("起手设置", (string)null, true);
		GnbSettingWidgets.FieldCheckbox("启用起手", ref s.OpenerEnabled);
		if (s.OpenerEnabled)
		{
			GnbSettingWidgets.ComboRow("起手方案", ref s.OpenerIndex, GunbreakerRotation.OpenerNames);
			GnbSettingWidgets.FieldCheckbox("弹道起手", ref s.TrajectoryOpener);
			GnbSettingWidgets.ComboRow("倒计时开怪技能", ref s.PullAction, PullActionNames);
			GnbSettingWidgets.QtCheckbox(GnbQt.UseSingleTargetOpener, "单体起手");
			GnbSettingWidgets.QtCheckbox(GnbQt.UsePotion, "开场爆发药");
			ImGui.Spacing();
			PromeUi.SectionHeader("高难轴开关（默认开）", (string)null, false);
			GnbSettingWidgets.QtCheckbox(GnbQt.BloodfestFirst, "先打血壤连");
		}
	}

	private static void DrawDailyMitigationSection(GnbSettings s)
	{
		PromeUi.SectionHeader("减伤设置", (string)null, true);
		GnbSettingWidgets.QtCheckbox(GnbQt.UseAutoMitigation, "自动减伤");
		GnbSettingWidgets.IntStepper("无敌血量", ref s.MitigationInvulnHpPercent, 20, 50);
	}

	private static void DrawCommonSection(GnbSettings s)
	{
		PromeUi.SectionHeader("通用", (string)null, false);
		GnbSettingWidgets.FieldCheckbox("切换模式时自动调整 QT", ref s.AutoAdjustQtOnModeSwitch);
		GnbSettingWidgets.FieldCheckbox("使用游戏内提示", ref s.UseToast);
		ImGui.Spacing();
		DrawAoeSection(s);
	}

	private static void DrawAoeSection(GnbSettings s)
	{
		PromeUi.SectionHeader("AOE", (string)null, false);
		// ★ 2026-10-19：两处 AOE 阈值从 IntSlider 换成 IntStepper（可加减的数字）——
		//   与「战斗 / 减伤」页那三处（docs\39 §4 批次 C / commit ce43f09）统一成一种控件。
		//   范围与字段不变（1..10，写死的是 UI 的上下限，不是判据）。
		GnbSettingWidgets.IntStepper("AOE 敌人数阈值", ref s.AoeEnemyCount, 1, 10);
		GnbSettingWidgets.IntStepper("群体连招敌人数阈值", ref s.AoeComboEnemyCount, 1, 10);
	}


	private static void DrawDevSection()
	{
		PromeUi.SectionHeader("开发测试", (string)null, false);
		bool enabled = GnbDiagnostics.Enabled;
		if (PromeUi.ToggleRow("gnb_dev_debug", "开发测试", string.Empty, ref enabled))
		{
			GnbDiagnostics.Enabled = enabled;
		}
		ImGui.Spacing();
		ImGui.TextUnformatted("目标形态：" + GnbHelper.DescribeTargetProfile());
		ImGui.Spacing();
		if (ImGui.CollapsingHeader("减伤调度器状态（诊断）###gnb_mit_status", (ImGuiTreeNodeFlags)0))
		{
			PromeUi.StatusPill("gnb_mit_active", "生效中：" + GnbMitigationScheduler.Instance.DescribeActiveMits(), PromeUiTheme.Success);
			ImGui.Spacing();
			ImGui.TextUnformatted(GnbMitigationScheduler.Instance.DescribeStatus());
		}
	}
}
