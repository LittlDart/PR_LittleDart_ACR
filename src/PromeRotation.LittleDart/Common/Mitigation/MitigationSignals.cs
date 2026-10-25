namespace LittleDart.Common.Mitigation;

/// <summary>
/// 减伤模块的「外部计时信号」接入口。
/// </summary>
/// <remarks>
/// <para>
/// ⚠️ <b>有意保留，不是漏删</b>（docs\21 §7.3 拍板：留作将来「Boss 倒计时锁减伤」的接入口；
/// docs\39 §7 F1 已复核）。现状：
/// </para>
/// <list type="bullet">
/// <item><see cref="BossTimer"/> 从未被赋值 ⇒ 恒为 <see cref="UnknownBossTimer"/>；</item>
/// <item><see cref="GetSecondsToBoss"/> / <see cref="SetManualSecondsToBoss"/> 目前 0 调用；</item>
/// <item><b>但 <see cref="ClearManual"/> 有 1 处真实调用</b>：<c>MitigationState.Reset()</c>，
/// 链路 <c>OnEnter/ExitRotation → MitigationScheduler.Reset → MitigationEngine.Reset → MitigationState.Reset</c>
/// 在运行时可达 ⇒ <b>不能按「整层 0 引用」把本文件删掉</b>（会 CS0103）。</item>
/// </list>
/// <para>
/// 若将来拍板「不做 Boss 倒计时」，整套可删：本文件 + <c>IBossTimer.cs</c> + <c>UnknownBossTimer.cs</c>
/// + <c>MitigationState.Reset()</c> 里那一行 <c>MitigationSignals.ClearManual()</c>。
/// </para>
/// </remarks>
public static class MitigationSignals
{
	public static IBossTimer BossTimer { get; set; } = new UnknownBossTimer();

	public static double? ManualSecondsToBoss { get; set; }

	public static double? GetSecondsToBoss()
	{
		return ManualSecondsToBoss ?? BossTimer.GetSecondsToBoss();
	}

	public static void ClearManual()
	{
		ManualSecondsToBoss = null;
	}

	public static void SetManualSecondsToBoss(double? seconds)
	{
		ManualSecondsToBoss = seconds;
	}
}
