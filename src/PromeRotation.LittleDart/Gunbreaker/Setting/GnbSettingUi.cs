using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using LittleDart.Gunbreaker.Mitigation;
using PromeRotation.UI.Components;

namespace LittleDart.Gunbreaker.Setting;

public class GnbSettingUi
{
	public static readonly GnbSettingUi Instance = new GnbSettingUi();

	private static readonly Vector4 DailyTrack = new Vector4(145f / 255f, 211f / 255f, 60f / 255f, 0.95f);

	private static readonly Vector4 HardTrack = new Vector4(214f / 255f, 69f / 255f, 69f / 255f, 0.95f);

	private static readonly Vector4 SwitchKnob = new Vector4(0.97f, 0.97f, 0.98f, 1f);

	private static readonly Vector4 KnobShadow = new Vector4(0f, 0f, 0f, 0.28f);

	private static readonly Vector4 DailyTextFrom = new Vector4(74f / 255f, 140f / 255f, 16f / 255f, 1f);

	private static readonly Vector4 DailyTextTo = new Vector4(168f / 255f, 226f / 255f, 84f / 255f, 1f);

	private static readonly Vector4 HardTextFrom = new Vector4(150f / 255f, 28f / 255f, 28f / 255f, 1f);

	private static readonly Vector4 HardTextTo = new Vector4(246f / 255f, 126f / 255f, 126f / 255f, 1f);

	private static float _modeKnobT;

	private static bool _modeKnobInitialized;

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
		if (!_modeKnobInitialized)
		{
			_modeKnobT = (daily ? 1f : 0f);
			_modeKnobInitialized = true;
		}
		float fontSize = ImGui.GetFontSize();
		string modeLabel = (daily ? "日常模式" : "高难模式");
		float x = ImGui.CalcTextSize(modeLabel, false, -1f).X;
		float knobPadding = fontSize * 0.55f;
		float trackHeight = fontSize * 2f;
		float trackInset = trackHeight * 0.1f;
		float knobHeight = trackHeight - trackInset * 2f;
		float knobWidth = x + knobPadding * 2f;
		float controlWidth = knobWidth * 2f + trackInset * 3f;
		float x2 = ImGui.GetContentRegionAvail().X;
		if (controlWidth > x2 && controlWidth > 0f)
		{
			float widthScale = x2 / controlWidth;
			controlWidth *= widthScale;
			knobWidth = MathF.Max(knobHeight, knobWidth * widthScale);
		}
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (x2 - controlWidth) * 0.5f);
		ImGui.InvisibleButton("###gnb_mode_switch", new Vector2(controlWidth, trackHeight), (ImGuiButtonFlags)0);
		bool clicked = ImGui.IsItemClicked();
		float knobTarget = (daily ? 1f : 0f);
		ImGuiIOPtr iO = ImGui.GetIO();
		float knobLerpSpeed = MathF.Min(1f, iO.DeltaTime * 14f);
		_modeKnobT += (knobTarget - _modeKnobT) * knobLerpSpeed;
		if (MathF.Abs(knobTarget - _modeKnobT) < 0.001f)
		{
			_modeKnobT = knobTarget;
		}
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 vector = new Vector2(itemRectMin.X + controlWidth, itemRectMin.Y + trackHeight);
		float trackRound = trackHeight * 0.5f;
		Vector4 c = (daily ? DailyTrack : HardTrack);
		if (ImGui.IsItemHovered())
		{
			c = new Vector4(MathF.Min(1f, c.X + 0.06f), MathF.Min(1f, c.Y + 0.06f), MathF.Min(1f, c.Z + 0.06f), c.W);
		}
		windowDrawList.AddRectFilled(itemRectMin, vector, ToU32(c), trackRound);
		windowDrawList.AddRect(itemRectMin, vector, ToU32(new Vector4(0f, 0f, 0f, 0.18f)), trackRound);
		float knobTrackLeft = itemRectMin.X + trackInset;
		float knobTrackRight = vector.X - trackInset - knobWidth;
		float knobX = knobTrackLeft + (knobTrackRight - knobTrackLeft) * _modeKnobT;
		Vector2 vector2 = new Vector2(knobX, itemRectMin.Y + trackInset);
		Vector2 vector3 = new Vector2(knobX + knobWidth, vector2.Y + knobHeight);
		float knobRound = knobHeight * 0.5f;
		windowDrawList.AddRectFilled(new Vector2(vector2.X, vector2.Y + 1f), new Vector2(vector3.X, vector3.Y + 1f), ToU32(KnobShadow), knobRound);
		windowDrawList.AddRectFilled(vector2, vector3, ToU32(SwitchKnob), knobRound);
		Vector2 vector4 = new Vector2(vector2.X + (knobWidth - x) * 0.5f, vector2.Y + (knobHeight - fontSize) * 0.5f);
		Vector4 textColorFrom = (daily ? DailyTextFrom : HardTextFrom);
		Vector4 textColorTo = (daily ? DailyTextTo : HardTextTo);
		float textCursorX = 0f;
		for (int i = 0; i < modeLabel.Length; i++)
		{
			string charText = modeLabel[i].ToString();
			float amount = ((modeLabel.Length <= 1) ? 0f : ((float)i / (float)(modeLabel.Length - 1)));
			windowDrawList.AddText(new Vector2(vector4.X + textCursorX, vector4.Y), ToU32(Vector4.Lerp(textColorFrom, textColorTo, amount)), charText);
			textCursorX += ImGui.CalcTextSize(charText, false, -1f).X;
		}
		ImGui.Dummy(new Vector2(0f, 6f));
		if (clicked)
		{
			ApplyMode((!daily) ? 1 : 0);
		}
	}

	private static uint ToU32(Vector4 c)
	{
		byte b = (byte)(Math.Clamp(c.X, 0f, 1f) * 255f);
		byte b2 = (byte)(Math.Clamp(c.Y, 0f, 1f) * 255f);
		byte b3 = (byte)(Math.Clamp(c.Z, 0f, 1f) * 255f);
		return (uint)(((byte)(Math.Clamp(c.W, 0f, 1f) * 255f) << 24) | (b3 << 16) | (b2 << 8) | b);
	}

	private static bool IsDailyMode()
	{
		return GnbHelper.IsDailyMode();
	}

	private static void ApplyMode(int mode)
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance != null)
		{
			bool appliedModeIsDaily = (byte)(instance.Mode = ((mode == 1) ? 1 : 0)) != 0;
			GnbQt.ApplyModePreset(appliedModeIsDaily);
			instance.LastAppliedMode = instance.Mode;
			instance.Save();
			GnbHelper.Log("已切换到" + (appliedModeIsDaily ? "日常" : "高难") + "模式");
		}
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
		GnbSettingWidgets.IntSlider("突进拉怪数量门槛", ref s.PullMinAggro, 1, 6);
	}

	private static void DrawRangedSection(GnbSettings s)
	{
		PromeUi.SectionHeader("远离档", (string)null, true);
		GnbSettingWidgets.QtCheckbox("远离闪雷弹", "远离闪雷弹");
		GnbSettingWidgets.IntSlider("闪雷弹距离", ref s.RangedLightningShotMinDistance, 3, 20);
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
			GnbSettingWidgets.QtCheckbox("单体起手", "单体起手");
			GnbSettingWidgets.QtCheckbox("开场爆发药", "开场爆发药");
			ImGui.Spacing();
			PromeUi.SectionHeader("高难轴开关（默认开）", (string)null, false);
			GnbSettingWidgets.QtCheckbox("先打血壤连", "先打血壤连");
			GnbSettingWidgets.QtCheckbox("续剑进无情", "续剑进无情");
		}
	}

	private static void DrawDailyMitigationSection(GnbSettings s)
	{
		PromeUi.SectionHeader("减伤设置", (string)null, true);
		GnbSettingWidgets.QtCheckbox("自动减伤", "自动减伤");
		GnbSettingWidgets.IntSlider("无敌血量", ref s.MitigationInvulnHpPercent, 20, 50);
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
		GnbSettingWidgets.IntSlider("AOE 敌人数阈值", ref s.AoeEnemyCount, 1, 10);
		GnbSettingWidgets.IntSlider("群体连招敌人数阈值", ref s.AoeComboEnemyCount, 1, 10);
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
