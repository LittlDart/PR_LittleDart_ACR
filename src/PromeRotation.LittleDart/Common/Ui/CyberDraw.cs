using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace LittleDart.Common.Ui;

[Flags]
public enum CutCorners
{
	None = 0,

	TopLeft = 1,

	TopRight = 2,

	BottomRight = 4,

	BottomLeft = 8,

	
	Diagonal = TopLeft | BottomRight,

	
	DiagonalReverse = TopRight | BottomLeft,

	
	Bottom = BottomLeft | BottomRight,

	All = TopLeft | TopRight | BottomRight | BottomLeft,
}

public static class CyberDraw
{
	
	public static void Path(ImDrawListPtr drawList, Vector2 min, Vector2 max, float cut, CutCorners corners)
	{
		float capped = MathF.Min(cut, MathF.Min(max.X - min.X, max.Y - min.Y) * 0.5f);
		float topLeft = corners.HasFlag(CutCorners.TopLeft) ? capped : 0f;
		float topRight = corners.HasFlag(CutCorners.TopRight) ? capped : 0f;
		float bottomRight = corners.HasFlag(CutCorners.BottomRight) ? capped : 0f;
		float bottomLeft = corners.HasFlag(CutCorners.BottomLeft) ? capped : 0f;

		drawList.PathLineTo(new Vector2(min.X + topLeft, min.Y));
		drawList.PathLineTo(new Vector2(max.X - topRight, min.Y));
		if (topRight > 0f)
		{
			drawList.PathLineTo(new Vector2(max.X, min.Y + topRight));
		}
		drawList.PathLineTo(new Vector2(max.X, max.Y - bottomRight));
		if (bottomRight > 0f)
		{
			drawList.PathLineTo(new Vector2(max.X - bottomRight, max.Y));
		}
		drawList.PathLineTo(new Vector2(min.X + bottomLeft, max.Y));
		if (bottomLeft > 0f)
		{
			drawList.PathLineTo(new Vector2(min.X, max.Y - bottomLeft));
		}
		drawList.PathLineTo(new Vector2(min.X, min.Y + topLeft));
		if (topLeft > 0f)
		{
			drawList.PathLineTo(new Vector2(min.X + topLeft, min.Y));
		}
	}

	
	public static void Fill(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float cut = 0f, CutCorners corners = CutCorners.None)
	{
		if (color.W <= 0.001f || max.X - min.X <= 0.4f || max.Y - min.Y <= 0.4f)
		{
			return;
		}
		uint col = UiTheme.ToU32(color);
		float capped = MathF.Min(cut, MathF.Min(max.X - min.X, max.Y - min.Y) * 0.5f);
		if (capped <= 0.4f || corners == CutCorners.None)
		{
			drawList.AddRectFilled(min, max, col);
			return;
		}
		Path(drawList, min, max, capped, corners);
		drawList.PathFillConvex(col);
	}

	
	public static void Line(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float cut = 0f,
		CutCorners corners = CutCorners.None, float thickness = 1f)
	{
		if (color.W <= 0.001f || max.X - min.X <= 0.4f || max.Y - min.Y <= 0.4f)
		{
			return;
		}
		uint col = UiTheme.ToU32(color);
		float capped = MathF.Min(cut, MathF.Min(max.X - min.X, max.Y - min.Y) * 0.5f);
		if (capped <= 0.4f || corners == CutCorners.None)
		{
			drawList.AddRect(min, max, col, 0f, ImDrawFlags.None, thickness);
			return;
		}
		Path(drawList, min, max, capped, corners);
		drawList.PathStroke(col, ImDrawFlags.Closed, thickness);
	}

	
	public static void Panel(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 fill, Vector4 line, float cut,
		CutCorners corners = CutCorners.Diagonal, float thickness = 1f)
	{
		Fill(drawList, min, max, fill, cut, corners);
		Line(drawList, min, max, line, cut, corners, thickness);
	}

	
	public static void Glow(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float cut,
		CutCorners corners = CutCorners.None, float spread = 3.5f, float intensity = 1f)
	{
		for (int layer = 3; layer >= 1; layer--)
		{
			float grow = spread * layer / 3f;
			float alpha = 0.20f * intensity * (4 - layer) / 3f;
			Line(drawList, min - new Vector2(grow), max + new Vector2(grow), UiTheme.Alpha(color, alpha),
				cut + grow, corners, 1f);
		}
	}

	
	public static void Stripes(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float spacing = 9f,
		float thickness = 5f)
	{
		if (color.W <= 0.001f || max.X - min.X <= 1f || max.Y - min.Y <= 1f)
		{
			return;
		}
		float height = max.Y - min.Y;
		float step = MathF.Max(3f, spacing);
		drawList.PushClipRect(min, max, true);
		uint col = UiTheme.ToU32(color);
		for (float x = min.X - height; x < max.X; x += step)
		{
			drawList.AddLine(new Vector2(x, max.Y), new Vector2(x + height, min.Y), col, thickness);
		}
		drawList.PopClipRect();
	}

	
	
	
	
	
	public static void Segments(ImDrawListPtr drawList, Vector2 min, Vector2 max, int count, float ratio,
		Vector4 on, Vector4 off, float cut = 0f, float gap = 2f, bool glowTip = true)
	{
		if (count <= 0 || max.X - min.X <= 2f)
		{
			return;
		}
		float clamped = Math.Clamp(float.IsNaN(ratio) ? 0f : ratio, 0f, 1f);
		float exact = clamped * count;
		int full = (int)MathF.Floor(exact);
		float fraction = exact - full;
		float segment = (max.X - min.X - gap * (count - 1)) / count;
		if (segment <= 0.5f)
		{
			return;
		}
		float cellCut = MathF.Min(cut, segment * 0.5f);
		for (int i = 0; i < count; i++)
		{
			float left = min.X + i * (segment + gap);
			Vector2 cellMin = new Vector2(left, min.Y);
			Vector2 cellMax = new Vector2(left + segment, max.Y);
			bool lit = i < full;
			Fill(drawList, cellMin, cellMax, off, cellCut, CutCorners.All);
			if (lit)
			{
				Fill(drawList, cellMin, cellMax, on, cellCut, CutCorners.All);
			}
			else if (i == full && fraction > 0.04f)
			{
				float partial = MathF.Max(1.5f, segment * fraction);
				Fill(drawList, cellMin, new Vector2(cellMin.X + partial, cellMax.Y), on,
					MathF.Min(cellCut, partial * 0.5f), CutCorners.All);
			}
		}
		if (glowTip && full < count)
		{
			float tipX = min.X + full * (segment + gap);
			Glow(drawList, new Vector2(tipX, min.Y), new Vector2(tipX + segment, max.Y), on, cellCut,
				CutCorners.All, 2.5f, 0.8f);
		}
	}

	
	public static void Ticks(ImDrawListPtr drawList, Vector2 min, Vector2 max, int count, Vector4 color, float length,
		int side = 0, float thickness = 1f)
	{
		if (count <= 1 || color.W <= 0.001f)
		{
			return;
		}
		uint col = UiTheme.ToU32(color);
		for (int i = 0; i < count; i++)
		{
			float t = (float)i / (count - 1);
			
			bool major = i % 4 == 0 || i == count - 1;
			float len = major ? length : length * 0.55f;
			switch (side)
			{
				case 1:
					{
						float x = min.X + (max.X - min.X) * t;
						drawList.AddLine(new Vector2(x, max.Y - len), new Vector2(x, max.Y), col, thickness);
						break;
					}
				case 2:
					{
						float y = min.Y + (max.Y - min.Y) * t;
						drawList.AddLine(new Vector2(min.X, y), new Vector2(min.X + len, y), col, thickness);
						break;
					}
				case 3:
					{
						float y = min.Y + (max.Y - min.Y) * t;
						drawList.AddLine(new Vector2(max.X - len, y), new Vector2(max.X, y), col, thickness);
						break;
					}
				default:
					{
						float x = min.X + (max.X - min.X) * t;
						drawList.AddLine(new Vector2(x, min.Y), new Vector2(x, min.Y + len), col, thickness);
						break;
					}
			}
		}
	}

	
	public static void Hexagon(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 fill, Vector4 line,
		float thickness = 1f)
	{
		float capped = MathF.Max(2f, radius);
		Span<Vector2> points = stackalloc Vector2[6];
		for (int i = 0; i < 6; i++)
		{
			float angle = -MathF.PI / 2f + i * MathF.PI / 3f;
			points[i] = center + new Vector2(MathF.Cos(angle) * capped, MathF.Sin(angle) * capped);
		}
		if (fill.W > 0.001f)
		{
			drawList.PathLineTo(points[0]);
			for (int j = 1; j < 6; j++)
			{
				drawList.PathLineTo(points[j]);
			}
			drawList.PathFillConvex(UiTheme.ToU32(fill));
		}
		if (line.W > 0.001f)
		{
			drawList.PathLineTo(points[0]);
			for (int k = 1; k < 6; k++)
			{
				drawList.PathLineTo(points[k]);
			}
			drawList.PathStroke(UiTheme.ToU32(line), ImDrawFlags.Closed, thickness);
		}
	}

	
	public static void Chevron(ImDrawListPtr drawList, Vector2 topLeft, float width, float height, Vector4 color,
		float thickness = 1.4f, float gap = 3.5f)
	{
		uint col = UiTheme.ToU32(color);
		float mid = topLeft.Y + height * 0.5f;
		Vector2 first = new Vector2(topLeft.X, topLeft.Y);
		Vector2 firstTip = new Vector2(topLeft.X + width, mid);
		Vector2 firstEnd = new Vector2(topLeft.X, topLeft.Y + height);
		drawList.AddLine(first, firstTip, col, thickness);
		drawList.AddLine(firstTip, firstEnd, col, thickness);
		Vector2 second = new Vector2(topLeft.X + gap, topLeft.Y + 1f);
		Vector2 secondTip = new Vector2(topLeft.X + gap + width, mid);
		Vector2 secondEnd = new Vector2(topLeft.X + gap, topLeft.Y + height - 1f);
		drawList.AddLine(second, secondTip, col, thickness);
		drawList.AddLine(secondTip, secondEnd, col, thickness);
	}

	
	public static void Warning(ImDrawListPtr drawList, Vector2 center, float size, Vector4 color, float thickness = 1.4f)
	{
		float half = size * 0.5f;
		Vector2 top = new Vector2(center.X, center.Y - half);
		Vector2 right = new Vector2(center.X + half, center.Y + half * 0.85f);
		Vector2 left = new Vector2(center.X - half, center.Y + half * 0.85f);
		uint col = UiTheme.ToU32(color);
		drawList.AddLine(top, right, col, thickness);
		drawList.AddLine(right, left, col, thickness);
		drawList.AddLine(left, top, col, thickness);
		drawList.AddLine(new Vector2(center.X, center.Y - size * 0.12f), new Vector2(center.X, center.Y + size * 0.16f), col, thickness);
		drawList.AddRectFilled(new Vector2(center.X - 0.8f, center.Y + size * 0.26f),
			new Vector2(center.X + 0.8f, center.Y + size * 0.26f + 1.8f), col);
	}

	
	public static void Brackets(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float length = 9f,
		float thickness = 1.2f)
	{
		uint col = UiTheme.ToU32(color);
		drawList.AddLine(new Vector2(min.X, min.Y + length), new Vector2(min.X, min.Y), col, thickness);
		drawList.AddLine(new Vector2(min.X, min.Y), new Vector2(min.X + length, min.Y), col, thickness);
		drawList.AddLine(new Vector2(max.X - length, min.Y), new Vector2(max.X, min.Y), col, thickness);
		drawList.AddLine(new Vector2(max.X, min.Y), new Vector2(max.X, min.Y + length), col, thickness);
		drawList.AddLine(new Vector2(min.X, max.Y - length), new Vector2(min.X, max.Y), col, thickness);
		drawList.AddLine(new Vector2(min.X, max.Y), new Vector2(min.X + length, max.Y), col, thickness);
		drawList.AddLine(new Vector2(max.X - length, max.Y), new Vector2(max.X, max.Y), col, thickness);
		drawList.AddLine(new Vector2(max.X, max.Y), new Vector2(max.X, max.Y - length), col, thickness);
	}

	
	public static void LightBar(ImDrawListPtr drawList, Vector2 min, Vector2 max, Vector4 color, float inset,
		bool glow = true)
	{
		Vector2 barMin = new Vector2(min.X + 1.5f, min.Y + inset);
		Vector2 barMax = new Vector2(min.X + 1.5f + 3f, max.Y - inset);
		if (barMax.Y <= barMin.Y)
		{
			return;
		}
		Fill(drawList, barMin, barMax, color, 0f, CutCorners.None);
		if (glow)
		{
			Glow(drawList, barMin, barMax, color, 0f, CutCorners.None, 2.4f, 0.9f);
		}
	}

	
	public static Vector2 Measure(string text, float tracking = 0f)
	{
		if (string.IsNullOrEmpty(text))
		{
			return Vector2.Zero;
		}
		Vector2 size = ImGui.CalcTextSize(text);
		return new Vector2(size.X + tracking * (text.Length - 1), size.Y);
	}

	
	public static void Text(ImDrawListPtr drawList, Vector2 position, string text, Vector4 color, float tracking = 0f)
	{
		if (string.IsNullOrEmpty(text) || color.W <= 0.001f)
		{
			return;
		}
		uint col = UiTheme.ToU32(color);
		if (tracking <= 0.01f)
		{
			drawList.AddText(position, col, text);
			return;
		}
		float x = position.X;
		for (int i = 0; i < text.Length; i++)
		{
			string part = text[i].ToString();
			drawList.AddText(new Vector2(x, position.Y), col, part);
			x += ImGui.CalcTextSize(part).X + tracking;
		}
	}

	
	public static void TextCentered(ImDrawListPtr drawList, Vector2 min, Vector2 max, string text, Vector4 color,
		float tracking = 0f)
	{
		Vector2 size = Measure(text, tracking);
		Text(drawList, new Vector2(min.X + (max.X - min.X - size.X) * 0.5f,
			min.Y + (max.Y - min.Y - size.Y) * 0.5f), text, color, tracking);
	}

	
	public static void TextRight(ImDrawListPtr drawList, Vector2 min, Vector2 max, string text, Vector4 color,
		float rightPadding = 0f, float tracking = 0f)
	{
		Vector2 size = Measure(text, tracking);
		Text(drawList, new Vector2(max.X - rightPadding - size.X, min.Y + (max.Y - min.Y - size.Y) * 0.5f), text,
			color, tracking);
	}

	
	public static void Micro(ImDrawListPtr drawList, Vector2 position, string text, float tracking = 1.3f)
	{
		Text(drawList, position, text, UiTheme.Micro, tracking);
	}

	
	public static void Rule(ImDrawListPtr drawList, Vector2 from, Vector2 to, Vector4 color, float thickness = 1f,
		bool arrow = false)
	{
		uint col = UiTheme.ToU32(color);
		drawList.AddLine(from, to, col, thickness);
		if (arrow)
		{
			drawList.AddLine(to, to + new Vector2(-4f, -2.5f), col, thickness);
			drawList.AddLine(to, to + new Vector2(-4f, 2.5f), col, thickness);
		}
	}
}
