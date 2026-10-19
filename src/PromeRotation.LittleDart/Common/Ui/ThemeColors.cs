using System;
using System.Numerics;
using PromeRotation.UI.Components;

namespace LittleDart.Common.Ui;

/// <summary>
/// 「日常 / 高难」模式开关的配色（GNB / PLD 共用）。
/// 来源：docs\31 T16 / docs\39 §6 批次 E；移植自 Gunbreaker\Setting\GnbSettingUi.cs 与
/// Paladin\Setting\PldSettingUi.cs 里逐字重复的两份调色板。
/// </summary>
/// <remarks>
/// <para><b>两条硬约束（T16 的根因都在这）</b></para>
/// <list type="number">
/// <item>颜色必须**每帧现算**（用属性 / 方法），禁止把所有值缓存成 <c>static readonly</c> 字段 ——
/// <c>PromeUiTheme</c> 的调色板是属性，值随 PR 配色方案（深色 / 浅色 / 浅色 Original）实时变；
/// 缓存后切主题不生效，只能重启插件。（这里只有「身份色」这类与主题无关的基色是 readonly。）</item>
/// <item>开关文字是画在**旋钮**上的，不是画在窗口底上。旋钮固定近白 (0.97)，所以文字 ink 固定深色，
/// 不能直接取 <c>PromeUiTheme.Text</c>（深色主题下 Text 是近白的，压近白旋钮 = 看不见）。</item>
/// </list>
/// <para>
/// ⚠️ 与 docs\39 §6 第 1 步「按 <c>IsLight</c> 分两套色板」的偏差（施工前已论证，见
/// scratch\research\batch-e-theme.md §4.3）：对比度的参照物是**写死近白的旋钮**，与窗口主题无关；
/// 且 <c>Original</c> 下 <c>IsLight</c> 要靠 Dalamud 原生底色反推。改成「身份色 → 固定深色 ink」混色后，
/// 深色 / 浅色 / 浅色 Original / 深色 Original 四种组合一次覆盖，且**深色模式零外观变化**。
/// 估算对比度：渐变终点 1.44:1 → 9.44:1（日常）、2.40:1 → 11.10:1（高难）。
/// </para>
/// </remarks>
internal static class ThemeColors
{
	// —— 身份色：与主题无关（中饱和，浅底 / 深底都能分辨）——
	private static readonly Vector4 DailyTrackColor = new Vector4(145f / 255f, 211f / 255f, 60f / 255f, 0.95f);

	private static readonly Vector4 HardTrackColor = new Vector4(214f / 255f, 69f / 255f, 69f / 255f, 0.95f);

	private static readonly Vector4 DailyLabelFrom = new Vector4(74f / 255f, 140f / 255f, 16f / 255f, 1f);

	private static readonly Vector4 DailyLabelTo = new Vector4(168f / 255f, 226f / 255f, 84f / 255f, 1f);

	private static readonly Vector4 HardLabelFrom = new Vector4(150f / 255f, 28f / 255f, 28f / 255f, 1f);

	private static readonly Vector4 HardLabelTo = new Vector4(246f / 255f, 126f / 255f, 126f / 255f, 1f);

	// —— 旋钮：保持原设计（固定近白）；ink 固定深色 ≈ (28,31,38) ——
	private static readonly Vector4 KnobFillColor = new Vector4(0.97f, 0.97f, 0.98f, 1f);

	private static readonly Vector4 KnobInk = new Vector4(0.11f, 0.12f, 0.15f, 1f);

	/// <summary>轨道底色。<paramref name="daily"/> = true 日常（绿）/ false 高难（红）。</summary>
	public static Vector4 Track(bool daily)
	{
		return daily ? DailyTrackColor : HardTrackColor;
	}

	/// <summary>悬停提亮：与原实现（逐通道 +0.06，Clamp 到 1）**数学完全一致**。</summary>
	public static Vector4 TrackHovered(bool daily)
	{
		Vector4 track = Track(daily);
		return new Vector4(MathF.Min(1f, track.X + 0.06f), MathF.Min(1f, track.Y + 0.06f), MathF.Min(1f, track.Z + 0.06f), track.W);
	}

	/// <summary>
	/// 轨道描边：轨道色混主题 <c>Text</c> —— 浅色下压深、深色下提亮，永远压在轨道上看得见。
	/// 原实现是写死的 <c>(0,0,0,0.18)</c>：深色下几乎看不见，浅色下偏脏。
	/// </summary>
	public static Vector4 TrackOutline(bool daily)
	{
		return PromeUiTheme.Mix(Track(daily), PromeUiTheme.Text, 0.45f);
	}

	/// <summary>旋钮填充（固定近白，与主题无关：文字 ink 也据此固定）。</summary>
	public static Vector4 Knob => KnobFillColor;

	/// <summary>旋钮投影（原值不变）。</summary>
	public static Vector4 KnobShadow => new Vector4(0f, 0f, 0f, 0.28f);

	/// <summary>旋钮文字渐变起点：身份色 → 深色 ink，60%（原对旋钮 3.90:1 ⇒ 8.73:1）。</summary>
	public static Vector4 KnobTextFrom(bool daily)
	{
		return Ink(daily ? DailyLabelFrom : HardLabelFrom, 0.6f);
	}

	/// <summary>旋钮文字渐变终点：身份色 → 深色 ink，80%（原对旋钮 1.44:1 ⇒ 9.44:1，主犯就是这一端）。</summary>
	public static Vector4 KnobTextTo(bool daily)
	{
		return Ink(daily ? DailyLabelTo : HardLabelTo, 0.8f);
	}

	/// <summary>
	/// 往固定深色 ink 拉。<c>EnsureTextContrast</c> 只是兜底：当前参数下伪亮度都 ≤ 0.52（恒等变换），
	/// 保留它是为了「以后有人把 ink 改浅」时不至于又画成看不见的浅字。
	/// </summary>
	private static Vector4 Ink(Vector4 identity, float amount)
	{
		return PromeUiTheme.EnsureTextContrast(PromeUiTheme.Mix(identity, KnobInk, amount));
	}
}
