using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using LittleDart.Common.Mitigation.Calc;
using Lumina.Excel.Sheets;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Common.Mitigation;

public sealed class MitigationEngine : IMitigationScheduler
{
	private readonly MitigationJobProfile _p;

	private MitigationDecision? _cached;

	private long _cachedAt;

	private const int CastDiagIntervalMs = 1000;

	private long _lastCastDiagMs;

		private readonly MitigationStationaryTracker _stationary;

		private readonly ThreatContext _threat;

	public MitigationState State { get; }

	public MitigationJobProfile Profile => _p;

	public bool Enabled
	{
		get
		{
			if (!_p.CanRun())
			{
				return false;
			}
			if (!_p.IsDailyMode())
			{
				return false;
			}
			if (!_p.AutoMitigationQt() || _p.StopAttackQt())
			{
				return false;
			}
			if (_p.Thresholds.RequireTankStance && !_p.HasTankStance())
			{
				return false;
			}
			return true;
		}
	}

	private static int MeLevel
	{
		get
		{
			try
			{
				IBattleChara me = Core.Me;
				return (me != null) ? ((ICharacter)me).Level : 0;
			}
			catch
			{
				return 0;
			}
		}
	}

	public MitigationEngine(MitigationJobProfile profile)
	{
		_p = profile;
		State = new MitigationState(profile);
		_stationary = new MitigationStationaryTracker(profile.IsLocalPlayerMoving);
		_threat = new ThreatContext(profile);
	}

	public void Tick()
	{
		_p.RefreshThresholds();
		State.Tick();
		_stationary.Tick(State.NowMs);

		_threat.TickAoeLock(_p.Thresholds.AoeWarnMs);
		DiagTargetCast();
		bool consumedWaveEnd = State.ConsumeWaveEnded();
		if (Enabled & consumedWaveEnd)
		{
			AuditAfterPull();
		}
	}

	private void DiagTargetCast()
	{
		if (!_p.DiagnosticsEnabled())
		{
			return;
		}
		IBattleChara target = _p.CurrentTarget();
		if (target != null && target.IsCasting)
		{
			long nowMs = State.NowMs;
			if (nowMs - _lastCastDiagMs >= 1000)
			{
				_lastCastDiagMs = nowMs;
				uint castActionId = target.CastActionId;
				string castActionName = ActionNameOrId(castActionId);
				int castRemainMs = (int)((target.TotalCastTime - target.CurrentCastTime) * 1000f);
				bool isTankDeathAction = PromeBattleData.TankDeathActions.Contains(castActionId);
				bool isAoeAction = PromeBattleData.AoeActions.Contains(castActionId);
				bool aoeGateOpen = _threat.IsCastingAoe(_p.Thresholds.AoeWarnMs);
				_p.Log($"{_p.LogPrefix}/死刑 读条={castActionName}({castActionId}) 死刑={isTankDeathAction} AOE集={isAoeAction} AOE闸={aoeGateOpen} 剩余={castRemainMs}ms 减伤Enabled={Enabled} 判据={ThreatContext.Describe(_threat.CurrentCriteria)}");
			}
		}
	}

	private static string ActionNameOrId(uint actionId)
	{
		try
		{
			Lumina.Excel.Sheets.Action? actionRow = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>((ClientLanguage?)null, (string)null)?.GetRowOrDefault(actionId);
			if (!actionRow.HasValue)
			{
				return actionId.ToString();
			}
			Lumina.Excel.Sheets.Action actionSheet = actionRow.Value;
			string actionName = ((object)actionSheet.Name).ToString();
			return string.IsNullOrEmpty(actionName) ? actionId.ToString() : actionName;
		}
		catch
		{
			return actionId.ToString();
		}
	}

	public bool ShouldCast(uint skillId)
	{
		MitigationDecision? currentDecision = GetCurrentDecision();
		if (currentDecision.HasValue)
		{
			return currentDecision.Value.SkillId == skillId;
		}
		return false;
	}

	public void NotifyCast(uint skillId)
	{
		State.RecordCast(skillId);

		if (_p.FindSkill(skillId)?.Category == MitigationCategory.Team)
		{
			_threat.OnTeamMitigationCast(skillId);
		}
		Invalidate();
	}

	public void Invalidate()
	{
		_cached = null;
		_cachedAt = 0L;
	}

	public void Reset()
	{
		State.Reset();
		_stationary.Reset();
		_threat.ResetAoeLock();
		Invalidate();
	}

	public MitigationDecision? GetCurrentDecision()
	{
		long nowMs = State.NowMs;
		if (_cached.HasValue && nowMs - _cachedAt < 80)
		{
			return _cached;
		}
		MitigationDecision? cached = _cached;
		_cached = Evaluate(nowMs);
		_cachedAt = nowMs;
		if (_p.DiagnosticsEnabled() && _cached.HasValue && (!cached.HasValue || cached.Value.SkillId != _cached.Value.SkillId))
		{
			MitigationSkillDef mitigationSkillDef = _p.FindSkill(_cached.Value.SkillId);

			_p.Log($"{_p.LogPrefix} 交={mitigationSkillDef?.Name ?? _cached.Value.SkillId.ToString()} | {_cached.Value.Reason} | 判据={ThreatContext.Describe(_threat.CurrentCriteria)} 死刑={_threat.IsCastingDeathSentence()} AOE={_threat.IsCastingAoe(_p.Thresholds.AoeWarnMs)} 堆={_threat.PackEnemyCount()}只");
		}
		return _cached;
	}

	private MitigationDecision? Evaluate(long now)
	{
		if (!Enabled)
		{
			return null;
		}
		MitigationState state = State;
		if (!state.WaveActive)
		{
			return null;
		}
		int meLevel = MeLevel;
		List<MitigationSkillDef> list = UsablePool(meLevel);
		if (list.Count == 0)
		{
			return null;
		}
		float damagePercent = state.DamageTakenPercent(_p.Thresholds.DamageWindowMs);
		float selfHpPercent = CurrentSelfHpPercent();

		MitigationDecision? invulnPick = TryPickInvulnByCurrentHp(list, meLevel, state, selfHpPercent, damagePercent);
		if (invulnPick.HasValue)
		{
			return invulnPick;
		}

		MitigationDecision? selfCarePick = TryPickSelfCare(meLevel, state, selfHpPercent);
		if (selfCarePick.HasValue)
		{
			return selfCarePick;
		}

		if (!_stationary.IsReady)
		{
			return null;
		}

		bool deathSentenceIncoming = _threat.IsCastingDeathSentence();
		bool aoeIncoming = _threat.IsCastingAoe(_p.Thresholds.AoeWarnMs);
		if (deathSentenceIncoming)
		{
			MitigationDecision? deathSentencePick = TryPickDeathSentence(list, meLevel, state, damagePercent);
			if (deathSentencePick.HasValue)
			{
				return deathSentencePick;
			}
		}
		if (!deathSentenceIncoming && aoeIncoming && _threat.CanUseTeamMitigation())
		{
			MitigationDecision? aoeTeamPick = TryPickAoeTeam(list, meLevel, state);
			if (aoeTeamPick.HasValue)
			{
				return aoeTeamPick;
			}
		}

		int enemyCount = _threat.PackEnemyCount();
		bool tight = IsResourceTight(list, damagePercent);
		bool hasFrequentBig = list.Any((MitigationSkillDef d) => d.Category == MitigationCategory.FrequentBig);
		MitigationCategory[] categoryPlan = ResolveCategories(state.SincePullMomentMs, enemyCount, state.AggroOnMe, damagePercent, tight, hasFrequentBig);
		foreach (MitigationCategory category in categoryPlan)
		{
			if (IsCategoryCovered(category, list, meLevel))
			{
				continue;
			}
			foreach (MitigationSkillDef item in CandidatesForCategory(category, list))
			{
				if (!item.AoeOnly && IsEligible(item, meLevel, state, enforceReprisalGate: true) && !IsSkillActive(item, meLevel) && !IsBlockedByTargetDeath(item) && CanUseWithoutStarving(item, damagePercent, tight, list, meLevel))
				{
					return new MitigationDecision(item.SkillId, item.Category, DescribeSituation(state, enemyCount, damagePercent, tight, category));
				}
			}
		}
		return null;
	}

	private MitigationDecision? TryPickAoeTeam(List<MitigationSkillDef> pool, int level, MitigationState st)
	{
		MitigationSkillDef mitigationSkillDef = null;
		foreach (MitigationSkillDef item in pool)
		{
			if (item.Category == MitigationCategory.Team && !(item.AoeEquivalentMitigation <= 0.0) && IsEligible(item, level, st, enforceReprisalGate: false) && !IsSkillActive(item, level) && (mitigationSkillDef == null || item.AoeEquivalentMitigation > mitigationSkillDef.AoeEquivalentMitigation))
			{
				mitigationSkillDef = item;
			}
		}
		if (mitigationSkillDef == null)
		{
			return null;
		}
		return new MitigationDecision(mitigationSkillDef.SkillId, mitigationSkillDef.Category, $"检测到AOE 等效减伤={mitigationSkillDef.AoeEquivalentMitigation:F0} 怪={st.EnemyCount} 掉血={st.DamageTakenPercent(_p.Thresholds.DamageWindowMs):F1}%");
	}

	private MitigationDecision? TryPickDeathSentence(List<MitigationSkillDef> pool, int level, MitigationState st, float damagePercent)
	{
		MitigationCategory[] categories = new MitigationCategory[3]
		{
			MitigationCategory.ScarceBig,
			MitigationCategory.FrequentBig,
			MitigationCategory.Short
		};
		foreach (MitigationCategory category in categories)
		{
			if (IsCategoryCovered(category, pool, level))
			{
				continue;
			}
			foreach (MitigationSkillDef item in CandidatesForCategory(category, pool))
			{
				if (!item.AoeOnly && IsEligible(item, level, st, enforceReprisalGate: true) && !IsSkillActive(item, level) && !IsBlockedByTargetDeath(item))
				{
					return new MitigationDecision(item.SkillId, item.Category, $"死刑读条 大减+短减 类别={item.CategoryName} 怪={st.EnemyCount} 掉血={damagePercent:F1}%");
				}
			}
		}
		return null;
	}

	private MitigationDecision? TryPickInvulnByCurrentHp(List<MitigationSkillDef> pool, int level, MitigationState st, float selfHpPercent, float damagePercent)
	{
		int invulnHpAt = _p.Thresholds.InvulnHpAt;
		float selfHpPercentNow = selfHpPercent * 100f;
		if (float.IsNaN(selfHpPercentNow) || selfHpPercentNow > (float)invulnHpAt)
		{
			return null;
		}
		if (damagePercent <= 0f)
		{
			return null;
		}
		foreach (MitigationSkillDef item in CandidatesForCategory(MitigationCategory.Invuln, pool))
		{
			if (IsEligible(item, level, st, enforceReprisalGate: true) && !IsSkillActive(item, level) && !IsBlockedByTargetDeath(item))
			{
				return new MitigationDecision(item.SkillId, item.Category, $"阶段=当前血量保命 自己血量={selfHpPercentNow:F1}%(阈值={invulnHpAt}%) 已掉血={damagePercent:F1}%(>0 才交) 怪={st.EnemyCount} 5m={st.EnemiesIn5m} 一仇={st.AggroOnMe} 类别={item.Category}");
			}
		}
		return null;
	}

		private MitigationDecision? TryPickSelfCare(int level, MitigationState st, float selfHpPercent)
	{
		if (_p.SelfCareRules.Count == 0 || float.IsNaN(selfHpPercent))
		{
			return null;
		}
		float selfHpPercentNow = selfHpPercent * 100f;
		foreach (MitigationSelfCareRule rule in _p.SelfCareRules)
		{
			MitigationSkillDef def = rule.Def;
			if (def.SkillId == 0u || !rule.Trigger(selfHpPercent))
			{
				continue;
			}
			if (!IsEligible(def, level, st, enforceReprisalGate: true) || IsSkillActive(def, level))
			{
				continue;
			}
			return new MitigationDecision(def.SkillId, def.Category, $"{rule.Reason} 自己血量={selfHpPercentNow:F1}% 怪={st.EnemyCount} 5m={st.EnemiesIn5m} 一仇={st.AggroOnMe} 类别={def.CategoryName}");
		}
		return null;
	}

	private MitigationCategory[] ResolveCategories(long elapsedMs, int enemies, int aggroOnMe, float damagePercent, bool tight, bool hasFrequentBig)
	{
		MitigationCategory[] phaseCategories = ((elapsedMs >= _p.Thresholds.Phase1EndMs && elapsedMs >= _p.Thresholds.Phase2StartMs) ? Phase2(damagePercent, enemies, tight) : Phase1(enemies, tight, hasFrequentBig, _p.Thresholds.MultiTargetEnemies));
		if (aggroOnMe > _p.Thresholds.ArmsLengthAggroOver)
		{
			return new List<MitigationCategory>(phaseCategories) { MitigationCategory.Personal }.ToArray();
		}
		return phaseCategories;
	}

	private static MitigationCategory[] Phase1(int enemies, bool tight, bool hasFrequentBig, int multiTargetEnemies)
	{
		if (enemies < multiTargetEnemies)
		{
			return Array.Empty<MitigationCategory>();
		}
		int enemyWeight = ((enemies <= 7) ? 1 : 2);
		if (tight)
		{
			enemyWeight--;
		}
		MitigationCategory mitigationCategory = (hasFrequentBig ? MitigationCategory.FrequentBig : MitigationCategory.ScarceBig);
		return enemyWeight switch
		{
			0 => new MitigationCategory[2]
			{
				mitigationCategory,
				MitigationCategory.Team
			},
			1 => new MitigationCategory[3]
			{
				mitigationCategory,
				MitigationCategory.Short,
				MitigationCategory.Team
			},
			_ => new MitigationCategory[4]
			{
				MitigationCategory.ScarceBig,
				MitigationCategory.FrequentBig,
				MitigationCategory.Short,
				MitigationCategory.Team
			},
		};
	}

	private MitigationCategory[] Phase2(float damagePercent, int enemies, bool tight)
	{
		if (damagePercent < 15f)
		{
			return Array.Empty<MitigationCategory>();
		}
		int scarceBigAt = _p.Thresholds.ScarceBigAt;
		int bigAt = _p.Thresholds.BigAt;
		bool isMultiTarget = enemies >= _p.Thresholds.MultiTargetEnemies;
		MitigationCategory[] categories;
		if (damagePercent < (float)scarceBigAt)
		{
			categories = ((!isMultiTarget) ? new MitigationCategory[1] { MitigationCategory.Team } : new MitigationCategory[2]
			{
				MitigationCategory.Short,
				MitigationCategory.Team
			});
		}
		else if (damagePercent < (float)bigAt)
		{
			categories = new MitigationCategory[3]
			{
				MitigationCategory.ScarceBig,
				MitigationCategory.Short,
				MitigationCategory.Team
			};
		}
		else if (damagePercent < 45f)
		{
			categories = ((bigAt <= scarceBigAt) ? new MitigationCategory[4]
			{
				MitigationCategory.ScarceBig,
				MitigationCategory.FrequentBig,
				MitigationCategory.Short,
				MitigationCategory.Team
			} : new MitigationCategory[3]
			{
				MitigationCategory.FrequentBig,
				MitigationCategory.Short,
				MitigationCategory.Team
			});
		}
		else
		{
			categories = ((!(damagePercent < 70f)) ? new MitigationCategory[1] { MitigationCategory.Invuln } : new MitigationCategory[4]
			{
				MitigationCategory.ScarceBig,
				MitigationCategory.FrequentBig,
				MitigationCategory.Short,
				MitigationCategory.Team
			});
		}
		if (tight)
		{
			for (int i = 0; i < categories.Length; i++)
			{
				if (categories[i] == MitigationCategory.ScarceBig)
				{
					categories[i] = MitigationCategory.FrequentBig;
				}
			}
		}
		return categories;
	}

	private static IEnumerable<MitigationSkillDef> CandidatesForCategory(MitigationCategory category, List<MitigationSkillDef> pool)
	{
		return from d in pool
			where d.Category == category
			orderby d.Priority, d.MitigationPercent descending
			select d;
	}

	private bool IsCategoryCovered(MitigationCategory category, List<MitigationSkillDef> pool, int level)
	{
		foreach (MitigationSkillDef item in pool)
		{
			if (item.Category == category && IsSkillActive(item, level))
			{
				return true;
			}
		}
		return false;
	}

	private List<MitigationSkillDef> UsablePool(int level)
	{
		List<MitigationSkillDef> list = _p.PoolForLevel(level);
		list.RemoveAll((MitigationSkillDef d) => !ActionHelper.IsUnlocked(d.SkillId));
		return list;
	}

	private bool IsEligible(MitigationSkillDef def, int level, MitigationState st, bool enforceReprisalGate)
	{
		if (!def.LevelAllowed(level) || !ActionHelper.IsUnlocked(def.SkillId))
		{
			return false;
		}
		if (def.ResourceCost > 0 && !_p.HasResourceFor(def))
		{
			return false;
		}
		if (enforceReprisalGate && def.SkillId == _p.ReprisalSkillId && PullPhaseDetector.IsPulling(st.EnemiesIn5m, _p.Thresholds.ReprisalMinEnemiesIn5m))
		{
			return false;
		}
		if (def.SingleBossForbidden && _threat.PackEnemyCount() <= 1)
		{
			return false;
		}
		if (def.DeathSentenceForbidden && _threat.IsCastingDeathSentence())
		{
			return false;
		}
		if (def.RequireAggroOnMeOver > 0 && st.AggroOnMe <= def.RequireAggroOnMeOver)
		{
			return false;
		}
		if (!def.SelfTarget && !_p.HasValidTarget())
		{
			return false;
		}
		return _p.IsReadyWithCanCast(def);
	}

	private bool IsSkillActive(MitigationSkillDef def, int level)
	{
		if (def.BuffId != 0 && (def.SelfTarget ? _p.HasSelfAura(def.BuffId) : (_p.TargetAuraTimeLeft(def.BuffId) > 0.0)))
		{
			return true;
		}
		if (def.ShieldBuffId != 0 && _p.HasSelfAura(def.ShieldBuffId))
		{
			return true;
		}
		return IsRecentlyCastByUs(def.SkillId, 1500);
	}

	private bool IsRecentlyCastByUs(uint skillId, int windowMs)
	{
		if (_p.IsRecentlyCast(skillId, windowMs))
		{
			return true;
		}
		long lastCastMs = State.LastCastMs(skillId);
		if (lastCastMs != 0L)
		{
			return State.NowMs - lastCastMs < windowMs;
		}
		return false;
	}

	private List<MitigationSkillDef> ProjectedAvailable(List<MitigationSkillDef> pool, double secondsAhead, MitigationSkillDef? simulatedUse)
	{
		List<MitigationSkillDef> list = new List<MitigationSkillDef>(pool.Count);
		foreach (MitigationSkillDef item in pool)
		{
			double cooldownSeconds = ((simulatedUse != null && item.SkillId == simulatedUse.SkillId) ? item.CooldownSeconds : ((double)ActionHelper.GetActionCooldown(item.SkillId)));
			double reserveMinCooldownSeconds = _p.Thresholds.ReserveMinCooldownSeconds;
			if (item.CooldownSeconds <= reserveMinCooldownSeconds || cooldownSeconds <= secondsAhead + 0.3)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private bool IsResourceTight(List<MitigationSkillDef> pool, float damagePercent)
	{
		double secondsAhead = State.EstimateSecondsToNextPull();
		List<MitigationSkillDef> projected = ProjectedAvailable(pool, secondsAhead, null);
		ReserveRequirement need = _p.MinReserve(InvertedBand(damagePercent));
		return !Satisfies(projected, need);
	}

	private bool CanUseWithoutStarving(MitigationSkillDef def, float damagePercent, bool tight, List<MitigationSkillDef> pool, int level)
	{
		bool hpAboveInvulnThreshold = damagePercent >= _p.Thresholds.InvulnThresholdPercent;
		if ((!hpAboveInvulnThreshold & tight) && def.Category == MitigationCategory.ScarceBig)
		{
			return false;
		}
		double secondsAhead = State.EstimateSecondsToNextPull();
		List<MitigationSkillDef> projected = ProjectedAvailable(pool, secondsAhead, def);
		ReserveRequirement need = _p.MinReserve(InvertedBand(damagePercent));
		if (Satisfies(projected, need))
		{
			return true;
		}
		MitigationCategory category = def.Category;
		if (((uint)(category - 2) <= 1u || category == MitigationCategory.Personal) ? true : false)
		{
			return true;
		}
		return hpAboveInvulnThreshold;
	}

	private static bool Satisfies(List<MitigationSkillDef> projected, ReserveRequirement need)
	{
		int readyBig = 0;
		int readyShort = 0;
		int readyTeam = 0;
		foreach (MitigationSkillDef item in projected)
		{
			if (!item.AoeOnly)
			{
				switch (item.Category)
				{
				case MitigationCategory.ScarceBig:
				case MitigationCategory.FrequentBig:
					readyBig++;
					break;
				case MitigationCategory.Short:
					readyShort++;
					break;
				case MitigationCategory.Team:
					readyTeam++;
					break;
				}
			}
		}
		if (readyBig >= need.Big && readyShort >= need.Short)
		{
			return readyTeam >= need.Team;
		}
		return false;
	}

	private PressureBand InvertedBand(float damagePercent)
	{
		float pressureScore = (float)State.EnemyCount * _p.Thresholds.StressEnemyWeight + damagePercent * _p.Thresholds.StressDamageWeight;
		return (PressureBand)((pressureScore >= _p.Thresholds.StressHighThreshold) ? 2 : ((pressureScore >= _p.Thresholds.StressMidThreshold) ? 1 : 0)) switch
		{
			PressureBand.Low => PressureBand.High,
			PressureBand.High => PressureBand.Low,
			_ => PressureBand.Mid,
		};
	}

	private bool IsBlockedByTargetDeath(MitigationSkillDef def)
	{
		if (def.CooldownSeconds <= (double)_p.Thresholds.TargetDeathSkipCooldownSeconds)
		{
			return false;
		}
		return TargetDeathPredictor.WillDieWithin(State.Hp, _p.Thresholds.TargetDeathSkipSeconds, 3000);
	}

	public void AuditAfterPull()
	{
		int meLevel = MeLevel;
		List<MitigationSkillDef> pool = UsablePool(meLevel);
		double secondsAhead = State.EstimateSecondsToNextPull();
		List<MitigationSkillDef> projected = ProjectedAvailable(pool, secondsAhead, null);
		ReserveRequirement need = _p.MinReserve(InvertedBand(State.LastWaveDamagePercent));
		State.ResourceTight = !Satisfies(projected, need);
	}

	private string DescribeSituation(MitigationState st, int enemies, float damagePercent, bool tight, MitigationCategory category)
	{
		string phaseLabel;
		if (st.SincePullMomentMs < _p.Thresholds.Phase1EndMs)
		{
			phaseLabel = "第一阶段(怪数)";
		}
		else
		{
			phaseLabel = ((st.SincePullMomentMs < _p.Thresholds.Phase2StartMs) ? "过渡(怪数)" : "第二阶段(掉血)");
		}
		string tightSuffix = (tight ? " resourceTight" : string.Empty);
		return $"{phaseLabel} 怪={enemies} 5m={st.EnemiesIn5m} 一仇={st.AggroOnMe} 掉血={damagePercent:F1}% 类别={category}{tightSuffix}";
	}

		private string DescribeSelfCareRules()
	{
		if (_p.SelfCareRules.Count == 0)
		{
			return "无（该职业没有自保规则）";
		}
		float selfHpPercent = CurrentSelfHpPercent();
		List<string> list = new List<string>(_p.SelfCareRules.Count);
		foreach (MitigationSelfCareRule rule in _p.SelfCareRules)
		{
			string triggered = float.IsNaN(selfHpPercent) ? "血量读不到" : (rule.Trigger(selfHpPercent) ? "成立" : "不成立");
			MitigationSkillDef def = rule.Def;
			list.Add($"{def.Name}({def.SkillId}) 触发={triggered} 生效中={IsSkillActive(def, MeLevel)} 文案={rule.Reason}");
		}
		return string.Join(" | ", list);
	}

	public string DescribeActiveMits()
	{
		int meLevel = MeLevel;
		List<string> list = new List<string>();
		foreach (MitigationSkillDef item in _p.PoolForLevel(meLevel))
		{
			if (IsSkillActive(item, meLevel))
			{
				list.Add(item.Name);
			}
		}
		if (list.Count != 0)
		{
			return string.Join("、", list);
		}
		return "无";
	}

	public string DescribeStatus()
	{
		MitigationThresholds thresholds = _p.Thresholds;
		MitigationState state = State;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"===== 减伤调度器状态（{_p.JobName}） =====");
		bool autoMitigationQt = _p.AutoMitigationQt();
		bool stopAttackQt = _p.StopAttackQt();
		bool dailyModeQt = _p.DailyModeQt();
		bool tankStanceOk = !thresholds.RequireTankStance || _p.HasTankStance();
		stringBuilder.AppendLine($"职业={_p.JobName} 存档模式判据 IsDailyMode={_p.IsDailyMode()} QT[日常模式]={dailyModeQt} QT[自动减伤]={autoMitigationQt} QT[停手]={stopAttackQt}");
		stringBuilder.AppendLine($"坦克姿态={_p.HasTankStance()}（要求={thresholds.RequireTankStance} → {tankStanceOk}）");
		stringBuilder.AppendLine($"→ Enabled = {Enabled}");
		stringBuilder.AppendLine($"    日常模式={_p.IsDailyMode()}  自动减伤={autoMitigationQt}  未停手={!stopAttackQt}");
		stringBuilder.AppendLine($"波次：WaveActive={state.WaveActive} 波内={state.WaveElapsedMs}ms");

		stringBuilder.AppendLine($"信号：怪10m={state.EnemyCount} 怪5m={state.EnemiesIn5m} 一仇={state.AggroOnMe} {thresholds.DamageWindowMs}ms掉血={state.DamageTakenPercent(thresholds.DamageWindowMs):F1}%");
		stringBuilder.AppendLine($"减伤阈值：稀缺大减={thresholds.ScarceBigThresholdPercent}%（生效 {thresholds.ScarceBigAt}%） 大减={thresholds.BigThresholdPercent}%（生效 {thresholds.BigAt}%） 无敌血量={thresholds.InvulnHpPercent}%（生效 {thresholds.InvulnHpAt}%） 自己血量={CurrentSelfHpPercent():F1}%");
		int level = MeLevel;

		stringBuilder.AppendLine($"目标形态：{TargetClassifier.Describe(ClassifyTargetProfile())}");
		stringBuilder.AppendLine($"威胁判据：{ThreatContext.Describe(_threat.CurrentCriteria)}  死刑读条={_threat.IsCastingDeathSentence()}  AOE读条={_threat.IsCastingAoe(thresholds.AoeWarnMs)}  交战堆(15m)={_threat.PackEnemyCount()}只");

		stringBuilder.AppendLine($"AOE团队减伤锁：{(_threat.CanUseTeamMitigation() ? "可交（未上锁）" : _threat.DescribeAoeLock())}  保险丝={ThreatContext.AoeMitigationLockFuseMs}ms");
		stringBuilder.AppendLine($"站定闸门：IsReady={_stationary.IsReady}（需连续站定 {MitigationStationaryTracker.RequiredStationaryMs}ms）");
		stringBuilder.AppendLine($"自保插槽：{DescribeSelfCareRules()}");
		stringBuilder.AppendLine($"技能明细（等级 {level}，未过滤解锁）：");
		foreach (MitigationSkillDef item in _p.PoolForLevel(level))
		{
			bool unlocked = ActionHelper.IsUnlocked(item.SkillId);
			bool ready = unlocked && _p.IsReadyWithCanCast(item);
			double cooldownSeconds = (unlocked ? ActionHelper.GetActionCooldown(item.SkillId) : (-1f));
			stringBuilder.AppendLine($"  {item.Name}({item.SkillId}) 解锁={unlocked} 就绪={ready} CD={cooldownSeconds:F1} 分类={item.Category}");
		}
		stringBuilder.AppendLine($"生效中：{DescribeActiveMits()}");
		MitigationDecision? currentDecision = GetCurrentDecision();
		stringBuilder.AppendLine(currentDecision.HasValue ? ("当前决策=" + _p.FindSkill(currentDecision.Value.SkillId)?.Name + " | " + currentDecision.Value.Reason) : "当前决策=无");
		return stringBuilder.ToString();
	}

		private TargetProfile ClassifyTargetProfile()
	{
		try
		{
			return TargetClassifier.Classify(
				_p.NearbyEnemyCount(TargetClassifier.EngageRadius),
				sec => _p.TargetSurvivesLongerThan(sec));
		}
		catch
		{
			return TargetProfile.Unknown;
		}
	}

	private float CurrentSelfHpPercent()
	{
		try
		{
			IBattleChara me = _p.Me();
			if (me == null)
			{
				return float.NaN;
			}
			ICharacter meCharacter = (ICharacter)(object)me;
			return (float)meCharacter.CurrentHp / (float)meCharacter.MaxHp;
		}
		catch
		{
			return float.NaN;
		}
	}
}
