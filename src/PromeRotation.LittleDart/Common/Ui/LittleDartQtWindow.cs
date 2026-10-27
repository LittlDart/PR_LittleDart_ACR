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

public sealed class LittleDartQtWindow : Window
{
	public const string WindowId = "littledart_qt_panel";

	
	
	
	
	
	public const float MinPanelWidth = 320f;

	
	public const float MaxPanelWidth = 470f;

	
	private const float FrameGap = 6f;

	
	private const float HeaderHeight = 22f;

	
	private const float HeaderGap = 9f;

	
	private const float FooterHeight = 18f;

	
	private const string TitleText = "// TACTICAL MATRIX";

	
	private const float TitleTracking = 1f;

	
	private const float MicroTracking = 1f;

	
	private const string ZoneText = "[DANGER]";

	
	private const string ProtocolText = "// PROTOCOL 0928-J14";

	
	private const string LinkText = "[LNK: STABLE]";

	private static LittleDartQtWindow? _instance;

	private static bool _registered;

	private PromeUiStyleScope? _promeScope;

	private UiShellStyleScope _shellScope;

	
	
	
	
	
	private Vector2 _frameSize = new Vector2(MinPanelWidth, 263f);

	public static LittleDartQtWindow Instance => _instance ??= new LittleDartQtWindow();

	private LittleDartQtWindow()
		: base("LittleDart · QT###" + WindowId,
			ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		RespectCloseHotkey = false;
		SizeCondition = ImGuiCond.Always;
		
		Size = _frameSize;
	}

	public override void PreDraw()
	{
		
		
		
		UpdateFrameSize();
		_promeScope = UiTheme.PushWindowBase(0.94f);
		_shellScope = UiTheme.PushShellStyle();
		base.PreDraw();
	}

	
	
	
	
	
	
	
	
	
	
	
	private void UpdateFrameSize()
	{
		IReadOnlyList<QtDefinition> definitions = ResolveDefinitions();
		Vector2 padding = UiTheme.WindowPadding;
		float preferred = ShellQtGrid.PreferredContentWidth(definitions);
		float minWidth = UiTheme.Px(MinPanelWidth);
		float maxWidth = UiTheme.Px(MaxPanelWidth);
		float contentWidth = Math.Clamp(preferred, minWidth - padding.X * 2f, maxWidth - padding.X * 2f);
		float gridHeight = definitions.Count == 0
			? ShellQtGrid.EmptyHintHeight
			: ShellQtGrid.CalculateHeight(definitions, contentWidth);
		_frameSize = new Vector2(contentWidth + padding.X * 2f,
			padding.Y * 2f + UiTheme.Px(HeaderHeight) + UiTheme.Px(HeaderGap) + gridHeight + UiTheme.Px(FooterHeight));
		Size = _frameSize / UiTheme.UiScale;
	}

	
	private static IReadOnlyList<QtDefinition> ResolveDefinitions()
	{
		return QtRegistry.ResolveVisible(PromeSettings.Instance.QuickToggles.Keys, PromeSettings.Instance.HiddenQts);
	}

	public override void PostDraw()
	{
		_shellScope.Dispose();
		_shellScope = default;
		_promeScope?.Dispose();
		_promeScope = null;
		base.PostDraw();
	}

	public override void Draw()
	{
		IReadOnlyList<QtDefinition> definitions = ResolveDefinitions();
		Vector2 padding = UiTheme.WindowPadding;

		
		
		
		
		Vector2 windowPos = ImGui.GetWindowPos();
		Vector2 windowSize = ImGui.GetWindowSize();
		Vector2 origin = windowPos + padding;
		Vector2 limit = windowPos + windowSize - padding;
		float headerHeight = UiTheme.Px(HeaderHeight);

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		DrawShell(drawList, origin, limit);
		DrawHeader(drawList, origin, limit);
		DrawFooter(drawList, origin, limit);

		ImGui.SetCursorScreenPos(new Vector2(origin.X, origin.Y + headerHeight + UiTheme.Px(HeaderGap)));
		ShellQtGrid.Draw(definitions, PromeSettings.Instance.GetQt, SetQt);
	}

	
	private static void DrawShell(ImDrawListPtr drawList, Vector2 origin, Vector2 limit)
	{
		float headerHeight = UiTheme.Px(HeaderHeight);
		float footerHeight = UiTheme.Px(FooterHeight);
		
		Vector2 frameMin = origin - new Vector2(UiTheme.Px(FrameGap));
		Vector2 frameMax = limit + new Vector2(UiTheme.Px(FrameGap));
		float cut = UiTheme.CutLarge;

		
		CyberDraw.Panel(drawList, frameMin, frameMax,
			UiTheme.Alpha(UiTheme.Mix(UiTheme.Layout, UiTheme.Border, 0.10f), 0.55f),
			UiTheme.Alpha(UiTheme.BorderStrong, 0.90f), cut, CutCorners.Diagonal, 1.2f);
		
		float innerCut = MathF.Max(0f, cut - UiTheme.Px(2f));
		CyberDraw.Fill(drawList, frameMin + new Vector2(UiTheme.Px(2f)),
			new Vector2(frameMax.X - UiTheme.Px(2f), origin.Y + headerHeight + UiTheme.Px(3.5f)),
			UiTheme.Alpha(UiTheme.TitleBarBackground, 0.55f), innerCut, CutCorners.TopLeft);
		
		CyberDraw.Line(drawList, frameMin + new Vector2(UiTheme.Px(3f)), frameMax - new Vector2(UiTheme.Px(3f)),
			UiTheme.Alpha(UiTheme.Border, 0.45f), innerCut, CutCorners.Diagonal, 1f);

		
		CyberDraw.Brackets(drawList, frameMin + new Vector2(UiTheme.Px(1.5f)), frameMax - new Vector2(UiTheme.Px(1.5f)),
			UiTheme.Alpha(UiTheme.Accent, 0.45f), UiTheme.Px(10f), 1.2f);
		CyberDraw.Ticks(drawList, frameMin + new Vector2(UiTheme.Px(10f), 0f), frameMax - new Vector2(UiTheme.Px(10f), UiTheme.Px(1f)), 13,
			UiTheme.Alpha(UiTheme.BorderStrong, 0.70f), UiTheme.Px(4f), 1);
		CyberDraw.Ticks(drawList, frameMin + new Vector2(0f, UiTheme.Px(10f)), frameMax - new Vector2(UiTheme.Px(1f), UiTheme.Px(10f)), 7,
			UiTheme.Alpha(UiTheme.Border, 0.55f), UiTheme.Px(3f), 3);

		
		Vector2 wellMin = new Vector2(origin.X - UiTheme.Px(3f), origin.Y + headerHeight + UiTheme.Px(HeaderGap) - UiTheme.Px(3f));
		Vector2 wellMax = new Vector2(limit.X + UiTheme.Px(3f), limit.Y - footerHeight + UiTheme.Px(3f));
		CyberDraw.Fill(drawList, wellMin, wellMax, UiTheme.Alpha(UiTheme.Mix(UiTheme.Layout, UiTheme.Well, 0.22f), 0.45f),
			0f, CutCorners.None);
	}

	
	
	
	
	
	private void DrawHeader(ImDrawListPtr drawList, Vector2 origin, Vector2 limit)
	{
		float headerHeight = UiTheme.Px(HeaderHeight);
		float top = origin.Y;
		float bottom = origin.Y + headerHeight;

		
		ImGui.SetCursorScreenPos(new Vector2(origin.X, top));
		ImGui.InvisibleButton("##ld_qt_drag", new Vector2(MathF.Max(UiTheme.Px(24f), limit.X - origin.X), headerHeight));
		if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left, 0f))
		{
			ImGui.SetWindowPos(ImGui.GetWindowPos() + ImGui.GetIO().MouseDelta);
		}

		
		Vector2 barMin = new Vector2(origin.X, top + UiTheme.Px(4f));
		Vector2 barMax = new Vector2(origin.X + UiTheme.Px(3f), bottom - UiTheme.Px(4f));
		CyberDraw.Fill(drawList, barMin, barMax, UiTheme.Accent, 0f, CutCorners.None);
		CyberDraw.Glow(drawList, barMin, barMax, UiTheme.Glow, 0f, CutCorners.None, UiTheme.Px(2.4f), 0.9f);

		
		
		Vector2 zoneSize = ShellQtGrid.MeasureMicro(ZoneText, MicroTracking);
		float titleLeft = origin.X + UiTheme.Px(9f);
		float titleWidth = CyberDraw.Measure(TitleText, TitleTracking).X;
		float zoneLeft = limit.X - zoneSize.X;
		bool showZone = zoneLeft - UiTheme.Px(10f) >= titleLeft + titleWidth;
		float titleClipRight = (showZone ? zoneLeft - UiTheme.Px(8f) : limit.X);
		drawList.PushClipRect(new Vector2(origin.X, top), new Vector2(titleClipRight, bottom), true);
		CyberDraw.Text(drawList, new Vector2(titleLeft, top + (headerHeight - ImGui.GetTextLineHeight()) * 0.5f), TitleText,
			UiTheme.Text, TitleTracking);
		drawList.PopClipRect();

		
		if (showZone)
		{
			ShellQtGrid.MicroText(drawList, new Vector2(zoneLeft, top + (headerHeight - zoneSize.Y) * 0.5f), ZoneText,
				UiTheme.Alpha(UiTheme.Danger, 0.80f), MicroTracking);
		}

		
		float ruleY = bottom + UiTheme.Px(3.5f);
		CyberDraw.Rule(drawList, new Vector2(origin.X, ruleY), new Vector2(limit.X, ruleY), UiTheme.Alpha(UiTheme.Border, 0.55f), 1f);
		CyberDraw.Rule(drawList, new Vector2(origin.X, ruleY), new Vector2(origin.X + UiTheme.Px(42f), ruleY), UiTheme.Alpha(UiTheme.Accent, 0.85f), 1.4f);
	}

	
	private static void DrawFooter(ImDrawListPtr drawList, Vector2 origin, Vector2 limit)
	{
		float y = limit.Y - UiTheme.Px(FooterHeight) + UiTheme.Px(3f);
		ShellQtGrid.MicroText(drawList, new Vector2(origin.X, y), ProtocolText, UiTheme.Micro, MicroTracking);
		Vector2 linkSize = ShellQtGrid.MeasureMicro(LinkText, MicroTracking);
		float linkLeft = limit.X - linkSize.X;
		float protocolRight = origin.X + ShellQtGrid.MeasureMicro(ProtocolText, MicroTracking).X;
		
		if (linkLeft - UiTheme.Px(10f) <= protocolRight + UiTheme.Px(8f))
		{
			return;
		}
		ShellQtGrid.MicroText(drawList, new Vector2(linkLeft, y), LinkText, UiTheme.Micro, MicroTracking);
		CyberDraw.Fill(drawList, new Vector2(linkLeft - UiTheme.Px(8f), y + UiTheme.Px(5f)),
			new Vector2(linkLeft - UiTheme.Px(6f), y + UiTheme.Px(7f)), UiTheme.Accent, 0f, CutCorners.None);
	}

	private static void SetQt(string id, bool value)
	{
		try
		{
			PromeSettings.Instance.SetQt(id, value);
		}
		catch (Exception exception)
		{
			PluginLog.Warning("[LittleDart/界面] 写 QT 失败（" + id + "）：" + exception.Message);
		}
	}

	public static void Register()
	{
		if (_registered)
		{
			return;
		}
		Plugin.Instance.WindowSystem.AddWindow((IWindow)(object)Instance);
		_registered = true;
	}

	public static void Unregister()
	{
		if (!_registered)
		{
			return;
		}
		_registered = false;
		((Window)Instance).IsOpen = false;
		Plugin.Instance.WindowSystem.RemoveWindow((IWindow)(object)Instance);
	}
}
