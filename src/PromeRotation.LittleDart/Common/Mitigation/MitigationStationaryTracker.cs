using System;
using PromeRotation.Helpers;

namespace LittleDart.Common.Mitigation;

/// <summary>
/// 「原地停止满 N 毫秒」计时器 —— 减伤放行的前置闸门（2026-10-14 玩家拍板）。
///
/// <para><b>需求原话</b>：「只有玩家在原地停止超过一秒钟后才进行减伤技能的放行」。</para>
///
/// <para><b>为什么要有它</b>：跑位途中交减伤会被下面几种情况吃掉 —— 减伤是 oGCD，
/// 走位时插不进去 / 插进去但人已经离开了要吃的那个判定点。玩家宁可「走位中干脆不放」，
/// 也要保证减伤落在站定的那一格。</para>
///
/// <para><b>读数选择</b>：用 <see cref="MoveManager.IsLocalPlayerActuallyMoving"/>（「真的没动」），
/// 不是 <c>IsLocalPlayerMoving</c>（「没有移动输入」）—— 后者会把被击退 / 拉拽也算成「停」。
/// 这是玩家在两种读数里明确选的那个（见 docs\23 §11）。</para>
///
/// <para><b>读不到时的策略是「保持上一状态、不重置」</b>：<c>Core.Me</c> 还没就绪、
/// 或者插件在换图那一瞬读不到移动状态时，如果按「读不到 = 在动」处理，
/// 计时器会被反复清零、战斗中永远攒不满 1 秒 ⇒ 减伤彻底不出手。
/// 宁可保守地不给重置，也不要制造这种「静默死锁」。</para>
/// </summary>
public sealed class MitigationStationaryTracker
{
	/// <summary>需要连续站定的毫秒数。</summary>
	public const int RequiredStationaryMs = 1000;

	private readonly Func<bool?> _isMoving;
	private long _stationarySinceMs;
	private bool _ready;

	/// <summary>
	/// 是否至少读到过一次有效的移动状态（2026-10-16 新增）。
	///
	/// <para>⚠️ 这个字段是为了修一类**静默死锁**：职业插槽 <c>IsLocalPlayerMoving</c>
	/// 漏填时（默认返回 <c>null</c>）<see cref="Tick"/> 永远走「读不到」分支，
	/// <c>_ready</c> 保持初值 false ⇒ 闸门**永久关闭**、该职业一条减伤都不出手，
	/// 且没有任何报错。骑士（<c>PldMitigationProfile</c>）就是这样整机哑掉的。</para>
	///
	/// <para>契约（见 <c>MitigationJobProfile.IsLocalPlayerMoving</c> 的注释）：**读不到 ⇒ 不拦**。
	/// 所以「从未读到过」时 <see cref="IsReady"/> 为 true（放行）；读到过之后才按真实计时判。</para>
	/// </summary>
	private bool _hasReading;

	public MitigationStationaryTracker(Func<bool?> isMoving)
	{
		_isMoving = isMoving;
	}

	/// <summary>
	/// 当前是否**放行**：连续站定满 <see cref="RequiredStationaryMs"/>，
	/// 或者从来没读到过移动状态（契约「读不到 ⇒ 不拦」）。
	/// </summary>
	public bool IsReady => _ready || !_hasReading;

	/// <summary>是否读到过有效的移动状态 —— 给面板 / 诊断用（false = 这个职业插槽没接上）。</summary>
	public bool HasReading => _hasReading;

	/// <summary>已经连续站定了多久（毫秒）—— 给诊断文案用。</summary>
	public long StationaryMs
	{
		get
		{
			if (!_ready)
			{
				return 0L;
			}
			return Environment.TickCount64 - _stationarySinceMs;
		}
	}

	/// <summary>
	/// 每个战斗帧调一次。由 <see cref="MitigationEngine.Tick"/> 驱动，
	/// <b>不</b>放在 <c>Evaluate()</c> 里 —— <c>Evaluate()</c> 有 80ms 缓存，靠它计时会不准。
	/// </summary>
	public void Tick(long nowMs)
	{
		bool? moving = ReadMoving();
		if (!moving.HasValue)
		{
			// 读不到 ⇒ 保持现状，不重置（理由见类注释）。
			// ⚠️ 但「从未读到过」时 IsReady 仍为 true（契约「读不到 ⇒ 不拦」，见 _hasReading），
			//    否则插槽漏填的职业会被永久锁死 —— 骑士踩过这个坑。
			return;
		}
		_hasReading = true;
		if (moving.Value)
		{
			_ready = false;
			_stationarySinceMs = 0L;
			return;
		}
		if (_stationarySinceMs == 0L)
		{
			_stationarySinceMs = nowMs;
		}
		if (nowMs - _stationarySinceMs >= RequiredStationaryMs)
		{
			_ready = true;
		}
	}

	/// <summary>脱战 / 换图 / 停用时清零（构造里 <c>+=</c> 了什么，这里就 <c>-=</c> 什么那类对称要求不适用，但状态要清）。</summary>
	public void Reset()
	{
		_stationarySinceMs = 0L;
		_ready = false;
	}

	private bool? ReadMoving()
	{
		try
		{
			return _isMoving();
		}
		catch
		{
			return null;
		}
	}
}
