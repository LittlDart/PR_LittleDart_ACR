using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using ECommons.Logging;
using PromeRotation;
using PromeRotation.Data;
using PromeRotation.UI.Components;
using PromeRotation.UI.QuickToggles;

namespace LittleDart.Common.Ui;

public sealed class LittleDartDeckWindow : Window
{
	
	public const float MinPanelWidth = 320f;

	
	public const float MaxPanelWidth = 460f;

	
	public const float BaselineHeight = 240f;

	
	private const float CollapsedMinimumHeight = 80f;

	
	private const float MaximumHeight = 460f;

	
	private const float PanelInset = 3f;

	private const float TitleHeight = 22f;

	
	
	
	
	private const float HazardHeightExpanded = 36f;

	
	private const float HazardHeightCollapsed = 30f;

	
	private const float SetButtonWidth = 54f;

	
	private const float SetButtonWidthCollapsedMin = 64f;

	private const float SetButtonWidthCollapsedMax = 104f;

	
	private const float GaugeRowHeight = 26f;

	private const float OpenButtonHeight = 34f;

	
	private const int QtGaugeCells = 12;

	
	private const float MicroTracking = 1.1f;

	private readonly DeckContext _context;

	private readonly UiWindowLayout _layout;

	private PromeUiStyleScope? _promeScope;

	private UiShellStyleScope _shellScope;

	
	
	
	
	private ModeSwitchAnim _modeAnim;

	
	private float _measuredHeight = BaselineHeight;

	
	private float _panelWidth = MinPanelWidth;

	private bool _registered;

	
	
	
	
	private static bool OptionsCollapsed
	{
		get
		{
			try
			{
				return InterfaceSettings.Instance.DeckCollapsed;
			}
			catch
			{
				return false;
			}
		}
	}

	
	public LittleDartDeckWindow(DeckContext context)
		: base(BuildWindowName(context),
			ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize |
			ImGuiWindowFlags.NoScrollWithMouse)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
		_layout = new UiWindowLayout(_context.WindowId);
		Size = new Vector2(MinPanelWidth, BaselineHeight);
		SizeCondition = ImGuiCond.Always;
		PositionCondition = ImGuiCond.FirstUseEver;
		
		RespectCloseHotkey = false;
	}

	
	public string WindowId => _context.WindowId;

	
	
	
	
	public new bool IsOpen
	{
		get => base.IsOpen;
		set => base.IsOpen = value;
	}

	
	public void SaveLayoutNow()
	{
		_layout.Flush();
	}

	
	public void Register()
	{
		if (_registered)
		{
			return;
		}
		Plugin? plugin = Plugin.Instance;
		if (plugin is null)
		{
			
			PluginLog.Warning("[LittleDart/界面] PR 插件实例未就绪，主控状态台注册失败：" + WindowId);
			return;
		}
		plugin.WindowSystem.AddWindow(this);
		_registered = true;
		PluginLog.Information("[LittleDart/界面] 主控状态台已注册：" + WindowId + "（" + _context.JobLabel + " / Build " +
			UiBuildInfo.Stamp + "）");
	}

	
	public void Unregister()
	{
		if (!_registered)
		{
			return;
		}
		_registered = false;
		SaveLayoutNow();
		IsOpen = false;
		Plugin? plugin = Plugin.Instance;
		if (plugin is not null)
		{
			plugin.WindowSystem.RemoveWindow(this);
		}
		PluginLog.Information("[LittleDart/界面] 主控状态台已从 PR 窗口系统摘除：" + WindowId);
	}

	public override void PreDraw()
	{
		
		_layout.Apply(this);
		
		
		
		_panelWidth = ComputePanelWidth();
		
		
		
		Size = new Vector2(_panelWidth, _measuredHeight) / UiTheme.UiScale;
		
		_promeScope = UiTheme.PushWindowBase(0.92f);
		_shellScope = UiTheme.PushShellStyle();
		base.PreDraw();
	}

	public override void PostDraw()
	{
		
		_layout.Flush();
		_shellScope.Dispose();
		_shellScope = default;
		_promeScope?.Dispose();
		_promeScope = null;
		base.PostDraw();
	}

	public override void Draw()
	{
		_layout.Track();
		
		
		UiKit.Surface = UiSurface.Shell;
		try
		{
			DrawBackdrop();
			Vector2 home = ImGui.GetCursorPos();
			DrawTitleStrip(home);
			Gap(4f);
			DrawHazardRow();
			if (OptionsCollapsed)
			{
				
				DrawCollapsedFooter();
			}
			else
			{
				
				Gap(4f);
				DrawInfoBand();
				DrawSectionDivider();
				Gap(4f);
				
				
				
				if (_context.IsDailyMode())
				{
					DrawAutoAttackRow();
					Gap(3f);
				}
				DrawModeRow();
				Gap(4f);
				DrawFooterRow();
				Gap(2f);
				DrawOpenConfigRow();
			}
			UpdateMeasuredHeight();
		}
		catch (Exception exception)
		{
			PluginLog.Error("[LittleDart/界面] 主控状态台绘制失败（" + WindowId + "）：" + exception);
			ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Danger);
			ImGui.TextWrapped("主控状态台绘制出错：" + exception.Message);
			ImGui.PopStyleColor();
		}
		finally
		{
			UiKit.Surface = UiSurface.Prome;
		}
	}

	
	
	
	
	private void DrawBackdrop()
	{
		Vector2 windowPos = ImGui.GetWindowPos();
		Vector2 panelMin = windowPos + new Vector2(PanelInset, PanelInset);
		Vector2 panelMax = windowPos + ImGui.GetWindowSize() - new Vector2(PanelInset, PanelInset);
		float cut = UiTheme.CutLarge;
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();

		CyberDraw.Glow(drawList, panelMin, panelMax, UiTheme.Glow, cut, CutCorners.Diagonal, 3f, 0.9f);
		CyberDraw.Panel(drawList, panelMin, panelMax, UiTheme.Alpha(UiTheme.Layout, 0.97f), UiTheme.Border, cut,
			CutCorners.Diagonal);
		
		CyberDraw.Brackets(drawList, panelMin + new Vector2(2f, 2f), panelMax - new Vector2(2f, 2f),
			UiTheme.Alpha(UiTheme.Accent, 0.50f), 9f, 1.2f);
		CyberDraw.Ticks(drawList, new Vector2(panelMin.X + 12f, panelMax.Y - 10f),
			new Vector2(panelMax.X - 12f, panelMax.Y - 6f), 21, UiTheme.Alpha(UiTheme.Accent, 0.28f), 4f, 0);
	}

	
	
	
	
	
	
	
	
	
	
	
	private void DrawTitleStrip(Vector2 home)
	{
		float contentWidth = ImGui.GetContentRegionAvail().X;
		Vector2 origin = ImGui.GetCursorScreenPos();

		
		ImGui.InvisibleButton(WindowId + "_drag", new Vector2(MathF.Max(80f, contentWidth), TitleHeight));
		if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left, 0f))
		{
			ImGui.SetWindowPos(ImGui.GetWindowPos() + ImGui.GetIO().MouseDelta);
		}

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 stripMax = new Vector2(origin.X + contentWidth, origin.Y + TitleHeight);
		CyberDraw.Fill(drawList, origin, stripMax, UiTheme.Alpha(UiTheme.SurfaceRaised, 0.85f), UiTheme.CutSmall,
			CutCorners.Diagonal);
		
		CyberDraw.Fill(drawList, new Vector2(origin.X + 5f, origin.Y + 5f),
			new Vector2(origin.X + 8f, origin.Y + TitleHeight - 5f), UiTheme.Accent, 0f, CutCorners.None);

		Vector2 plateSize = CyberDraw.Measure("// CONTROL DECK", MicroTracking);
		float textY = origin.Y + (TitleHeight - plateSize.Y) * 0.5f;
		CyberDraw.Text(drawList, new Vector2(origin.X + 14f, textY), "// CONTROL DECK", UiTheme.TextMuted,
			MicroTracking);

		
		Vector2 zoneSize = CyberDraw.Measure("ZONE: DANGER", MicroTracking);
		CyberDraw.Text(drawList, new Vector2(stripMax.X - zoneSize.X, origin.Y + (TitleHeight - zoneSize.Y) * 0.5f),
			"ZONE: DANGER", UiTheme.Alpha(UiTheme.Danger, 0.65f), MicroTracking);

		
		ImGui.SetCursorPos(new Vector2(home.X, home.Y + TitleHeight));
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	private void DrawHazardRow()
	{
		float contentWidth = ImGui.GetContentRegionAvail().X;
		AcrState state = ReadAcrState();
		string label;
		string code;
		Vector4 color;
		switch (state)
		{
			case AcrState.On:
				label = "运行中";
				code = "ARMED";
				color = UiTheme.Danger;
				break;
			case AcrState.Hold:
				label = "停手中";
				code = "HOLD";
				color = UiTheme.Warning;
				break;
			default:
				label = "已关闭";
				code = "OFFLINE";
				color = UiTheme.Dim;
				break;
		}

		float barHeight = OptionsCollapsed ? HazardHeightCollapsed : HazardHeightExpanded;
		
		
		float setWidth = OptionsCollapsed
			? Math.Clamp(contentWidth * 0.30f, SetButtonWidthCollapsedMin, SetButtonWidthCollapsedMax)
			: SetButtonWidth;
		float barWidth = MathF.Max(80f, contentWidth - setWidth - 6f);
		bool leftClick = ShellWidgets.HazardBar(WindowId + "_hazard", label, code, color, new Vector2(barWidth, barHeight));
		
		bool rightClick = ImGui.IsItemClicked(ImGuiMouseButton.Right);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
			ImGui.SetTooltip("左键：运行 / 关闭　·　右键：运行 / 停手");
		}
		if (leftClick)
		{
			ApplyAcrState(state == AcrState.Off ? AcrState.On : AcrState.Off);
		}
		else if (rightClick)
		{
			switch (state)
			{
				case AcrState.On:
					ApplyAcrState(AcrState.Hold);
					break;
				case AcrState.Hold:
					ApplyAcrState(AcrState.On);
					break;
			}
		}

		ImGui.SameLine(0f, 6f);
		
		bool setClicked = ShellWidgets.IconButton(WindowId + "_set", "设置", new Vector2(setWidth, barHeight));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(OptionsCollapsed ? "展开下方选项（主动攻击 / 战斗模式）" : "收起下方选项（只留运行状态）");
		}
		if (setClicked)
		{
			ToggleCollapsed();
		}
	}

	
	private static void ToggleCollapsed()
	{
		try
		{
			InterfaceSettings settings = InterfaceSettings.Instance;
			settings.DeckCollapsed = !settings.DeckCollapsed;
			settings.Save();
		}
		catch (Exception exception)
		{
			PluginLog.Warning("[LittleDart/界面] 切换主控台折叠状态失败：" + exception.Message);
		}
	}

	
	
	
	
	private static void ApplyAcrState(AcrState state)
	{
		try
		{
			PromeSettings.Instance.EnableAcr = state;
			PluginLog.Information("[LittleDart/界面] ACR 状态切换为 " + state + "（主控状态台「运行中」条）");
		}
		catch (Exception exception)
		{
			PluginLog.Warning("[LittleDart/界面] 切换 ACR 状态失败：" + exception.Message);
		}
	}

	
	
	
	
	
	
	
	private void DrawInfoBand()
	{
		float contentWidth = ImGui.GetContentRegionAvail().X;
		float lineHeight = ImGui.GetTextLineHeight();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();

		
		
		Vector2 origin = ImGui.GetCursorScreenPos();
		string jobText = "JT: " + _context.JobShort;
		float jobWidth = CyberDraw.Measure(jobText, MicroTracking).X;
		const string buildPrefix = "// BUILD ";
		float stampBudget = MathF.Max(60f, contentWidth - jobWidth - 12f - CyberDraw.Measure(buildPrefix, 0f).X);
		string stamp = buildPrefix + FitBuildStamp(UiBuildInfo.Stamp, stampBudget);
		Vector2 stampSize = CyberDraw.Measure(stamp, 0f);
		CyberDraw.Text(drawList, origin, jobText, UiTheme.Micro, MicroTracking);
		CyberDraw.Text(drawList, new Vector2(origin.X + contentWidth - stampSize.X, origin.Y), stamp, UiTheme.Micro,
			0f);
		ImGui.Dummy(new Vector2(contentWidth, lineHeight));
		Gap(2f);

		
		
		origin = ImGui.GetCursorScreenPos();
		string jobLabel = _context.JobLabel;
		const string qtLabel = "QT ACTIVE";
		Vector2 jobSize = CyberDraw.Measure(jobLabel, 0.6f);
		Vector2 qtSize = CyberDraw.Measure(qtLabel, 0.8f);
		CyberDraw.Text(drawList, origin, jobLabel, UiTheme.TextMuted, 0.6f);
		CyberDraw.TextRight(drawList, origin, new Vector2(origin.X + contentWidth, origin.Y + lineHeight), qtLabel,
			UiTheme.Micro, 0f, 0.8f);
		float ruleLeft = origin.X + jobSize.X + 10f;
		float ruleRight = origin.X + contentWidth - qtSize.X - 8f;
		if (ruleRight > ruleLeft)
		{
			CyberDraw.Rule(drawList, new Vector2(ruleLeft, origin.Y + lineHeight * 0.5f),
				new Vector2(ruleRight, origin.Y + lineHeight * 0.5f), UiTheme.Alpha(UiTheme.Hairline, 0.55f), 1f);
		}
		ImGui.Dummy(new Vector2(contentWidth, lineHeight));
		Gap(3f);

		
		ReadQtCounts(out int active, out int total);
		float ratio = total <= 0 ? 0f : (float)active / total;
		origin = ImGui.GetCursorScreenPos();
		float counterWidth = 68f;
		float barWidth = MathF.Max(48f, contentWidth - counterWidth - 8f);
		float barTop = origin.Y + (GaugeRowHeight - 10f) * 0.5f;
		
		CyberDraw.Segments(drawList, new Vector2(origin.X, barTop), new Vector2(origin.X + barWidth, barTop + 10f),
			QtGaugeCells, ratio, UiTheme.Accent, UiTheme.Dim, UiTheme.CutSmall, 2f, ratio > 0f && ratio < 1f);

		ImFontPtr font = ImGui.GetFont();
		float bigSize = ImGui.GetFontSize() * 1.45f;
		string counter = active.ToString("D2") + "/" + total.ToString("D2");
		float counterWidthText = ImGui.CalcTextSize(counter).X * 1.45f;
		drawList.AddText(font, bigSize, new Vector2(origin.X + contentWidth - counterWidthText, origin.Y - 1f),
			UiTheme.ToU32(UiTheme.Accent), counter);
		ImGui.Dummy(new Vector2(contentWidth, GaugeRowHeight));
	}

	
	
	
	
	
	private void DrawSectionDivider()
	{
		float width = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
		Vector2 origin = ImGui.GetCursorScreenPos();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		bool collapsed = OptionsCollapsed;
		string code = collapsed ? "[ OPTIONS HIDDEN ]" : "[ OPTIONS ]";
		Vector2 codeSize = CyberDraw.Measure(code, 1.2f);
		CyberDraw.Micro(drawList, new Vector2(origin.X + 2f, origin.Y + 1f), "// OPTION BUS", 1.2f);
		CyberDraw.Text(drawList, new Vector2(origin.X + width - codeSize.X, origin.Y + 1f), code,
			collapsed ? UiTheme.Alpha(UiTheme.Micro, 0.95f) : UiTheme.Alpha(UiTheme.Accent, 0.90f), 1.2f);

		Vector4 lineColor = collapsed ? UiTheme.Alpha(UiTheme.Border, 0.90f) : UiTheme.Alpha(UiTheme.Accent, 0.55f);
		CyberDraw.Fill(drawList, new Vector2(origin.X, origin.Y + 15f),
			new Vector2(origin.X + width, origin.Y + 17f), lineColor);
		CyberDraw.Fill(drawList, new Vector2(origin.X, origin.Y + 14f),
			new Vector2(origin.X + 3f, origin.Y + 18f), lineColor);
		CyberDraw.Fill(drawList, new Vector2(origin.X + width - 3f, origin.Y + 14f),
			new Vector2(origin.X + width, origin.Y + 18f), lineColor);
		ImGui.Dummy(new Vector2(width, 19f));
	}

	
	
	
	
	
	private void DrawCollapsedFooter()
	{
		float width = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
		Vector2 origin = ImGui.GetCursorScreenPos();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		const string code = "[ OPTIONS HIDDEN ]";
		Vector2 codeSize = CyberDraw.Measure(code, 1.2f);
		CyberDraw.Text(drawList, new Vector2(origin.X + width - codeSize.X, origin.Y + 2f), code,
			UiTheme.Alpha(UiTheme.Micro, 0.95f), 1.2f);
		CyberDraw.Fill(drawList, new Vector2(origin.X, origin.Y + 18f),
			new Vector2(origin.X + width, origin.Y + 20f), UiTheme.Alpha(UiTheme.Accent, 0.45f));
		ImGui.Dummy(new Vector2(width, 22f));
	}

	
	private void DrawAutoAttackRow()
	{
		bool value = _context.AutoPullEnabled();
		if (UiKit.Checkbox(WindowId + "_autopull", "主动攻击 / AUTO ATTACK", string.Empty, ref value))
		{
			try
			{
				_context.SetAutoPull(value);
			}
			catch (Exception exception)
			{
				PluginLog.Warning("[LittleDart/界面] 写「主动攻击」失败：" + exception.Message);
			}
		}
	}

	
	private void DrawModeRow()
	{
		bool daily = _context.IsDailyMode();
		if (UiKit.ModeSwitch(WindowId + "_mode", daily, ref _modeAnim))
		{
			try
			{
				_context.ToggleMode();
			}
			catch (Exception exception)
			{
				PluginLog.Warning("[LittleDart/界面] 切换战斗模式失败：" + exception.Message);
			}
		}
	}

	
	
	
	
	private void DrawFooterRow()
	{
		float contentWidth = ImGui.GetContentRegionAvail().X;
		float lineHeight = ImGui.GetTextLineHeight();
		Vector2 origin = ImGui.GetCursorScreenPos();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();

		
		
		const string hardware = "HW: PWR 98%";
		const string access = "[ CONFIG ACCESS ]";
		Vector2 hardwareSize = CyberDraw.Measure(hardware, 0.4f);
		Vector2 accessSize = CyberDraw.Measure(access, MicroTracking);
		float midY = origin.Y + lineHeight * 0.5f;
		CyberDraw.Hexagon(drawList, new Vector2(origin.X + 5f, midY), 5f, UiTheme.Alpha(UiTheme.Accent, 0.18f),
			UiTheme.Alpha(UiTheme.Accent, 0.70f), 1.2f);
		CyberDraw.Text(drawList, new Vector2(origin.X + 15f, midY - hardwareSize.Y * 0.5f), hardware, UiTheme.Micro,
			0.4f);
		CyberDraw.TextRight(drawList, origin, new Vector2(origin.X + contentWidth, origin.Y + lineHeight), access,
			UiTheme.Micro, 0f, MicroTracking);
		
		float ruleLeft = origin.X + 15f + hardwareSize.X + 8f;
		float ruleRight = origin.X + contentWidth - accessSize.X - 8f;
		if (ruleRight > ruleLeft)
		{
			CyberDraw.Rule(drawList, new Vector2(ruleLeft, midY), new Vector2(ruleRight, midY), UiTheme.Hairline, 1f);
		}
		ImGui.Dummy(new Vector2(contentWidth, lineHeight));
	}

	
	private void DrawOpenConfigRow()
	{
		float contentWidth = ImGui.GetContentRegionAvail().X;
		if (ShellWidgets.NumberedButton(WindowId + "_open", "02", "OPEN LITTLEDART CONFIG",
			new Vector2(contentWidth, OpenButtonHeight)))
		{
			OpenSettings();
		}
	}

	
	
	
	
	
	
	private void UpdateMeasuredHeight()
	{
		
		
		
		float minimum = UiTheme.Px(OptionsCollapsed ? CollapsedMinimumHeight : BaselineHeight);
		float used = ImGui.GetCursorPosY() + UiTheme.WindowPadding.Y + UiTheme.Px(2f);
		_measuredHeight = Math.Clamp(MathF.Ceiling(used), minimum, UiTheme.Px(MaximumHeight));
	}

	
	
	
	
	
	private float ComputePanelWidth()
	{
		float padding = UiTheme.WindowPadding.X * 2f + 8f;
		float widest = 0f;

		void Consider(string? text, float tracking, float extra)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			Vector2 size = ImGui.CalcTextSize(text!);
			float width = size.X + tracking * MathF.Max(0, text!.Length - 1) + extra;
			widest = MathF.Max(widest, width);
		}

		
		if (!OptionsCollapsed)
		{
			
			if (_context.IsDailyMode())
			{
				Consider("主动攻击 / AUTO ATTACK", 0f, 18f + 8f);
			}
			
			Consider("OPEN LITTLEDART CONFIG", 1.4f, 26f + 10f);
			
			Consider("[ MODE: DAILY ]", 1.3f, 132f + 12f + 12f);
		}
		
		Consider("运行中", 1.2f, 26f + 60f + 52f);
		Consider("OFFLINE", 1.4f, 26f + 60f + 52f);
		
		Consider("JT: " + _context.JobShort, 1.1f, CyberDraw.Measure("// BUILD 2026-10-24 20:00:00", 0f).X + 24f);
		Consider(_context.JobLabel, 0.6f, CyberDraw.Measure("QT ACTIVE", 0.8f).X + 150f);

		
		
		return Math.Clamp(widest + padding, UiTheme.Px(MinPanelWidth), UiTheme.Px(MaxPanelWidth));
	}

	
	private static AcrState ReadAcrState()
	{
		try
		{
			return PromeSettings.Instance.EnableAcr;
		}
		catch (Exception exception)
		{
			PluginLog.Warning("[LittleDart/界面] 读取 ACR 状态失败：" + exception.Message);
			return AcrState.On;
		}
	}

	
	
	
	
	
	
	private static void ReadQtCounts(out int active, out int total)
	{
		active = 0;
		total = 1;
		try
		{
			IReadOnlyList<QtDefinition> definitions = QtRegistry.ResolveVisible(
				PromeSettings.Instance.QuickToggles.Keys, PromeSettings.Instance.HiddenQts);
			int on = 0;
			foreach (QtDefinition definition in definitions)
			{
				if (PromeSettings.Instance.GetQt(definition.Id))
				{
					on++;
				}
			}
			total = Math.Max(1, definitions.Count);
			active = Math.Min(on, total);
		}
		catch (Exception exception)
		{
			
			PluginLog.Warning("[LittleDart/界面] 读取 QT 计数失败：" + exception.Message);
		}
	}

	
	private static string FitBuildStamp(string stamp, float maxWidth)
	{
		if (string.IsNullOrWhiteSpace(stamp))
		{
			return "未知";
		}
		if (CyberDraw.Measure(stamp, 0f).X <= maxWidth)
		{
			return stamp;
		}
		const int secondsLength = 19;
		return stamp.Length > secondsLength ? stamp.Substring(0, secondsLength) : stamp;
	}

	
	private void OpenSettings()
	{
		try
		{
			_context.OpenSettings();
		}
		catch (Exception exception)
		{
			PluginLog.Warning("[LittleDart/界面] 打开 LittleDart 设置窗口失败：" + exception.Message);
		}
	}

	
	private static void Gap(float height)
	{
		ImGui.Dummy(new Vector2(1f, height));
	}

	
	
	
	
	
	private static string BuildWindowName(DeckContext? context)
	{
		string title = string.IsNullOrWhiteSpace(context?.Title) ? "LITTLEDART // CONTROL DECK" : context!.Title!;
		string id = string.IsNullOrWhiteSpace(context?.WindowId) ? "littledart_deck" : context!.WindowId!;
		return title + "###" + id;
	}
}
