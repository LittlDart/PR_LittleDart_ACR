using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using LittleDart.Common.Mitigation.Calc;

namespace LittleDart.Common.Mitigation;

public sealed class MitigationState
{
	private readonly MitigationJobProfile _profile;

	private readonly List<double> _pullIntervals = new List<double>();

	private readonly Dictionary<uint, long> _lastCastMs = new Dictionary<uint, long>();

	private long _lastEnemySampleMs;

	private long _lastInCombatMs;

	private long _lastWaveStartMs;

	private bool _waveEndedFlag;

	private long _pullMomentMs;

	private int _prevEnemyCount;

	public HpTracker Hp { get; }

	public bool WaveActive { get; private set; }

	public long WaveStartMs { get; private set; }

	public int EnemyCount { get; private set; }

	public int EnemiesIn5m { get; private set; }

	public int AggroOnMe { get; private set; }

	public long NowMs => Environment.TickCount64;

	public long WaveElapsedMs
	{
		get
		{
			if (!WaveActive)
			{
				return 0L;
			}
			return NowMs - WaveStartMs;
		}
	}

	public long SincePullMomentMs
	{
		get
		{
			if (!WaveActive)
			{
				return 0L;
			}
			return NowMs - Math.Max(_pullMomentMs, WaveStartMs);
		}
	}

	public IReadOnlyList<double> PullIntervals => _pullIntervals;

	public bool ResourceTight { get; set; }

	public float LastWaveDamagePercent { get; set; }

	private int CurrentDamageWindowMs => _profile.Thresholds.DamageWindowMs;

	private float CurrentEnemyRadius => _profile.Thresholds.EnemyCountRadius;

	private int CurrentWaveEndDebounceMs => _profile.Thresholds.WaveEndDebounceMs;

	public MitigationState(MitigationJobProfile profile)
	{
		_profile = profile;
		Hp = new HpTracker(profile.CurrentTarget);
	}

	public void Tick()
	{
		long nowMs = NowMs;
		Hp.Tick(nowMs, 200, CurrentDamageWindowMs);
		SampleEnemies(nowMs);
		if (IsInCombat())
		{
			_lastInCombatMs = nowMs;
			if (WaveActive)
			{
				return;
			}
			WaveActive = true;
			WaveStartMs = nowMs;
			_pullMomentMs = nowMs;
			_prevEnemyCount = 0;
			if (_lastWaveStartMs > 0)
			{
				double secondsSinceLastWave = (double)(nowMs - _lastWaveStartMs) / 1000.0;
				if (secondsSinceLastWave >= 3.0 && secondsSinceLastWave <= 180.0)
				{
					_pullIntervals.Add(secondsSinceLastWave);
					while (_pullIntervals.Count > 10)
					{
						_pullIntervals.RemoveAt(0);
					}
				}
			}
			_lastWaveStartMs = nowMs;
		}
		else if (WaveActive && nowMs - _lastInCombatMs >= CurrentWaveEndDebounceMs)
		{
			WaveActive = false;
			_waveEndedFlag = true;
			LastWaveDamagePercent = DamageTakenPercent(CurrentDamageWindowMs);
		}
	}

	public bool ConsumeWaveEnded()
	{
		bool waveEndedFlag = _waveEndedFlag;
		_waveEndedFlag = false;
		return waveEndedFlag;
	}

	public float DamageTakenPercent(int windowMs)
	{
		return Hp.SelfDamageTakenPercent(windowMs);
	}

	public void RecordCast(uint skillId)
	{
		_lastCastMs[skillId] = NowMs;
	}

	public long LastCastMs(uint skillId)
	{
		if (!_lastCastMs.TryGetValue(skillId, out var recordedMs))
		{
			return 0L;
		}
		return recordedMs;
	}

	public double EstimateSecondsToNextPull()
	{
		double seconds = _profile.Thresholds.DefaultNextPullSeconds;
		if (_pullIntervals.Count > 0)
		{
			int sampleCount = Math.Min(3, _pullIntervals.Count);
			double totalSeconds = 0.0;
			for (int i = _pullIntervals.Count - sampleCount; i < _pullIntervals.Count; i++)
			{
				totalSeconds += _pullIntervals[i];
			}
			seconds = totalSeconds / (double)sampleCount;
		}
		return Math.Max(0.0, seconds);
	}

	public void Reset()
	{
		Hp.Reset();
		_pullIntervals.Clear();
		_lastCastMs.Clear();
		_lastEnemySampleMs = 0L;
		_lastInCombatMs = 0L;
		_lastWaveStartMs = 0L;
		_waveEndedFlag = false;
		_pullMomentMs = 0L;
		_prevEnemyCount = 0;
		WaveActive = false;
		WaveStartMs = 0L;
		EnemyCount = 0;
		EnemiesIn5m = 0;
		AggroOnMe = 0;
		ResourceTight = false;
		LastWaveDamagePercent = 0f;
		MitigationSignals.ClearManual();
	}

	private void SampleEnemies(long now)
	{
		if (now - _lastEnemySampleMs >= 250)
		{
			_lastEnemySampleMs = now;
			EnemyCount = EnemyCounter.CountWithin(_profile, CurrentEnemyRadius);
			EnemiesIn5m = EnemyCounter.CountWithin(_profile, 5f);
			AggroOnMe = EnemyCounter.CountAggroOnMe(_profile);
			if (EnemyCount > _prevEnemyCount)
			{
				_pullMomentMs = now;
			}
			_prevEnemyCount = EnemyCount;
		}
	}

	public bool IsInCombat()
	{
		try
		{
			return Svc.Condition[(ConditionFlag)26];
		}
		catch
		{
			return EnemyCount > 0;
		}
	}
}
