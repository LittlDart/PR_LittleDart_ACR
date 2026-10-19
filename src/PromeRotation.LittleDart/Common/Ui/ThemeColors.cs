using System;
using System.Numerics;
using PromeRotation.UI.Components;

namespace LittleDart.Common.Ui;

internal static class ThemeColors
{
	
	private static readonly Vector4 DailyTrackColor = new Vector4(145f / 255f, 211f / 255f, 60f / 255f, 0.95f);

	private static readonly Vector4 HardTrackColor = new Vector4(214f / 255f, 69f / 255f, 69f / 255f, 0.95f);

	private static readonly Vector4 DailyLabelFrom = new Vector4(74f / 255f, 140f / 255f, 16f / 255f, 1f);

	private static readonly Vector4 DailyLabelTo = new Vector4(168f / 255f, 226f / 255f, 84f / 255f, 1f);

	private static readonly Vector4 HardLabelFrom = new Vector4(150f / 255f, 28f / 255f, 28f / 255f, 1f);

	private static readonly Vector4 HardLabelTo = new Vector4(246f / 255f, 126f / 255f, 126f / 255f, 1f);

	
	private static readonly Vector4 KnobFillColor = new Vector4(0.97f, 0.97f, 0.98f, 1f);

	private static readonly Vector4 KnobInk = new Vector4(0.11f, 0.12f, 0.15f, 1f);

	
	public static Vector4 Track(bool daily)
	{
		return daily ? DailyTrackColor : HardTrackColor;
	}

	
	public static Vector4 TrackHovered(bool daily)
	{
		Vector4 track = Track(daily);
		return new Vector4(MathF.Min(1f, track.X + 0.06f), MathF.Min(1f, track.Y + 0.06f), MathF.Min(1f, track.Z + 0.06f), track.W);
	}

	
	
	
	
	public static Vector4 TrackOutline(bool daily)
	{
		return PromeUiTheme.Mix(Track(daily), PromeUiTheme.Text, 0.45f);
	}

	
	public static Vector4 Knob => KnobFillColor;

	
	public static Vector4 KnobShadow => new Vector4(0f, 0f, 0f, 0.28f);

	
	public static Vector4 KnobTextFrom(bool daily)
	{
		return Ink(daily ? DailyLabelFrom : HardLabelFrom, 0.6f);
	}

	
	public static Vector4 KnobTextTo(bool daily)
	{
		return Ink(daily ? DailyLabelTo : HardLabelTo, 0.8f);
	}

	
	
	
	
	private static Vector4 Ink(Vector4 identity, float amount)
	{
		return PromeUiTheme.EnsureTextContrast(PromeUiTheme.Mix(identity, KnobInk, amount));
	}
}
