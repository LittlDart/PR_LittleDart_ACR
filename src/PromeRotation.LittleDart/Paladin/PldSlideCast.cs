using System;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;

namespace LittleDart.Paladin;

public static class PldSlideCast
{
	public enum SlideState
	{
		Idle,
		WaitingForPlayerToStop,
		PendingCast,
		Casting,
		Released
	}

	private static SlideState _state;

	private static long _requestTime;

	private static long _castStartTime;

	private static float _releaseRemainMs;

	private static bool _castBySlideCast;

	private static SlideState _lastDiagState;

	private static bool _lastDiagMoving;

	private const int PendingTimeoutMs = 1000;

	private const float MaxLockMsUnusedInPr = 5000f;

	private const string LogTag = "[LittleDart-PLD/滑步]";

	public static bool IsEnabled
	{
		get
		{
			if (PldSettings.Instance == null)
			{
				return false;
			}
			if (!PromeSettings.Instance.GetQt("滑步模式"))
			{
				return false;
			}
			return !PldHelper.IsDailyMode();
		}
	}

	public static bool CanSlideCast
	{
		get
		{
			if (IsEnabled)
			{
				return !PldHelper.IsMovingForCast();
			}
			return false;
		}
	}

	public static SlideState State => _state;

	public static float ReleaseThresholdMs => Math.Clamp(PldSettings.Instance?.SlideCastReleaseMs ?? 350, 0, 1000);

	public static void RequestCast()
	{
		if (_state == SlideState.Idle)
		{
			_state = SlideState.WaitingForPlayerToStop;
			_requestTime = Environment.TickCount64;
		}
	}

	public static void NotifyCasting()
	{
		_castBySlideCast = true;
		_castStartTime = Environment.TickCount64;
		SlideState state = _state;
		if ((uint)state <= 1u)
		{
			_state = SlideState.PendingCast;
		}
	}

	public static void Tick()
	{
		long tickCount = Environment.TickCount64;
		bool moving = PldHelper.IsMovingForCast();
		bool castingHolySpirit = IsCastingHolySpirit();
		if (!IsEnabled)
		{
			if (_state != SlideState.Idle)
			{
				Log("复位 原因=QT关或切日常（原状态 " + StateName(_state) + "）");
			}
			Reset();
			return;
		}
		switch (_state)
		{
		case SlideState.WaitingForPlayerToStop:
			if (castingHolySpirit)
			{
				_state = SlideState.Casting;
				_releaseRemainMs = RemainingMs();
				Log($"圣灵已起读条（剩余 {_releaseRemainMs:F0}ms）");
			}
			else if (tickCount - _requestTime > 1000)
			{
				Log($"放弃请求 超时={1000}ms（玩家始终没停下）");
				Reset();
			}
			break;
		case SlideState.PendingCast:
			if (castingHolySpirit)
			{
				_state = SlideState.Casting;
				_castStartTime = tickCount;
				_releaseRemainMs = RemainingMs();
				Log($"圣灵读条中（总时长 {_releaseRemainMs:F0}ms）");
			}
			else if (tickCount - _castStartTime > 1000)
			{
				Log($"技能未起读条 超时={1000}ms（疑似被移动或射程拒收）");
				Reset();
			}
			break;
		case SlideState.Casting:
			if (!castingHolySpirit)
			{
				LogCastEnd(tickCount);
				Reset();
			}
			break;
		}
		Diag(_state, moving, castingHolySpirit);
	}

	public static void OnDispose()
	{
		if (_state != SlideState.Idle)
		{
			Log("Dispose 复位（原状态 " + StateName(_state) + "）");
		}
		Reset();
	}

	private static void Reset()
	{
		_state = SlideState.Idle;
		_castBySlideCast = false;
		_castStartTime = 0L;
		_releaseRemainMs = 0f;
	}

	private static bool IsCastingHolySpirit()
	{
		IBattleChara me = Core.Me;
		if (me != null && me.IsCasting)
		{
			return me.CastActionId == 7384;
		}
		return false;
	}

	private static float RemainingMs()
	{
		IBattleChara me = Core.Me;
		if (me == null || !me.IsCasting)
		{
			return 0f;
		}
		return MathF.Max(0f, me.TotalCastTime - me.CurrentCastTime) * 1000f;
	}

	private static void LogCastEnd(long now)
	{
		long castElapsedMs = ((_castStartTime == 0L) ? 0 : (now - _castStartTime));
		Log(_castBySlideCast ? $"读条结束 源=滑步档 起始剩余={_releaseRemainMs:F0}ms 持续时间={castElapsedMs}ms" : $"读条结束 源=其它档 持续时间={castElapsedMs}ms");
	}

	private static void Log(string message)
	{
		if (Diagnostics.Enabled)
		{
			PldHelper.LogTagged("[LittleDart-PLD/滑步] " + message);
		}
	}

	private static void Diag(SlideState state, bool moving, bool castingHoly)
	{
		if (Diagnostics.Enabled && (state != _lastDiagState || moving != _lastDiagMoving))
		{
			_lastDiagState = state;
			_lastDiagMoving = moving;
			PldHelper.LogTagged($"{"[LittleDart-PLD/滑步]"} 状态={StateName(state)} 移动={moving} 圣灵读条={castingHoly} 剩余={RemainingMs():F0}ms 阈值={ReleaseThresholdMs:F0}ms");
		}
	}

	private static string StateName(SlideState state)
	{
		return state switch
		{
			SlideState.Idle => "空闲", 
			SlideState.WaitingForPlayerToStop => "等玩家站定", 
			SlideState.PendingCast => "已递出等读条", 
			SlideState.Casting => "读条中", 
			SlideState.Released => "已结束", 
			_ => state.ToString(), 
		};
	}

	public static string DescribeState()
	{
		return $"滑步模式：启用={IsEnabled} 可出手={CanSlideCast} 状态={StateName(_state)} 移动={PldHelper.IsMovingForCast()} 放行阈值={ReleaseThresholdMs:F0}ms（PR 版不参与决策）";
	}
}
