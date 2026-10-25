using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace LittleDart.Common.Ui;

/// <summary>
/// 模式开关的动画状态。
/// <para>
/// ⚠️ **由调用方持有**（每个职业一份 <c>static</c> 字段），不要做成共享静态字段 ——
/// 原实现是 per-class static（<c>GnbSettingUi._modeKnobT</c> / <c>PldSettingUi._modeKnobT</c>），
/// 搬到 Common 后若共用一份，一个职业切模式另一个职业的旋钮会跟着动。
/// </para>
/// </summary>
internal struct ModeSwitchAnim
{
	public float KnobT;

	public bool Initialized;
}

/// <summary>
/// 「日常 / 高难」模式开关的绘制（GNB / PLD 共用，消灭两份逐字同构）。
/// 来源：docs\31 T16 / docs\39 §6 批次 E。
/// <para>
/// ★ 只负责「画」和「报告本帧是否被点击」；切模式的副作用（改设置 / 同步 QT / 存盘）
/// 留在各职业的 SettingUi 里 —— 判据与副作用各有单一真相源（docs\18 S1）。
/// </para>
/// <para>
/// ★ 几何、动画速度、逐字渐变算法与原实现逐行一致，只把颜色来源换成 <see cref="ThemeColors"/>。
/// </para>
/// </summary>
internal static class ThemeModeSwitch
{
	/// <summary>
	/// 画开关。返回 true = 本帧被点击（调用方据此切模式）。
	/// </summary>
	/// <param name="id">
	/// ImGui 控件 ID，**必须由调用方原样传原字符串**（<c>"###gnb_mode_switch"</c> / <c>"###pld_mode_switch"</c>）：
	/// 含 <c>###</c> 前缀的 ID 决定 item 状态（hover / active），改名会让悬停状态重置。
	/// </param>
	/// <param name="daily">当前是不是日常模式（判据在调用方）。</param>
	/// <param name="anim">调用方持有的动画状态。</param>
	public static bool Draw(string id, bool daily, ref ModeSwitchAnim anim)
	{
		if (!anim.Initialized)
		{
			anim.KnobT = (daily ? 1f : 0f);
			anim.Initialized = true;
		}
		float fontSize = ImGui.GetFontSize();
		string modeLabel = (daily ? "日常模式" : "高难模式");
		float labelWidth = ImGui.CalcTextSize(modeLabel, false, -1f).X;
		float knobPadding = fontSize * 0.55f;
		float trackHeight = fontSize * 2f;
		float trackInset = trackHeight * 0.1f;
		float knobHeight = trackHeight - trackInset * 2f;
		float knobWidth = labelWidth + knobPadding * 2f;
		float controlWidth = knobWidth * 2f + trackInset * 3f;
		float availWidth = ImGui.GetContentRegionAvail().X;
		if (controlWidth > availWidth && controlWidth > 0f)
		{
			float widthScale = availWidth / controlWidth;
			controlWidth *= widthScale;
			knobWidth = MathF.Max(knobHeight, knobWidth * widthScale);
		}
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (availWidth - controlWidth) * 0.5f);
		ImGui.InvisibleButton(id, new Vector2(controlWidth, trackHeight), (ImGuiButtonFlags)0);
		bool clicked = ImGui.IsItemClicked();
		float knobTarget = (daily ? 1f : 0f);
		ImGuiIOPtr io = ImGui.GetIO();
		float knobLerpSpeed = MathF.Min(1f, io.DeltaTime * 14f);
		anim.KnobT += (knobTarget - anim.KnobT) * knobLerpSpeed;
		if (MathF.Abs(knobTarget - anim.KnobT) < 0.001f)
		{
			anim.KnobT = knobTarget;
		}
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		Vector2 rectMin = ImGui.GetItemRectMin();
		Vector2 rectMax = new Vector2(rectMin.X + controlWidth, rectMin.Y + trackHeight);
		float trackRound = trackHeight * 0.5f;
		Vector4 trackColor = (ImGui.IsItemHovered() ? ThemeColors.TrackHovered(daily) : ThemeColors.Track(daily));
		drawList.AddRectFilled(rectMin, rectMax, ToU32(trackColor), trackRound);
		drawList.AddRect(rectMin, rectMax, ToU32(ThemeColors.TrackOutline(daily)), trackRound);
		float knobTrackLeft = rectMin.X + trackInset;
		float knobTrackRight = rectMax.X - trackInset - knobWidth;
		float knobX = knobTrackLeft + (knobTrackRight - knobTrackLeft) * anim.KnobT;
		Vector2 knobMin = new Vector2(knobX, rectMin.Y + trackInset);
		Vector2 knobMax = new Vector2(knobX + knobWidth, knobMin.Y + knobHeight);
		float knobRound = knobHeight * 0.5f;
		drawList.AddRectFilled(new Vector2(knobMin.X, knobMin.Y + 1f), new Vector2(knobMax.X, knobMax.Y + 1f), ToU32(ThemeColors.KnobShadow), knobRound);
		drawList.AddRectFilled(knobMin, knobMax, ToU32(ThemeColors.Knob), knobRound);
		Vector2 labelPos = new Vector2(knobMin.X + (knobWidth - labelWidth) * 0.5f, knobMin.Y + (knobHeight - fontSize) * 0.5f);
		Vector4 textColorFrom = ThemeColors.KnobTextFrom(daily);
		Vector4 textColorTo = ThemeColors.KnobTextTo(daily);
		float textCursorX = 0f;
		for (int i = 0; i < modeLabel.Length; i++)
		{
			string charText = modeLabel[i].ToString();
			float amount = ((modeLabel.Length <= 1) ? 0f : ((float)i / (float)(modeLabel.Length - 1)));
			drawList.AddText(new Vector2(labelPos.X + textCursorX, labelPos.Y), ToU32(Vector4.Lerp(textColorFrom, textColorTo, amount)), charText);
			textCursorX += ImGui.CalcTextSize(charText, false, -1f).X;
		}
		ImGui.Dummy(new Vector2(0f, 6f));
		return clicked;
	}

	/// <summary>颜色打包（与原实现逐字一致：ABGR，逐通道 Clamp 到 [0,1]）。</summary>
	private static uint ToU32(Vector4 c)
	{
		byte b = (byte)(Math.Clamp(c.X, 0f, 1f) * 255f);
		byte b2 = (byte)(Math.Clamp(c.Y, 0f, 1f) * 255f);
		byte b3 = (byte)(Math.Clamp(c.Z, 0f, 1f) * 255f);
		return (uint)(((byte)(Math.Clamp(c.W, 0f, 1f) * 255f) << 24) | (b3 << 16) | (b2 << 8) | b);
	}
}
