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

	/// <summary>
	/// ★ 2026-10-16：「本波锁定」的**到期时刻**（TickCount64），只对 <see cref="MitigationCombo.Cheap"/> 有意义。
	///
	/// <para><b>解决什么</b>：锁定原本只在「脱战开新波」时清空，而 boss 房从头到尾一直 InCombat
	/// ⇒ 第一次死刑交完铁壁后，整个 boss 战都被钉在 <c>PlanCheap</c> 上。那张表里**没有 Major 档**，
	/// 于是后面的死刑永远轮不到星云 / 大星云（「第二发死刑空转」的根因）。
	/// 锁定本意的覆盖面只有 <c>铁壁 / 伪装</c>（20s）那么长，过了这个窗口就该允许升级到大减。</para>
	///
	/// <para><b>为什么是「时长 + 余量」而不是固定 30s</b>：便宜组最长的持续就是 铁壁 / 伪装 的 20s，
	/// 覆盖窗 = 20s + 5s 余量。窗口一过，<see cref="MitigationEngine.ResolveComboPlan"/> 重新按
	/// 「铁壁 / 小减任一就绪」现算，而不是继续读这条过期的锁定。</para>
	///
	/// <para><b><see cref="MitigationCombo.Big"/></b> 不设到期：大减（120s）本来就该压住整波，
	/// 没有「过期后还要再交一发大减」的诉求。</para>
	/// </summary>
	public long CommittedComboCoverUntilMs { get; private set; }

	/// <summary>锁定此刻是否还在**覆盖窗内**（见 <see cref="CommittedComboCoverUntilMs"/>）。</summary>
	public bool CommittedComboActive => CommittedCombo != MitigationCombo.None && (CommittedComboCoverUntilMs == 0L || NowMs < CommittedComboCoverUntilMs);

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
			CommittedComboCoverUntilMs = 0L;
		}
		else if (WaveActive && nowMs - _lastInCombatMs >= CurrentWaveEndDebounceMs)
		{
			WaveActive = false;
		}
	}

	public void CommitCombo(MitigationCombo combo)
	{
		CommittedCombo = combo;
		// ★ 2026-10-16：锁定带上覆盖窗（只对 Cheap）。Big 记 0 = 永不到期。
		CommittedComboCoverUntilMs = ((combo == MitigationCombo.Cheap) ? (NowMs + MitigationDefaults.CheapComboCoverMs) : 0L);
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
		CommittedComboCoverUntilMs = 0L;
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