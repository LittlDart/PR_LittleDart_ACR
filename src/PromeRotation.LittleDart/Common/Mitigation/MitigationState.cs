using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using LittleDart.Common.Mitigation.Calc;

namespace LittleDart.Common.Mitigation;

public sealed class MitigationState
{
	private readonly MitigationJobProfile _profile;

	private readonly Dictionary<uint, long> _lastCastMs = new Dictionary<uint, long>();

	private long _lastEnemySampleMs;

	private long _lastInCombatMs;

	public HpTracker Hp { get; }

	public bool WaveActive { get; private set; }

	public long WaveStartMs { get; private set; }

	public int EnemyCount { get; private set; }

	public int EnemiesIn5m { get; private set; }

	public int AggroOnMe { get; private set; }

	public MitigationCombo CommittedCombo { get; private set; }

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
			CommittedCombo = MitigationCombo.None;
		}
		else if (WaveActive && nowMs - _lastInCombatMs >= CurrentWaveEndDebounceMs)
		{
			WaveActive = false;
		}
	}

	public void CommitCombo(MitigationCombo combo)
	{
		CommittedCombo = combo;
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

	public void Reset()
	{
		Hp.Reset();
		_lastCastMs.Clear();
		_lastEnemySampleMs = 0L;
		_lastInCombatMs = 0L;
		WaveActive = false;
		WaveStartMs = 0L;
		EnemyCount = 0;
		EnemiesIn5m = 0;
		AggroOnMe = 0;
		CommittedCombo = MitigationCombo.None;
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
