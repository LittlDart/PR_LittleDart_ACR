using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;

namespace LittleDart.Common.Ui;

public static class ShellWidgets
{
	
	private const float MicroScale = 0.84f;

	
	private const float ValueScale = 1.14f;

	
	
	
	private static readonly Dictionary<string, string> ProtocolCache = new Dictionary<string, string>(StringComparer.Ordinal);

	
	private static readonly Dictionary<string, int> TabOrder = new Dictionary<string, int>(StringComparer.Ordinal);

	
	
	

	
	public static void SectionHeader(string title, string? description = null, bool accent = true)
	{
		ImGui.Spacing();
		float width = MathF.Max(48f, ImGui.GetContentRegionAvail().X);
		float lineHeight = ImGui.GetTextLineHeight();
		Vector2 start = ImGui.GetCursorScreenPos();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		float barHeight = lineHeight + 4f;

		
		Vector4 barColor = accent ? UiTheme.Accent : UiTheme.Border;
		float barWidth = accent ? 3f : 2f;
		CyberDraw.Fill(drawList, new Vector2(start.X, start.Y + 1f),
			new Vector2(start.X + barWidth, start.Y + barHeight), barColor, 0f, CutCorners.None);

		string caption = "// " + title;
		TextScaled(drawList, new Vector2(start.X + 11f, start.Y), caption,
			accent ? UiTheme.Heading : UiTheme.TextMuted, 1f, 1.1f);

		
		string code = ProtocolCode(title);
		Vector2 codeSize = MeasureScaled(code, MicroScale, 1.4f);
		float titleWidth = MeasureScaled(caption, 1f, 1.1f).X;
		float codeLeft = start.X + width - codeSize.X;
		if (codeLeft > start.X + 11f + titleWidth + 16f)
		{
			TextScaled(drawList, new Vector2(codeLeft, start.Y + (lineHeight - codeSize.Y) * 0.5f), code,
				UiTheme.Micro, MicroScale, 1.4f);
		}

		float ruleY = start.Y + barHeight + 3f;
		CyberDraw.Rule(drawList, new Vector2(start.X, ruleY), new Vector2(start.X + width, ruleY), UiTheme.Hairline);
		CyberDraw.Ticks(drawList, new Vector2(start.X, ruleY), new Vector2(start.X + width, ruleY + 4f), 21,
			UiTheme.Alpha(UiTheme.Border, 0.75f), 3f, 0);
		ImGui.Dummy(new Vector2(width, barHeight + 8f));

		DrawDescription(description);
		ImGui.Spacing();
	}

	
	
	

	public static bool TabButton(string id, string label, bool selected, float width = 0f)
	{
		const float height = 34f;
		ImGui.PushID(id);
		float textWidth = ImGui.CalcTextSize(label).X;
		float tabWidth = width > 0f ? width : MathF.Max(64f, textWidth + 46f);
		Vector2 start = ImGui.GetCursorScreenPos();
		ImGui.InvisibleButton("##tab", new Vector2(tabWidth, height));
		bool hovered = ImGui.IsItemHovered();
		bool clicked = ImGui.IsItemClicked();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 end = start + new Vector2(tabWidth, height);
		CutCorners corners = CutCorners.Diagonal;

		Vector4 fill = selected ? UiTheme.Well : (hovered ? UiTheme.SurfaceHover : UiTheme.CardBackground);
		CyberDraw.Fill(drawList, start, end, fill, UiTheme.CutSmall, corners);
		if (selected)
		{
			CyberDraw.Glow(drawList, start, end, UiTheme.GlowSoft, UiTheme.CutSmall, corners, 2.5f, 0.9f);
		}
		CyberDraw.Line(drawList, start, end, selected ? UiTheme.Accent : UiTheme.Border, UiTheme.CutSmall, corners, 1f);

		if (selected)
		{
			
			Vector2 underlineMin = new Vector2(start.X + 10f, end.Y - 3f);
			Vector2 underlineMax = new Vector2(end.X - 10f, end.Y);
			CyberDraw.Fill(drawList, underlineMin, underlineMax, UiTheme.Accent, 0f, CutCorners.None);
			CyberDraw.Glow(drawList, underlineMin, underlineMax, UiTheme.Glow, 0f, CutCorners.None, 2.5f, 0.7f);
		}

		
		TextScaled(drawList, new Vector2(start.X + 5f, start.Y + 3f), TabIndex(label),
			selected ? UiTheme.Accent : UiTheme.Micro, 0.76f, 1.1f);

		Vector2 textAreaMin = new Vector2(start.X + 20f, start.Y);
		Vector2 textAreaMax = new Vector2(end.X - 6f, end.Y);
		TextScaledCentered(drawList, textAreaMin, textAreaMax, label,
			selected ? UiTheme.Accent : (hovered ? UiTheme.Text : UiTheme.TextMuted), 1f, 0f);

		ImGui.PopID();
		return clicked;
	}

	
	
	

	
	public static bool ToggleRow(string id, string label, string description, ref bool value)
	{
		ImGui.PushID(id);
		float width = MathF.Max(80f, ImGui.GetContentRegionAvail().X);
		float height = MathF.Max(26f, ImGui.GetFrameHeight());
		Vector2 start = ImGui.GetCursorScreenPos();
		ImGui.InvisibleButton("##row", new Vector2(width, height));
		bool hovered = ImGui.IsItemHovered();
		bool changed = false;
		if (ImGui.IsItemClicked())
		{
			value = !value;
			changed = true;
		}

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 end = start + new Vector2(width, height);
		if (hovered)
		{
			CyberDraw.Fill(drawList, start, end, UiTheme.SurfaceHover, UiTheme.CutSmall, CutCorners.Diagonal);
		}

		
		string status = value ? "[ ON ]" : "[ OFF ]";
		Vector2 statusSize = MeasureScaled(status, MicroScale, 1.3f);
		float statusX = end.X - statusSize.X - 2f;
		TextScaled(drawList, new Vector2(statusX, start.Y + (height - statusSize.Y) * 0.5f), status,
			value ? UiTheme.Accent : UiTheme.Micro, MicroScale, 1.3f);

		const float trackWidth = 40f;
		float trackHeight = 18f;
		float trackRight = statusX - 10f;
		float trackLeft = trackRight - trackWidth;
		float trackTop = start.Y + (height - trackHeight) * 0.5f;
		Vector2 trackMin = new Vector2(trackLeft, trackTop);
		Vector2 trackMax = new Vector2(trackRight, trackTop + trackHeight);

		CyberDraw.Fill(drawList, trackMin, trackMax, UiTheme.Dim, UiTheme.CutSmall, CutCorners.Diagonal);
		
		CyberDraw.Stripes(drawList, new Vector2(trackMin.X + 2f, trackMin.Y + 2f),
			new Vector2(trackMax.X - 2f, trackMax.Y - 2f), UiTheme.Alpha(UiTheme.Border, 0.45f), 5f, 1.4f);
		CyberDraw.Line(drawList, trackMin, trackMax, value ? UiTheme.Accent : UiTheme.Border, UiTheme.CutSmall,
			CutCorners.Diagonal, 1f);

		float knobWidth = trackWidth * 0.46f;
		float knobX = value ? trackMax.X - knobWidth - 2f : trackMin.X + 2f;
		Vector2 knobMin = new Vector2(knobX, trackMin.Y + 2f);
		Vector2 knobMax = new Vector2(knobX + knobWidth, trackMax.Y - 2f);
		CyberDraw.Fill(drawList, knobMin, knobMax, value ? UiTheme.Accent : UiTheme.BorderStrong, 2f,
			CutCorners.Diagonal);
		if (value)
		{
			CyberDraw.Glow(drawList, knobMin, knobMax, UiTheme.Glow, 2f, CutCorners.Diagonal, 2.6f, 0.9f);
		}

		CyberDraw.Text(drawList, new Vector2(start.X + 8f, start.Y + (height - ImGui.GetTextLineHeight()) * 0.5f),
			label, value || hovered ? UiTheme.Text : UiTheme.Mix(UiTheme.Text, UiTheme.TextMuted, 0.35f));

		ImGui.PopID();
		DrawDescription(description);
		return changed;
	}

	
	public static bool Checkbox(string id, string label, ref bool value)
	{
		ImGui.PushID(id);
		const float box = 18f;
		float height = MathF.Max(box + 2f, ImGui.GetFrameHeight());
		float textWidth = ImGui.CalcTextSize(label).X;
		Vector2 start = ImGui.GetCursorScreenPos();
		ImGui.InvisibleButton("##check", new Vector2(box + 8f + textWidth + 2f, height));
		bool hovered = ImGui.IsItemHovered();
		bool changed = false;
		if (ImGui.IsItemClicked())
		{
			value = !value;
			changed = true;
		}

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 boxMin = new Vector2(start.X, start.Y + (height - box) * 0.5f);
		Vector2 boxMax = boxMin + new Vector2(box, box);
		CyberDraw.Fill(drawList, boxMin, boxMax, UiTheme.Alpha(UiTheme.CardBackground, 0.9f), UiTheme.CutSmall,
			CutCorners.Diagonal);
		CyberDraw.Line(drawList, boxMin, boxMax, hovered ? UiTheme.Accent : UiTheme.BorderStrong,
			UiTheme.CutSmall, CutCorners.Diagonal, 1f);
		if (value)
		{
			Vector2 innerMin = boxMin + new Vector2(3f);
			Vector2 innerMax = boxMax - new Vector2(3f);
			CyberDraw.Fill(drawList, innerMin, innerMax, UiTheme.Accent, 2f, CutCorners.Diagonal);
			CyberDraw.Glow(drawList, innerMin, innerMax, UiTheme.Glow, 2f, CutCorners.Diagonal, 2.6f, 0.9f);
		}

		CyberDraw.Text(drawList, new Vector2(boxMax.X + 8f, start.Y + (height - ImGui.GetTextLineHeight()) * 0.5f),
			label, value ? UiTheme.Accent : (hovered ? UiTheme.Text : UiTheme.TextMuted));

		ImGui.PopID();
		return changed;
	}

	
	
	
	
	internal static bool ModeSwitch(string id, bool daily, ref ModeSwitchAnim anim)
	{
		if (!anim.Initialized)
		{
			anim.KnobT = daily ? 1f : 0f;
			anim.Initialized = true;
		}

		float fontSize = ImGui.GetFontSize();
		float trackHeight = MathF.Max(26f, fontSize * 2f);
		float inset = trackHeight * 0.1f;
		float knobHeight = trackHeight - inset * 2f;
		string knobLabel = daily ? "日常" : "高难";
		string status = daily ? "[ MODE: DAILY ]" : "[ MODE: HARD ]";
		Vector2 statusSize = MeasureScaled(status, MicroScale, 1.3f);
		float gap = 12f;

		float knobWidth = ImGui.CalcTextSize(knobLabel).X + fontSize * 1.2f;
		float trackWidth = knobWidth * 2f + inset * 3f;
		float avail = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
		
		float maxTrack = MathF.Max(72f, avail - gap - statusSize.X);
		if (trackWidth > maxTrack)
		{
			trackWidth = maxTrack;
			knobWidth = MathF.Max(knobHeight, (trackWidth - inset * 3f) * 0.5f);
		}

		float rowWidth = trackWidth + gap + statusSize.X;
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0f, (avail - rowWidth) * 0.5f));
		ImGui.InvisibleButton(id, new Vector2(rowWidth, trackHeight));
		bool clicked = ImGui.IsItemClicked();
		bool hovered = ImGui.IsItemHovered();

		float target = daily ? 1f : 0f;
		float lerp = MathF.Min(1f, ImGui.GetIO().DeltaTime * 14f);
		anim.KnobT += (target - anim.KnobT) * lerp;
		if (MathF.Abs(target - anim.KnobT) < 0.001f)
		{
			anim.KnobT = target;
		}

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 start = ImGui.GetItemRectMin();
		Vector2 trackMin = start;
		Vector2 trackMax = start + new Vector2(trackWidth, trackHeight);

		CyberDraw.Fill(drawList, trackMin, trackMax, UiTheme.Dim, UiTheme.CutSmall, CutCorners.Diagonal);
		CyberDraw.Stripes(drawList, trackMin + new Vector2(2f), trackMax - new Vector2(2f),
			UiTheme.Alpha(UiTheme.Border, 0.45f), 6f, 1.6f);
		CyberDraw.Line(drawList, trackMin, trackMax, hovered ? UiTheme.BorderStrong : UiTheme.Border,
			UiTheme.CutSmall, CutCorners.Diagonal, 1f);
		
		CyberDraw.Rule(drawList, new Vector2(trackMin.X + trackWidth * 0.5f, trackMin.Y + 4f),
			new Vector2(trackMin.X + trackWidth * 0.5f, trackMax.Y - 4f), UiTheme.Alpha(UiTheme.Border, 0.7f));

		float knobTravel = trackWidth - inset * 2f - knobWidth;
		float knobX = trackMin.X + inset + knobTravel * Math.Clamp(anim.KnobT, 0f, 1f);
		Vector2 knobMin = new Vector2(knobX, trackMin.Y + inset);
		Vector2 knobMax = new Vector2(knobX + knobWidth, knobMin.Y + knobHeight);
		CyberDraw.Fill(drawList, knobMin, knobMax, UiTheme.Accent, 2f, CutCorners.Diagonal);
		CyberDraw.Glow(drawList, knobMin, knobMax, UiTheme.Glow, 2f, CutCorners.Diagonal, 3f, 0.9f);
		
		TextScaledCentered(drawList, knobMin, knobMax, knobLabel, UiTheme.Ink, 1f, 0f);

		float statusX = trackMax.X + gap;
		TextScaled(drawList, new Vector2(statusX, start.Y + (trackHeight - statusSize.Y) * 0.5f), status,
			daily ? UiTheme.Accent : UiTheme.Danger, MicroScale, 1.3f);

		ImGui.Dummy(new Vector2(0f, 6f));
		return clicked;
	}

	
	
	

	
	public static bool ComboRow(string id, string label, string description, ref int selectedIndex,
		IReadOnlyList<string> options, float controlWidth = 180f)
	{
		ImGui.PushID(id);
		float width = MathF.Max(120f, ImGui.GetContentRegionAvail().X);
		float rowStartX = ImGui.GetCursorPosX();
		float control = MathF.Min(controlWidth, MathF.Max(80f, width * 0.6f));
		float boxHeight = MathF.Max(24f, ImGui.GetFrameHeight());
		bool changed = false;
		int index = options.Count == 0 ? 0 : Math.Clamp(selectedIndex, 0, options.Count - 1);
		string preview = options.Count == 0 ? "—" : options[index];

		
		
		Vector2 rowOrigin = ImGui.GetCursorScreenPos();
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		if (ImGui.IsMouseHoveringRect(rowOrigin, rowOrigin + new Vector2(width, boxHeight)))
		{
			CyberDraw.Fill(drawList, rowOrigin, rowOrigin + new Vector2(width, boxHeight), UiTheme.SurfaceHover,
				UiTheme.CutSmall, CutCorners.Diagonal);
		}

		ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Text);
		ImGui.AlignTextToFramePadding();
		ImGui.TextUnformatted(label);
		ImGui.PopStyleColor();
		ImGui.SameLine();
		ImGui.SetCursorPosX(rowStartX + width - control);

		Vector2 boxMin = ImGui.GetCursorScreenPos();
		ImGui.InvisibleButton("##combo", new Vector2(control, boxHeight));
		bool hovered = ImGui.IsItemHovered();
		bool clicked = ImGui.IsItemClicked();
		Vector2 boxMax = boxMin + new Vector2(control, boxHeight);

		if (clicked && options.Count > 0)
		{
			ImGui.OpenPopup("##shell_combo", ImGuiPopupFlags.None);
		}

		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4f, 4f));
		ImGui.SetNextWindowSizeConstraints(new Vector2(control, 0f), new Vector2(control, float.MaxValue));
		bool popupOpen = ImGui.BeginPopup("##shell_combo", ImGuiWindowFlags.None);
		if (popupOpen)
		{
			
			ImDrawListPtr popupDrawList = ImGui.GetWindowDrawList();
			float optionHeight = MathF.Max(24f, ImGui.GetFrameHeight());
			
			ImGui.PushStyleColor(ImGuiCol.Header, Vector4.Zero);
			ImGui.PushStyleColor(ImGuiCol.HeaderHovered, Vector4.Zero);
			ImGui.PushStyleColor(ImGuiCol.HeaderActive, Vector4.Zero);
			for (int i = 0; i < options.Count; i++)
			{
				bool isSelected = i == index;
				if (ImGui.Selectable("##opt" + i, isSelected, ImGuiSelectableFlags.None,
						new Vector2(0f, optionHeight)))
				{
					index = i;
					changed = true;
				}
				bool optionHovered = ImGui.IsItemHovered();
				Vector2 optionMin = ImGui.GetItemRectMin();
				Vector2 optionMax = ImGui.GetItemRectMax();
				if (isSelected)
				{
					CyberDraw.Fill(popupDrawList, optionMin, optionMax, UiTheme.Well, 0f, CutCorners.None);
					CyberDraw.LightBar(popupDrawList, optionMin, optionMax, UiTheme.Accent, 5f);
				}
				else if (optionHovered)
				{
					CyberDraw.Fill(popupDrawList, optionMin, optionMax, UiTheme.SurfaceHover, 0f, CutCorners.None);
				}
				CyberDraw.Text(popupDrawList,
					new Vector2(optionMin.X + 12f, optionMin.Y + (optionHeight - ImGui.GetTextLineHeight()) * 0.5f),
					options[i], isSelected ? UiTheme.Accent : (optionHovered ? UiTheme.Text : UiTheme.TextMuted));
				CyberDraw.Rule(popupDrawList, new Vector2(optionMin.X + 4f, optionMax.Y),
					new Vector2(optionMax.X - 4f, optionMax.Y), UiTheme.Alpha(UiTheme.Border, 0.35f));
				if (isSelected)
				{
					ImGui.SetItemDefaultFocus();
				}
			}
			ImGui.PopStyleColor(3);
			ImGui.EndPopup();
		}
		
		ImGui.PopStyleVar();

		
		
		drawList = ImGui.GetWindowDrawList();
		CyberDraw.Fill(drawList, boxMin, boxMax, popupOpen ? UiTheme.Well : UiTheme.CardBackground, UiTheme.CutSmall,
			CutCorners.Diagonal);
		CyberDraw.Line(drawList, boxMin, boxMax, popupOpen || hovered ? UiTheme.Accent : UiTheme.Border,
			UiTheme.CutSmall, CutCorners.Diagonal, 1f);
		
		CyberDraw.Fill(drawList, new Vector2(boxMin.X + 2f, boxMin.Y + 5f),
			new Vector2(boxMin.X + 4f, boxMax.Y - 5f), popupOpen ? UiTheme.Accent : UiTheme.Border);

		float textX = boxMin.X + 10f;
		drawList.PushClipRect(new Vector2(textX, boxMin.Y), new Vector2(boxMax.X - 22f, boxMax.Y), true);
		CyberDraw.Text(drawList, new Vector2(textX, boxMin.Y + (boxHeight - ImGui.GetTextLineHeight()) * 0.5f), preview,
			options.Count == 0 ? UiTheme.TextMuted : UiTheme.Text);
		drawList.PopClipRect();

		CyberDraw.Chevron(drawList, new Vector2(boxMax.X - 18f, boxMin.Y + boxHeight * 0.5f - 4f), 5f, 8f,
			popupOpen || hovered ? UiTheme.Accent : UiTheme.BorderStrong, 1.2f, 3f);

		if (changed)
		{
			selectedIndex = index;
		}
		ImGui.PopID();
		DrawDescription(description);
		return changed;
	}

	
	public static bool InputIntRow(string id, string label, string description, ref int value, int step = 1,
		int fastStep = 10, float controlWidth = 180f)
	{
		ImGui.PushID(id);
		float width = MathF.Max(140f, ImGui.GetContentRegionAvail().X);
		float rowStartX = ImGui.GetCursorPosX();
		float control = MathF.Min(controlWidth, MathF.Max(96f, width * 0.6f));
		float frameHeight = MathF.Max(24f, ImGui.GetFrameHeight());
		float button = Math.Clamp(control * 0.16f, 16f, 24f);
		float gap = 4f;
		float field = MathF.Max(36f, control - (button + gap) * 2f);
		int normal = step != 0 ? step : 1;
		int fast = fastStep != 0 ? fastStep : normal;
		int delta = ImGui.GetIO().KeyCtrl ? fast : normal;
		bool changed = false;

		ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Text);
		ImGui.AlignTextToFramePadding();
		ImGui.TextUnformatted(label);
		ImGui.PopStyleColor();
		ImGui.SameLine();

		float controlStart = rowStartX + width - control;
		ImGui.SetCursorPosX(controlStart);
		if (GlyphButton("##minus", new Vector2(button, frameHeight), false))
		{
			value -= delta;
			changed = true;
		}

		
		ImGui.SameLine();
		ImGui.SetCursorPosX(controlStart + button + gap + field + gap);
		if (GlyphButton("##plus", new Vector2(button, frameHeight), true))
		{
			value += delta;
			changed = true;
		}

		ImGui.SameLine();
		ImGui.SetCursorPosX(controlStart + button + gap);
		Vector2 fieldMin = ImGui.GetCursorScreenPos();
		Vector2 fieldMax = fieldMin + new Vector2(field, frameHeight);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		
		CyberDraw.Fill(drawList, fieldMin, fieldMax, UiTheme.CardBackground, UiTheme.CutSmall, CutCorners.Diagonal);
		ImGui.PushStyleColor(ImGuiCol.FrameBg, Vector4.Zero);
		ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, Vector4.Zero);
		ImGui.PushStyleColor(ImGuiCol.FrameBgActive, Vector4.Zero);
		ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Accent);
		ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
		ImGui.SetNextItemWidth(field);
		if (ImGui.InputInt("##value", ref value, 0, 0))
		{
			changed = true;
		}
		ImGui.PopStyleVar();
		ImGui.PopStyleColor(4);
		bool fieldHovered = ImGui.IsItemHovered();
		bool fieldActive = ImGui.IsItemActive();
		CyberDraw.Line(drawList, fieldMin, fieldMax,
			fieldActive ? UiTheme.Accent : (fieldHovered ? UiTheme.BorderStrong : UiTheme.Border), UiTheme.CutSmall,
			CutCorners.Diagonal, 1f);

		ImGui.PopID();
		DrawDescription(description);
		return changed;
	}

	
	
	
	
	
	public static bool SliderRow(string id, string label, string description, ref float value, float minimum,
		float maximum, string format = "%.1f", float controlWidth = 180f)
	{
		ImGui.PushID(id);
		float width = MathF.Max(140f, ImGui.GetContentRegionAvail().X);
		float rowStartX = ImGui.GetCursorPosX();
		float control = MathF.Min(controlWidth, MathF.Max(96f, width * 0.6f));
		float frameHeight = MathF.Max(22f, ImGui.GetFrameHeight());
		float valueWidth = Math.Clamp(control * 0.34f, 42f, 72f);
		float barWidth = MathF.Max(30f, control - valueWidth - 8f);

		ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.Text);
		ImGui.AlignTextToFramePadding();
		ImGui.TextUnformatted(label);
		ImGui.PopStyleColor();
		ImGui.SameLine();
		ImGui.SetCursorPosX(rowStartX + width - control);

		ImGui.InvisibleButton("##bar", new Vector2(barWidth, frameHeight));
		bool hovered = ImGui.IsItemHovered();
		bool active = ImGui.IsItemActive();
		Vector2 itemMin = ImGui.GetItemRectMin();
		float before = value;
		if (active && ImGui.IsMouseDown(ImGuiMouseButton.Left))
		{
			float ratio = Math.Clamp((ImGui.GetMousePos().X - itemMin.X) / MathF.Max(1f, barWidth), 0f, 1f);
			value = minimum + (maximum - minimum) * ratio;
		}
		if (hovered)
		{
			ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
		}

		float amount = maximum <= minimum ? 0f : Math.Clamp((value - minimum) / (maximum - minimum), 0f, 1f);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		float barHeight = MathF.Min(14f, frameHeight - 6f);
		Vector2 barMin = new Vector2(itemMin.X, itemMin.Y + (frameHeight - barHeight) * 0.5f);
		Vector2 barMax = new Vector2(itemMin.X + barWidth, barMin.Y + barHeight);
		CyberDraw.Segments(drawList, barMin, barMax, 12, amount, UiTheme.Accent, UiTheme.Dim,
			UiTheme.CutSmall * 0.6f, 2f, true);
		CyberDraw.Ticks(drawList, new Vector2(barMin.X, barMax.Y + 2f), new Vector2(barMax.X, barMax.Y + 5f), 13,
			UiTheme.Alpha(UiTheme.Border, 0.7f), 3f, 0);

		
		float valueAreaRight = itemMin.X + barWidth + 8f + valueWidth;
		Vector2 plateMin = new Vector2(valueAreaRight - valueWidth, itemMin.Y + 2f);
		Vector2 plateMax = new Vector2(valueAreaRight, itemMin.Y + frameHeight - 2f);
		CyberDraw.Fill(drawList, plateMin, plateMax, UiTheme.Alpha(UiTheme.Well, 0.85f), UiTheme.CutSmall * 0.7f,
			CutCorners.Diagonal);
		CyberDraw.Line(drawList, plateMin, plateMax, UiTheme.Alpha(UiTheme.Border, 0.9f), UiTheme.CutSmall * 0.7f,
			CutCorners.Diagonal, 1f);

		string readout = FormatValue(value, format);
		float readoutScale = ValueScale;
		Vector2 readoutSize = MeasureScaled(readout, readoutScale, 1.2f);
		
		float readoutRoom = valueWidth - 8f;
		if (readoutSize.X > readoutRoom && readoutSize.X > 0.01f)
		{
			readoutScale *= readoutRoom / readoutSize.X;
			readoutSize = MeasureScaled(readout, readoutScale, 1.2f);
		}
		TextScaled(drawList,
			new Vector2(valueAreaRight - 6f - readoutSize.X, itemMin.Y + (frameHeight - readoutSize.Y) * 0.5f),
			readout, active || hovered ? UiTheme.Accent : UiTheme.AccentStrong, readoutScale, 1.2f);

		ImGui.PopID();
		DrawDescription(description);
		return MathF.Abs(before - value) > 0.0001f;
	}

	
	
	

	
	public static bool PrimaryButton(string id, string label, Vector2 size)
	{
		bool clicked = ButtonItem(id, size, out Vector2 min, out Vector2 max, out bool hovered);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector4 fill = hovered ? UiTheme.Mix(UiTheme.Danger, UiTheme.Text, 0.10f) : UiTheme.Danger;

		CyberDraw.Fill(drawList, min, max, fill, UiTheme.Cut, CutCorners.Diagonal);
		CyberDraw.Stripes(drawList, min + new Vector2(1f), max - new Vector2(1f), UiTheme.Alpha(UiTheme.Ink, 0.12f),
			10f, 5f);
		CyberDraw.Line(drawList, min, max, UiTheme.Alpha(UiTheme.Ink, 0.55f), UiTheme.Cut, CutCorners.Diagonal, 1f);
		if (hovered)
		{
			CyberDraw.Glow(drawList, min, max, UiTheme.Alpha(UiTheme.Danger, 0.55f), UiTheme.Cut,
				CutCorners.Diagonal, 3.5f, 1f);
		}

		
		CyberDraw.Warning(drawList, new Vector2(min.X + 15f, (min.Y + max.Y) * 0.5f), 12f, UiTheme.Ink, 1.6f);
		TextScaledCentered(drawList, new Vector2(min.X + 28f, min.Y), new Vector2(max.X - 10f, max.Y),
			label.ToUpperInvariant(), UiTheme.Ink, 1f, 1.3f, true);
		return clicked;
	}

	
	public static void StatusPill(string id, string label, Vector4 color)
	{
		ImGui.PushID(id);
		string text = label.ToUpperInvariant();
		Vector2 textSize = MeasureScaled(text, 0.92f, 0.9f);
		Vector2 size = new Vector2(textSize.X + 24f, MathF.Max(20f, textSize.Y + 8f));
		Vector2 start = ImGui.GetCursorScreenPos();
		ImGui.Dummy(size);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 end = start + size;

		CyberDraw.Fill(drawList, start, end, UiTheme.Mix(UiTheme.CardBackground, color, 0.14f), UiTheme.CutSmall,
			CutCorners.Diagonal);
		CyberDraw.Line(drawList, start, end, UiTheme.Alpha(color, 0.55f), UiTheme.CutSmall, CutCorners.Diagonal, 1f);
		Vector2 barMin = new Vector2(start.X + 2f, start.Y + 4f);
		Vector2 barMax = new Vector2(start.X + 4f, end.Y - 4f);
		CyberDraw.Fill(drawList, barMin, barMax, color);
		CyberDraw.Glow(drawList, barMin, barMax, UiTheme.Alpha(color, 0.5f), 0f, CutCorners.None, 2.4f, 0.9f);
		TextScaled(drawList, new Vector2(start.X + 12f, start.Y + (size.Y - textSize.Y) * 0.5f), text,
			UiTheme.Mix(UiTheme.Text, color, 0.35f), 0.92f, 0.9f);

		ImGui.PopID();
	}

	
	public static bool HazardBar(string id, string label, string code, Vector4 color, Vector2 size)
	{
		bool clicked = ButtonItem(id, size, out Vector2 min, out Vector2 max, out bool hovered);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector4 fill = hovered ? UiTheme.Mix(color, UiTheme.Text, 0.12f) : color;

		CyberDraw.Fill(drawList, min, max, fill, UiTheme.CutLarge, CutCorners.Diagonal);
		CyberDraw.Stripes(drawList, min + new Vector2(1f), max - new Vector2(1f), UiTheme.Alpha(UiTheme.Ink, 0.10f),
			12f, 6f);
		CyberDraw.Line(drawList, min, max, UiTheme.Alpha(UiTheme.Ink, 0.5f), UiTheme.CutLarge, CutCorners.Diagonal, 1f);
		if (hovered)
		{
			CyberDraw.Glow(drawList, min, max, UiTheme.Alpha(color, 0.6f), UiTheme.CutLarge, CutCorners.Diagonal, 3f, 1f);
		}

		CyberDraw.Warning(drawList, new Vector2(min.X + 15f, (min.Y + max.Y) * 0.5f), 11f, UiTheme.Ink, 1.6f);

		Vector2 codeSize = MeasureScaled(code.ToUpperInvariant(), MicroScale, 1.4f);
		float codeX = max.X - codeSize.X - 10f;
		TextScaled(drawList, new Vector2(codeX, min.Y + (size.Y - codeSize.Y) * 0.5f), code.ToUpperInvariant(),
			UiTheme.Alpha(UiTheme.Ink, 0.85f), MicroScale, 1.4f);

		float labelLeft = min.X + 26f;
		float labelRight = MathF.Max(labelLeft + 8f, codeX - 8f);
		TextScaledCentered(drawList, new Vector2(labelLeft, min.Y), new Vector2(labelRight, max.Y),
			label.ToUpperInvariant(), UiTheme.Ink, 1f, 1.3f, true);
		return clicked;
	}

	
	public static bool NumberedButton(string id, string index, string label, Vector2 size)
	{
		bool clicked = ButtonItem(id, size, out Vector2 min, out Vector2 max, out bool hovered);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();

		CyberDraw.Fill(drawList, min, max, hovered ? UiTheme.Well : UiTheme.CardBackground, UiTheme.Cut,
			CutCorners.Bottom);
		CyberDraw.Line(drawList, min, max, hovered ? UiTheme.Accent : UiTheme.Border, UiTheme.Cut, CutCorners.Bottom, 1f);
		if (hovered)
		{
			CyberDraw.Glow(drawList, min, max, UiTheme.Glow, UiTheme.Cut, CutCorners.Bottom, 3.5f, 0.9f);
		}

		float half = MathF.Min(size.X * 0.18f, 18f);
		float midX = (min.X + max.X) * 0.5f;
		CyberDraw.Rule(drawList, new Vector2(midX - half, min.Y + 4f), new Vector2(midX + half, min.Y + 4f),
			UiTheme.Accent, 2f);

		Vector2 indexSize = MeasureScaled(index, MicroScale, 1.3f);
		TextScaled(drawList, new Vector2(min.X + 9f, min.Y + (size.Y - indexSize.Y) * 0.5f), index,
			hovered ? UiTheme.Accent : UiTheme.Micro, MicroScale, 1.3f);
		TextScaledCentered(drawList, new Vector2(min.X + 26f, min.Y), new Vector2(max.X - 10f, max.Y),
			label.ToUpperInvariant(), hovered ? UiTheme.Accent : UiTheme.Text, 1f, 1.4f);
		return clicked;
	}

	
	public static bool IconButton(string id, string glyph, Vector2 size, bool danger = false)
	{
		bool clicked = ButtonItem(id, size, out Vector2 min, out Vector2 max, out bool hovered);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector4 accent = danger ? UiTheme.Danger : UiTheme.Accent;

		CyberDraw.Fill(drawList, min, max, hovered ? UiTheme.Well : UiTheme.CardBackground, UiTheme.CutSmall,
			CutCorners.Diagonal);
		CyberDraw.Line(drawList, min, max, hovered ? accent : (danger ? UiTheme.Alpha(UiTheme.Danger, 0.65f) : UiTheme.Border),
			UiTheme.CutSmall, CutCorners.Diagonal, 1f);
		if (hovered)
		{
			CyberDraw.Glow(drawList, min, max, UiTheme.Alpha(accent, 0.5f), UiTheme.CutSmall, CutCorners.Diagonal, 2.5f, 0.9f);
		}

		if (string.IsNullOrEmpty(glyph))
		{
			
			CyberDraw.Fill(drawList, new Vector2(min.X + size.X * 0.5f - 2f, min.Y + size.Y * 0.5f - 2f),
				new Vector2(min.X + size.X * 0.5f + 2f, min.Y + size.Y * 0.5f + 2f),
				hovered ? accent : UiTheme.Micro);
		}
		else
		{
			
			uint glyphColor = UiTheme.ToU32(hovered ? accent : (danger ? UiTheme.Danger : UiTheme.Text));
			Vector2 center = (min + max) * 0.5f;
			switch (glyph)
			{
				case "☰":
					for (int i = -1; i <= 1; i++)
					{
						float y = center.Y + i * 4f;
						drawList.AddLine(new Vector2(center.X - 5f, y), new Vector2(center.X + 5f, y), glyphColor, 1.4f);
					}
					break;
				case "⌄":
				case "v":
					drawList.AddLine(center + new Vector2(-5f, -2f), center + new Vector2(0f, 3f), glyphColor, 1.4f);
					drawList.AddLine(center + new Vector2(0f, 3f), center + new Vector2(5f, -2f), glyphColor, 1.4f);
					break;
				case "×":
				case "✕":
				case "x":
				case "X":
					drawList.AddLine(center + new Vector2(-4f, -4f), center + new Vector2(4f, 4f), glyphColor, 1.4f);
					drawList.AddLine(center + new Vector2(4f, -4f), center + new Vector2(-4f, 4f), glyphColor, 1.4f);
					break;
				default:
					CyberDraw.TextCentered(drawList, min, max, glyph, hovered ? accent : UiTheme.Text);
					break;
			}
		}
		return clicked;
	}

	
	
	

	
	private static bool ButtonItem(string id, Vector2 size, out Vector2 min, out Vector2 max, out bool hovered)
	{
		ImGui.PushID(id);
		ImGui.InvisibleButton("##item", size);
		hovered = ImGui.IsItemHovered();
		bool clicked = ImGui.IsItemClicked();
		min = ImGui.GetItemRectMin();
		max = ImGui.GetItemRectMax();
		ImGui.PopID();
		return clicked;
	}

	
	private static bool GlyphButton(string id, Vector2 size, bool plus)
	{
		ImGui.InvisibleButton(id, size);
		bool hovered = ImGui.IsItemHovered();
		bool clicked = ImGui.IsItemClicked();
		Vector2 min = ImGui.GetItemRectMin();
		Vector2 max = min + size;
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		CyberDraw.Fill(drawList, min, max, hovered ? UiTheme.Well : UiTheme.CardBackground, UiTheme.CutSmall,
			CutCorners.Diagonal);
		CyberDraw.Line(drawList, min, max, hovered ? UiTheme.Accent : UiTheme.Border, UiTheme.CutSmall,
			CutCorners.Diagonal, 1f);
		uint color = UiTheme.ToU32(hovered ? UiTheme.Accent : UiTheme.Text);
		Vector2 center = (min + max) * 0.5f;
		drawList.AddLine(center + new Vector2(-4f, 0f), center + new Vector2(4f, 0f), color, 1.4f);
		if (plus)
		{
			drawList.AddLine(center + new Vector2(0f, -4f), center + new Vector2(0f, 4f), color, 1.4f);
		}
		return clicked;
	}

	
	private static void TextScaled(ImDrawListPtr drawList, Vector2 position, string text, Vector4 color, float scale,
		float tracking = 0f)
	{
		if (string.IsNullOrEmpty(text) || color.W <= 0.001f)
		{
			return;
		}
		var font = ImGui.GetFont();
		float fontSize = ImGui.GetFontSize() * scale;
		uint col = UiTheme.ToU32(color);
		if (tracking <= 0.01f)
		{
			drawList.AddText(font, fontSize, position, col, text);
			return;
		}
		float x = position.X;
		for (int i = 0; i < text.Length; i++)
		{
			string part = text[i].ToString();
			drawList.AddText(font, fontSize, new Vector2(x, position.Y), col, part);
			x += ImGui.CalcTextSize(part).X * scale + tracking;
		}
	}

	
	private static void TextScaledCentered(ImDrawListPtr drawList, Vector2 min, Vector2 max, string text, Vector4 color,
		float scale, float tracking = 0f, bool bold = false)
	{
		Vector2 size = MeasureScaled(text, scale, tracking);
		float x = min.X + (max.X - min.X - size.X) * 0.5f;
		float y = min.Y + (max.Y - min.Y - size.Y) * 0.5f;
		TextScaled(drawList, new Vector2(x, y), text, color, scale, tracking);
		if (bold)
		{
			TextScaled(drawList, new Vector2(x + 0.6f, y), text, color, scale, tracking);
		}
	}

	private static Vector2 MeasureScaled(string text, float scale, float tracking = 0f)
	{
		if (string.IsNullOrEmpty(text))
		{
			return Vector2.Zero;
		}
		Vector2 size = ImGui.CalcTextSize(text);
		float width = size.X * scale + tracking * (text.Length - 1);
		return new Vector2(width, size.Y * scale);
	}

	
	
	
	
	private static string ProtocolCode(string seed)
	{
		if (ProtocolCache.TryGetValue(seed, out string? cached))
		{
			return cached;
		}
		uint hash = 2166136261u;
		for (int i = 0; i < seed.Length; i++)
		{
			hash ^= seed[i];
			hash *= 16777619u;
		}
		string code = "PROTOCOL " + (hash & 0xFFFFu).ToString("X4", CultureInfo.InvariantCulture) + "-J" +
			(0x10u + hash / 0x10000u % 0x60u).ToString("X2", CultureInfo.InvariantCulture);
		if (ProtocolCache.Count > 128)
		{
			
			ProtocolCache.Clear();
		}
		ProtocolCache[seed] = code;
		return code;
	}

	
	private static string TabIndex(string label)
	{
		if (TabOrder.TryGetValue(label, out int stored))
		{
			return stored.ToString("00", CultureInfo.InvariantCulture);
		}
		int next = Math.Clamp(TabOrder.Count + 1, 1, 99);
		if (TabOrder.Count < 99)
		{
			TabOrder[label] = next;
		}
		return next.ToString("00", CultureInfo.InvariantCulture);
	}

	
	private static void DrawDescription(string? description)
	{
		if (string.IsNullOrWhiteSpace(description))
		{
			return;
		}
		ImGui.PushStyleColor(ImGuiCol.Text, UiTheme.TextMuted);
		ImGui.TextWrapped(description!);
		ImGui.PopStyleColor();
	}

	
	
	
	
	
	private static string FormatValue(float value, string format)
	{
		string pattern = string.IsNullOrEmpty(format) ? "%.1f" : format;
		StringBuilder builder = new StringBuilder(pattern.Length + 8);
		bool replaced = false;
		int cursor = 0;
		while (cursor < pattern.Length)
		{
			char current = pattern[cursor];
			if (current != '%')
			{
				builder.Append(current);
				cursor++;
				continue;
			}
			if (cursor + 1 < pattern.Length && pattern[cursor + 1] == '%')
			{
				builder.Append('%');
				cursor += 2;
				continue;
			}

			int index = cursor + 1;
			bool plus = false;
			bool space = false;
			bool zeroPad = false;
			bool leftAlign = false;
			while (index < pattern.Length)
			{
				char flag = pattern[index];
				if (flag == '+')
				{
					plus = true;
					index++;
				}
				else if (flag == '-')
				{
					leftAlign = true;
					index++;
				}
				else if (flag == '0')
				{
					zeroPad = true;
					index++;
				}
				else if (flag == ' ')
				{
					space = true;
					index++;
				}
				else if (flag == '#')
				{
					index++;
				}
				else
				{
					break;
				}
			}

			int width = 0;
			while (index < pattern.Length && char.IsAsciiDigit(pattern[index]))
			{
				width = width * 10 + (pattern[index] - '0');
				index++;
			}

			int precision = -1;
			if (index < pattern.Length && pattern[index] == '.')
			{
				index++;
				precision = 0;
				while (index < pattern.Length && char.IsAsciiDigit(pattern[index]))
				{
					precision = precision * 10 + (pattern[index] - '0');
					index++;
				}
			}

			if (index >= pattern.Length)
			{
				
				builder.Append(pattern, cursor, pattern.Length - cursor);
				break;
			}

			char conversion = pattern[index];
			string text = FormatNumber(value, conversion, precision);
			if ((plus || space) && text.Length > 0 && text[0] != '-')
			{
				text = (plus ? "+" : " ") + text;
			}
			if (width > text.Length)
			{
				if (leftAlign)
				{
					text = text.PadRight(width);
				}
				else if (zeroPad)
				{
					int signLength = text.Length > 1 && (text[0] == '-' || text[0] == '+') ? 1 : 0;
					text = text.Substring(0, signLength) + text.Substring(signLength).PadLeft(width - signLength, '0');
				}
				else
				{
					text = text.PadLeft(width);
				}
			}

			builder.Append(text);
			replaced = true;
			cursor = index + 1;
		}

		return replaced ? builder.ToString() : pattern;
	}

	
	private static string FormatNumber(float value, char conversion, int precision)
	{
		switch (conversion)
		{
			case 'd':
			case 'i':
				return ((long)value).ToString(CultureInfo.InvariantCulture);
			case 'u':
				return ((ulong)MathF.Max(0f, value)).ToString(CultureInfo.InvariantCulture);
			case 'x':
				return ((long)value).ToString("x", CultureInfo.InvariantCulture);
			case 'X':
				return ((long)value).ToString("X", CultureInfo.InvariantCulture);
			case 'e':
				return value.ToString("E" + (precision < 0 ? 2 : precision), CultureInfo.InvariantCulture)
					.ToLowerInvariant();
			case 'E':
				return value.ToString("E" + (precision < 0 ? 2 : precision), CultureInfo.InvariantCulture);
			case 'g':
				return value.ToString("g" + (precision < 0 ? 6 : precision), CultureInfo.InvariantCulture);
			case 'G':
				return value.ToString("G" + (precision < 0 ? 6 : precision), CultureInfo.InvariantCulture);
			case 'f':
			case 'F':
				return value.ToString("F" + (precision < 0 ? 2 : precision), CultureInfo.InvariantCulture);
			default:
				return value.ToString("0.##", CultureInfo.InvariantCulture);
		}
	}
}
