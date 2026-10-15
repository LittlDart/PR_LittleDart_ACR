using System;
using PromeRotation.Helpers;

namespace LittleDart.Common.Mitigation;

public sealed class MitigationStationaryTracker
{
		public const int RequiredStationaryMs = 1000;

	private readonly Func<bool?> _isMoving;
	private long _stationarySinceMs;
	private bool _ready;

	public MitigationStationaryTracker(Func<bool?> isMoving)
	{
		_isMoving = isMoving;
	}

		public bool IsReady => _ready;

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

		public void Tick(long nowMs)
	{
		bool? moving = ReadMoving();
		if (!moving.HasValue)
		{

			return;
		}
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
