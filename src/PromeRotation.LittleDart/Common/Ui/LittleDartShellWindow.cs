using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using ECommons.Logging;
using PromeRotation.Data;
using PromeRotation.UI.Components;

namespace LittleDart.Common.Ui;

public abstract class LittleDartShellWindow : Window
{
	private const float TitleHeight = 44f;

	private const float ModeHeight = 58f;

	private const float TabHeight = 36f;

	private const float GutterHeight = 16f;

	private const float StatusHeight = 26f;

	private const float IconSize = 24f;

	private readonly string _windowId;

	private readonly UiWindowLayout _layout;

	private PromeUiStyleScope? _promeScope;

	private UiShellStyleScope _shellScope;

	private int _page;

	protected LittleDartShellWindow(string windowId, string title)
		: base(title + "###" + windowId, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse)
	{
		_windowId = windowId;
		_layout = new UiWindowLayout(windowId);
		WindowSizeConstraints constraints = new WindowSizeConstraints();
		constraints.MinimumSize = new Vector2(660f, 440f);
		constraints.MaximumSize = new Vector2(1600f, 1600f);
		SizeConstraints = constraints;
		Size = new Vector2(880f, 620f);
		SizeCondition = ImGuiCond.FirstUseEver;
		PositionCondition = ImGuiCond.FirstUseEver;
		RespectCloseHotkey = true;
	}

	
	protected abstract string JobLabel { get; }

	
	protected abstract string JobShort { get; }

	
	protected abstract IReadOnlyList<string> PageNames { get; }

	
	protected abstract bool IsDailyMode { get; }

	
	protected abstract void DrawPageContent(int pageIndex);

	
	protected abstract void DrawModeStrip();

	
	public void SaveLayoutNow()
	{
		_layout.Flush();
	}

	public override void PreDraw()
	{
		_promeScope = UiTheme.PushWindowBase(1f);
		_shellScope = UiTheme.PushShellStyle();
		_layout.Apply(this);
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
			DrawWindowFrame();
			DrawTitleBar();
			DrawModeDeck();
			DrawTabStrip();
			DrawGutter();
			DrawBody();
			DrawStatusBar();
		}
		catch (Exception exception)
		{
			PluginLog.Error("[LittleDart/界面] 自建窗口绘制失败：" + exception);
			ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Danger);
			ImGui.TextWrapped("自建窗口绘制出错：" + exception.Message);
			ImGui.PopStyleColor();
		}
		finally
		{
			UiKit.Surface = UiSurface.Prome;
		}
	}

	
	private void DrawWindowFrame()
	{
		Vector2 min = ImGui.GetWindowPos() + new Vector2(1f, 1f);
		Vector2 max = ImGui.GetWindowPos() + ImGui.GetWindowSize() - new Vector2(1f, 1f);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		CyberDraw.Line(drawList, min, max, UiTheme.Border, UiTheme.CutLarge, CutCorners.Diagonal, 1f);
		CyberDraw.Line(drawList, min + new Vector2(3f, 3f), max - new Vector2(3f, 3f),
			UiTheme.Alpha(UiTheme.Border, 0.35f), UiTheme.CutLarge, CutCorners.Diagonal, 1f);
		CyberDraw.Brackets(drawList, min + new Vector2(6f, 6f), max - new Vector2(6f, 6f),
			UiTheme.Alpha(UiTheme.Accent, 0.55f), 10f, 1.2f);
	}

	private void DrawTitleBar()
	{
		float available = ImGui.GetContentRegionAvail().X;
		Vector2 origin = ImGui.GetCursorScreenPos();
		Vector2 barMax = new Vector2(origin.X + available, origin.Y + TitleHeight);
		float controlsWidth = IconSize + 6f;
		float dragWidth = MathF.Max(120f, available - controlsWidth);

		ImGui.InvisibleButton("##ld_title_drag", new Vector2(dragWidth, TitleHeight - 6f));
		if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left, 0f))
		{
			ImGui.SetWindowPos(ImGui.GetWindowPos() + ImGui.GetIO().MouseDelta);
		}

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		CyberDraw.Fill(drawList, origin, barMax, UiTheme.Alpha(UiTheme.TitleBarBackground, 0.92f), UiTheme.CutLarge,
			CutCorners.Diagonal);
		CyberDraw.Line(drawList, new Vector2(origin.X, barMax.Y - 1f), new Vector2(barMax.X, barMax.Y - 1f),
			UiTheme.Hairline);
		CyberDraw.LightBar(drawList, origin + new Vector2(4f, 6f), new Vector2(origin.X + 16f, barMax.Y - 6f),
			UiTheme.Accent, 2f);

		
		Vector2 badgeCenter = new Vector2(origin.X + 30f, origin.Y + TitleHeight * 0.5f - 1f);
		CyberDraw.Hexagon(drawList, badgeCenter, 12f, UiTheme.Well, UiTheme.Accent, 1.3f);
		CyberDraw.TextCentered(drawList, badgeCenter - new Vector2(11f, 11f), badgeCenter + new Vector2(11f, 11f), "LD",
			UiTheme.Accent, 0.6f);

		float titleX = origin.X + 50f;
		PromeUi.AddTextBold(drawList, new Vector2(titleX, origin.Y + 7f), UiTheme.Heading, "LITTLEDART // " + JobLabel);
		CyberDraw.Micro(drawList, new Vector2(titleX + 2f, origin.Y + TitleHeight - 16f), "CYBERDECK CONFIG", 1.5f);

		
		float iconY = origin.Y + (TitleHeight - IconSize) * 0.5f;
		float iconX = barMax.X - controlsWidth + 4f;
		if (TitleCloseButton("##ld_title_close", new Vector2(iconX, iconY)))
		{
			SaveLayoutNow();
			IsOpen = false;
		}

		ImGui.SetCursorScreenPos(new Vector2(origin.X, barMax.Y + 4f));
	}

	
	private static bool TitleCloseButton(string id, Vector2 position)
	{
		ImGui.SetCursorScreenPos(position);
		ImGui.PushID(id);
		ImGui.InvisibleButton("##icon", new Vector2(IconSize, IconSize));
		bool hovered = ImGui.IsItemHovered();
		bool clicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
		if (hovered)
		{
			ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
		}
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 min = position;
		Vector2 max = position + new Vector2(IconSize, IconSize);
		if (hovered)
		{
			CyberDraw.Fill(drawList, min, max, UiTheme.Alpha(UiTheme.Danger, 0.22f), UiTheme.CutSmall,
				CutCorners.Diagonal);
		}
		CyberDraw.Line(drawList, min, max, UiTheme.Alpha(UiTheme.Danger, hovered ? 1f : 0.7f), UiTheme.CutSmall,
			CutCorners.Diagonal, 1f);
		Vector2 center = (min + max) * 0.5f;
		drawList.AddLine(center + new Vector2(-5f, -5f), center + new Vector2(5f, 5f), UiTheme.ToU32(UiTheme.Danger),
			1.7f);
		drawList.AddLine(center + new Vector2(5f, -5f), center + new Vector2(-5f, 5f), UiTheme.ToU32(UiTheme.Danger),
			1.7f);
		ImGui.PopID();
		return clicked;
	}

	
	private void DrawModeDeck()
	{
		float available = ImGui.GetContentRegionAvail().X;
		Vector2 origin = ImGui.GetCursorScreenPos();
		Vector2 max = new Vector2(origin.X + available, origin.Y + ModeHeight);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		CyberDraw.Panel(drawList, origin, max, UiTheme.Alpha(UiTheme.CardBackground, 0.85f), UiTheme.Border,
			UiTheme.Cut, CutCorners.Diagonal, 1f);
		CyberDraw.LightBar(drawList, origin + new Vector2(2f, 5f), new Vector2(origin.X + 14f, max.Y - 5f),
			IsDailyMode ? UiTheme.Accent : UiTheme.Danger, 2f);
		CyberDraw.Micro(drawList, new Vector2(origin.X + 20f, origin.Y + 4f), "// COMBAT MODE", 1.4f);
		CyberDraw.Micro(drawList, new Vector2(max.X - 96f, origin.Y + 4f), "SYS.STATE //", 1.2f);
		CyberDraw.Ticks(drawList, new Vector2(origin.X + 2f, max.Y - 3f), new Vector2(max.X - 2f, max.Y), 18,
			UiTheme.Alpha(UiTheme.Border, 0.8f), 3f, 1);

		ImGui.SetCursorScreenPos(new Vector2(origin.X + 12f, origin.Y + 18f));
		DrawModeStrip();
		ImGui.SetCursorScreenPos(new Vector2(origin.X, max.Y + 4f));
	}

	private void DrawTabStrip()
	{
		IReadOnlyList<string> pages = PageNames;
		if (_page < 0 || _page >= pages.Count)
		{
			_page = 0;
		}
		float available = ImGui.GetContentRegionAvail().X;
		float gap = 4f;
		float tabWidth = MathF.Max(72f, (available - gap * (pages.Count - 1)) / pages.Count);
		for (int i = 0; i < pages.Count; i++)
		{
			if (i > 0)
			{
				ImGui.SameLine(0f, gap);
			}
			if (UiKit.TabButton(_windowId + "_tab_" + i, pages[i], _page == i, tabWidth))
			{
				_page = i;
			}
		}
		ImGui.Dummy(new Vector2(0f, 2f));
	}

	
	private void DrawGutter()
	{
		float available = ImGui.GetContentRegionAvail().X;
		Vector2 origin = ImGui.GetCursorScreenPos();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		string pageLabel = _page >= 0 && _page < PageNames.Count ? PageNames[_page] : string.Empty;
		string module = "MODULE " + (_page + 1).ToString("D2") + "/" + PageNames.Count.ToString("D2") + " // " + pageLabel;
		CyberDraw.Micro(drawList, new Vector2(origin.X + 2f, origin.Y + 2f), module.ToUpperInvariant(), 1.2f);
		CyberDraw.Micro(drawList, new Vector2(origin.X + available - 132f, origin.Y + 2f), "PROTOCOL 0928-J14",
			1.2f);
		CyberDraw.Ticks(drawList, new Vector2(origin.X + 2f, origin.Y), new Vector2(origin.X + available - 2f, origin.Y + 3f),
			26, UiTheme.Alpha(UiTheme.Border, 0.7f), 3f, 0);
		ImGui.Dummy(new Vector2(available, GutterHeight));
	}

	private void DrawBody()
	{
		float bodyHeight = MathF.Max(120f, ImGui.GetContentRegionAvail().Y - StatusHeight - 6f);
		ImGui.BeginChild("##ld_content", new Vector2(0f, bodyHeight), false);
		DrawContentSafely();
		ImGui.EndChild();

		Vector2 min = ImGui.GetItemRectMin();
		Vector2 max = ImGui.GetItemRectMax();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		CyberDraw.Line(drawList, min, max, UiTheme.Hairline, UiTheme.Cut, CutCorners.Diagonal, 1f);
		CyberDraw.Brackets(drawList, min + new Vector2(4f, 4f), max - new Vector2(4f, 4f),
			UiTheme.Alpha(UiTheme.Accent, 0.35f), 8f, 1f);
	}

	private void DrawContentSafely()
	{
		try
		{
			DrawPageContent(_page);
		}
		catch (Exception exception)
		{
			PluginLog.Error("[LittleDart/界面] 自建窗口绘制失败（页 " + _page + "）：" + exception);
			ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Danger);
			ImGui.TextWrapped("这一页绘制出错：" + exception.Message);
			ImGui.PopStyleColor();
		}
	}

	private void DrawStatusBar()
	{
		ImGui.Spacing();
		float available = ImGui.GetContentRegionAvail().X;
		Vector2 origin = ImGui.GetCursorScreenPos();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		CyberDraw.Rule(drawList, new Vector2(origin.X, origin.Y), new Vector2(origin.X + available, origin.Y),
			UiTheme.Hairline);
		CyberDraw.Ticks(drawList, new Vector2(origin.X, origin.Y), new Vector2(origin.X + 160f, origin.Y + 3f), 14,
			UiTheme.Alpha(UiTheme.Border, 0.9f), 3f, 0);
		ImGui.Dummy(new Vector2(available, 5f));

		AcrState state = AcrState.On;
		try
		{
			state = PromeSettings.Instance.EnableAcr;
		}
		catch (Exception exception)
		{
			PluginLog.Warning("[LittleDart/界面] 读取 ACR 状态失败：" + exception.Message);
		}
		string stateText = state switch
		{
			AcrState.On => "ACR RUNNING",
			AcrState.Hold => "ACR HOLD",
			_ => "ACR OFFLINE",
		};
		Vector4 stateColor = state switch
		{
			AcrState.On => UiTheme.Success,
			AcrState.Hold => UiTheme.Warning,
			_ => UiTheme.Danger,
		};
		UiKit.StatusPill(_windowId + "_st_acr", stateText, stateColor);
		ImGui.SameLine(0f, 6f);
		UiKit.StatusPill(_windowId + "_st_mode", IsDailyMode ? "MODE DAILY" : "MODE HARD",
			IsDailyMode ? UiTheme.Accent : UiTheme.Danger);
		ImGui.SameLine(0f, 6f);
		UiKit.StatusPill(_windowId + "_st_theme", UiTheme.CurrentThemeName, UiTheme.TextMuted);
		ImGui.SameLine(0f, 10f);
		ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.StatusText);
		ImGui.TextUnformatted("BUILD " + UiBuildInfo.Stamp + "  ·  " + JobShort);
		ImGui.PopStyleColor();

		
		CyberDraw.TextRight(drawList, new Vector2(origin.X, origin.Y + 6f),
			new Vector2(origin.X + available - 2f, origin.Y + 6f + ImGui.GetTextLineHeight()),
			"ZONE: DANGER  //  PWR 98%", UiTheme.Alpha(UiTheme.Danger, 0.85f), 0f, 1.1f);
	}
}
