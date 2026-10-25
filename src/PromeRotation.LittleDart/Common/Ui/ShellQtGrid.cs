using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using PromeRotation.UI.QuickToggles;

namespace LittleDart.Common.Ui;

public static class ShellQtGrid
{
	
	private const int ColumnCount = 3;

	
	private const float CellHeight = 44f;

	
	private const float CellSpacing = 6f;

	
	private const float CategoryHeight = 16f;

	
	private const float MinimumItemWidth = 100f;

	
	private const float LabelTracking = 0.25f;

	
	private const float IndexTracking = 1f;

	
	
	
	
	private const float MicroScale = 0.84f;

	
	
	
	
	internal const float EmptyHintHeight = 36f;

	
	
	
	
	private static readonly string[] IndexLabels = CreateIndexLabels(64);

	
	
	
	
	
	public static float CalculateHeight(IReadOnlyList<QtDefinition> definitions, float availableWidth, float minItemWidth = MinimumItemWidth)
	{
		if (definitions.Count == 0)
		{
			return 0f;
		}
		List<(string Category, List<QtDefinition> Items)> groups = Group(definitions);
		float labelHeight = groups.Count > 1 ? CategoryHeight : 0f;
		float total = 0f;
		for (int g = 0; g < groups.Count; g++)
		{
			total += GroupHeight(groups[g].Items.Count, labelHeight);
			if (g < groups.Count - 1)
			{
				total += CellSpacing;
			}
		}
		return total;
	}

	
	public static bool Draw(IReadOnlyList<QtDefinition> definitions, Func<string, bool> getValue, Action<string, bool> setValue, float minItemWidth = MinimumItemWidth)
	{
		Vector2 origin = ImGui.GetCursorScreenPos();
		float available = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
		float cellWidth = CellWidth(available, minItemWidth);

		if (definitions.Count == 0)
		{
			DrawEmpty(origin, available);
			ImGui.SetCursorScreenPos(new Vector2(origin.X, origin.Y + EmptyHintHeight));
			return false;
		}

		List<(string Category, List<QtDefinition> Items)> groups = Group(definitions);
		bool showCategory = groups.Count > 1;
		float labelHeight = showCategory ? CategoryHeight : 0f;

		
		
		ImGui.Dummy(new Vector2(available, CalculateHeight(definitions, available, minItemWidth)));

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		bool changed = false;
		float y = origin.Y;
		for (int g = 0; g < groups.Count; g++)
		{
			List<QtDefinition> items = groups[g].Items;
			float groupTop = y;
			if (showCategory)
			{
				DrawCategory(drawList, groups[g].Category, groupTop, origin.X, available);
			}
			float rowTop = groupTop + labelHeight;
			for (int i = 0; i < items.Count; i++)
			{
				QtDefinition definition = items[i];
				bool enabled = getValue(definition.Id);
				int row = i / ColumnCount;
				int column = i % ColumnCount;
				Vector2 min = new Vector2(origin.X + column * (cellWidth + CellSpacing), rowTop + row * (CellHeight + CellSpacing));
				Vector2 max = min + new Vector2(cellWidth, CellHeight);
				if (DrawCell(drawList, definition, i, enabled, min, max))
				{
					setValue(definition.Id, !enabled);
					changed = true;
				}
			}
			y = groupTop + GroupHeight(items.Count, labelHeight);
			if (g < groups.Count - 1)
			{
				y += CellSpacing;
			}
		}

		
		ImGui.SetCursorScreenPos(new Vector2(origin.X, y));
		return changed;
	}

	
	private static float GroupHeight(int itemCount, float labelHeight)
	{
		int rows = (itemCount + ColumnCount - 1) / ColumnCount;
		return labelHeight + rows * CellHeight + MathF.Max(0f, rows - 1) * CellSpacing;
	}

	
	private static float CellWidth(float available, float minItemWidth)
	{
		float width = (available - CellSpacing * (ColumnCount - 1)) / ColumnCount;
		return MathF.Max(minItemWidth, width);
	}

	
	private static List<(string Category, List<QtDefinition> Items)> Group(IReadOnlyList<QtDefinition> definitions)
	{
		List<(string Category, List<QtDefinition> Items)> groups = new List<(string, List<QtDefinition>)>();
		foreach (QtDefinition definition in definitions)
		{
			int index = groups.FindIndex((g) => string.Equals(g.Category, definition.Category, StringComparison.Ordinal));
			if (index < 0)
			{
				groups.Add((definition.Category, new List<QtDefinition> { definition }));
			}
			else
			{
				groups[index].Items.Add(definition);
			}
		}
		return groups;
	}

	
	private static void DrawCategory(ImDrawListPtr drawList, string category, float top, float left, float width)
	{
		
		string label = category.ToUpperInvariant();
		MicroText(drawList, new Vector2(left + 2f, top), "//", UiTheme.Alpha(UiTheme.BorderStrong, 0.85f), 1f);
		float prefixWidth = MeasureMicro("//", 1f).X + 5f;
		MicroText(drawList, new Vector2(left + 2f + prefixWidth, top), label, UiTheme.Micro, 1.4f);
		float labelEnd = left + 2f + prefixWidth + MeasureMicro(label, 1.4f).X + 6f;
		DotLeader(drawList, labelEnd, left + width, top + CategoryHeight * 0.5f, UiTheme.Alpha(UiTheme.Border, 0.55f));
	}

	
	private static void DotLeader(ImDrawListPtr drawList, float fromX, float toX, float y, Vector4 color)
	{
		if (color.W <= 0.001f || toX - fromX <= 2f)
		{
			return;
		}
		uint col = UiTheme.ToU32(color);
		for (float x = fromX; x < toX - 1.5f; x += 4.5f)
		{
			drawList.AddRectFilled(new Vector2(x, y), new Vector2(x + 1.5f, y + 1.5f), col);
		}
	}

	
	
	
	
	private static bool DrawCell(ImDrawListPtr drawList, QtDefinition definition, int index, bool enabled, Vector2 min, Vector2 max)
	{
		ImGui.PushID(definition.Id);
		ImGui.SetCursorScreenPos(min);
		ImGui.InvisibleButton("##qt", max - min);
		bool hovered = ImGui.IsItemHovered();
		bool clicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);

		float cut = UiTheme.CutSmall;
		Vector4 background;
		Vector4 border;
		Vector4 text;
		if (enabled)
		{
			
			background = UiTheme.Alpha(UiTheme.Well, hovered ? 0.96f : 0.86f);
			if (hovered)
			{
				background = UiTheme.Mix(background, UiTheme.Accent, 0.10f);
			}
			border = UiTheme.Accent;
			text = UiTheme.Accent;
			CyberDraw.Glow(drawList, min, max, UiTheme.Glow, cut, CutCorners.BottomRight, 3.2f, hovered ? 1.15f : 0.9f);
		}
		else
		{
			
			background = UiTheme.Alpha(UiTheme.Mix(UiTheme.CardBackground, UiTheme.Border, 0.16f), 0.90f);
			if (hovered)
			{
				background = UiTheme.Mix(background, UiTheme.SurfaceHover, 0.55f);
			}
			border = hovered ? UiTheme.BorderStrong : UiTheme.Alpha(UiTheme.Border, 0.85f);
			text = hovered ? UiTheme.Mix(UiTheme.TextMuted, UiTheme.Text, 0.40f) : UiTheme.Mix(UiTheme.TextMuted, UiTheme.Border, 0.35f);
		}

		
		CyberDraw.Fill(drawList, min, max, background, cut, CutCorners.BottomRight);
		CyberDraw.Line(drawList, min, max, border, cut, CutCorners.BottomRight, 1f);

		
		CyberDraw.LightBar(drawList, min, max, enabled ? UiTheme.Accent : UiTheme.Mix(UiTheme.Border, UiTheme.Layout, 0.30f), 9f, enabled);

		
		string indexLabel = IndexLabel(index);
		Vector2 indexSize = MeasureMicro(indexLabel, IndexTracking);
		MicroText(drawList, new Vector2(max.X - 6f - indexSize.X, min.Y + 3f), indexLabel,
			enabled ? UiTheme.Alpha(UiTheme.Accent, 0.85f) : UiTheme.Micro, IndexTracking);

		
		
		Vector2 labelSize = CyberDraw.Measure(definition.Label, LabelTracking);
		float clipRight = MathF.Max(min.X + 8f, max.X - 2f);
		drawList.PushClipRect(new Vector2(min.X + 4f, min.Y + 1f), new Vector2(clipRight, max.Y - 1f), true);
		CyberDraw.Text(drawList, new Vector2(min.X + 8f, min.Y + (max.Y - min.Y - labelSize.Y) * 0.5f + 2f), definition.Label,
			text, LabelTracking);
		drawList.PopClipRect();

		if (hovered)
		{
			
			if (!string.IsNullOrWhiteSpace(definition.Description))
			{
				ImGui.SetTooltip(definition.Description);
			}
			ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
		}
		ImGui.PopID();
		return clicked;
	}

	
	private static void DrawEmpty(Vector2 origin, float width)
	{
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 max = origin + new Vector2(width, EmptyHintHeight);
		CyberDraw.Panel(drawList, origin, max, UiTheme.Alpha(UiTheme.Mix(UiTheme.CardBackground, UiTheme.Border, 0.16f), 0.80f),
			UiTheme.Alpha(UiTheme.Border, 0.75f), UiTheme.CutSmall, CutCorners.Diagonal, 1f);
		MicroText(drawList, origin + new Vector2(9f, 3f), "// NO QT LINK", UiTheme.Micro, 1.3f);
		CyberDraw.Text(drawList, origin + new Vector2(9f, 3f + ImGui.GetTextLineHeight()), "当前职业没有可显示的 QT",
			UiTheme.Mix(UiTheme.TextMuted, UiTheme.Border, 0.30f), 0.25f);
	}

	
	
	
	
	
	
	internal static void MicroText(ImDrawListPtr drawList, Vector2 position, string text, Vector4 color, float tracking)
	{
		if (string.IsNullOrEmpty(text) || color.W <= 0.001f)
		{
			return;
		}
		var font = ImGui.GetFont();
		float fontSize = ImGui.GetFontSize() * MicroScale;
		uint col = UiTheme.ToU32(color);
		if (tracking <= 0.01f)
		{
			drawList.AddText(font, fontSize, position, col, text, 0f);
			return;
		}
		float x = position.X;
		for (int i = 0; i < text.Length; i++)
		{
			string part = text[i].ToString();
			drawList.AddText(font, fontSize, new Vector2(x, position.Y), col, part, 0f);
			x += ImGui.CalcTextSize(part).X * MicroScale + tracking;
		}
	}

	
	internal static Vector2 MeasureMicro(string text, float tracking)
	{
		if (string.IsNullOrEmpty(text))
		{
			return Vector2.Zero;
		}
		Vector2 size = ImGui.CalcTextSize(text);
		return new Vector2(size.X * MicroScale + tracking * (text.Length - 1), size.Y * MicroScale);
	}

	
	
	
	
	
	internal static float PreferredContentWidth(IReadOnlyList<QtDefinition> definitions, float minCellWidth = MinimumItemWidth, float maxCellWidth = 168f)
	{
		float widest = minCellWidth;
		for (int i = 0; i < definitions.Count; i++)
		{
			string label = definitions[i].Label ?? string.Empty;
			float needed = 22f + ImGui.CalcTextSize(label).X + 8f + MeasureMicro("+0", IndexTracking).X + 6f;
			if (needed > widest)
			{
				widest = MathF.Min(maxCellWidth, needed);
			}
		}
		return widest * ColumnCount + CellSpacing * (ColumnCount - 1);
	}

	
	private static string IndexLabel(int index)
	{
		if ((uint)index < (uint)IndexLabels.Length)
		{
			return IndexLabels[index];
		}
		return "+" + index;
	}

	private static string[] CreateIndexLabels(int count)
	{
		string[] labels = new string[count];
		for (int i = 0; i < count; i++)
		{
			labels[i] = "+" + i;
		}
		return labels;
	}
}
