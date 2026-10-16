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

	private static readonly MitigationCategory[] PlanLow = new MitigationCategory[1]
	{
		MitigationCategory.Flavor
	};

	private static readonly MitigationCategory[] PlanCheap = new MitigationCategory[5]
	{
		MitigationCategory.Flavor,
		MitigationCategory.Rampart,
		MitigationCategory.Minor,
		MitigationCategory.Team,
		MitigationCategory.Personal
	};

	private static readonly MitigationCategory[] PlanBig = new MitigationCategory[4]
	{
		MitigationCategory.Flavor,
		MitigationCategory.Major,
		MitigationCategory.Team,
		MitigationCategory.Personal
	};

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
		MitigationSkillDef def = _p.FindSkill(skillId);
		if (def != null && def.Category == MitigationCategory.Team)
		{
			_threat.OnTeamMitigationCast(skillId);
		}
		if (def != null && State.CommittedCombo == MitigationCombo.None)
		{
			if (def.Category == MitigationCategory.Rampart || def.Category == MitigationCategory.Minor)
			{
				State.CommitCombo(MitigationCombo.Cheap);
			}
			else if (def.Category == MitigationCategory.Major)
			{
				State.CommitCombo(MitigationCombo.Big);
			}
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
		if (deathSentenceIncoming)
		{
			// ★★ 2026-10-16（玩家：boss 房第一发死刑交铁壁、第二发星云/大星云不生效）：
			//   根因是「本波锁定」在 boss 房等于「整场 boss 战锁定」—— 铁壁一出就把组合钉死成
			//   PlanCheap（Flavor/Rampart/Minor/Team/Personal，**没有 Major 档**），
			//   后面的死刑再也不会去问星云 / 大星云。修法两条一起走：
			//     ② 锁定带覆盖窗（见 MitigationState.CommittedComboCoverUntilMs），过期即作废；
			//     ① 就算锁定还在窗内，只要**便宜组这一发真的填不出东西**（该生效的都生效了、
			//        其余全在 CD），也不再空转 —— 直接落到 PlanBig 让大减兜底。
			//   ⚠️ 不变量：先便宜后大减。只要便宜组还能出一把（且锁定还没过期），
			//      就仍然先走便宜组，绝不会因为这条升级逻辑把 90s 组白白省下来。
			bool committedCheap = state.CommittedComboActive && state.CommittedCombo == MitigationCombo.Cheap;
			MitigationDecision? deathSentencePick = committedCheap
				? TryPickPlan(PlanCheap, list, meLevel, state, "死刑读条")
				: TryPickPlan(ResolveComboPlan(list, meLevel, state), list, meLevel, state, "死刑读条");
			if (!deathSentencePick.HasValue && CheapPlanHasPick(list, meLevel, state))
			{
				deathSentencePick = TryPickPlan(PlanCheap, list, meLevel, state, "死刑读条·便宜组兜底");
			}
			if (!deathSentencePick.HasValue)
			{
				deathSentencePick = TryPickPlan(PlanBig, list, meLevel, state, "死刑读条·便宜组填不出来改交大减");
			}
			if (deathSentencePick.HasValue)
			{
				return deathSentencePick;
			}
		}
		bool aoeIncoming = !deathSentenceIncoming && _threat.IsCastingAoe(_p.Thresholds.AoeWarnMs);
		if (aoeIncoming && _threat.CanUseTeamMitigation())
		{
			MitigationDecision? aoeTeamPick = TryPickAoeTeam(list, meLevel, state);
			if (aoeTeamPick.HasValue)
			{
				return aoeTeamPick;
			}
		}

		if (_threat.CurrentCriteria == ThreatContext.Criteria.BossRoom)
		{
			return null;
		}
		int enemies = _threat.PackEnemyCount();
		if (enemies <= MitigationDefaults.LowPressureMaxEnemies)
		{
			if (!FlavorTriggerOk(enemies, selfHpPercent))
			{
				return null;
			}
			return TryPickPlan(PlanLow, list, meLevel, state, $"低压波次 怪={enemies}");
		}
		return TryPickPlan(ResolveComboPlan(list, meLevel, state), list, meLevel, state, $"正常波次 怪={enemies}");
	}

	private MitigationCategory[] ResolveComboPlan(List<MitigationSkillDef> pool, int level, MitigationState st)
	{
		// ★ 2026-10-16：锁定有覆盖窗（state.CommittedComboActive）。过期 = 本波的便宜组已经用完，
		//   重新按「铁壁 / 小减任一就绪」现算 —— 否则 boss 房会被第一次铁壁永久钉在 PlanCheap 上。
		if (st.CommittedComboActive)
		{
			if (st.CommittedCombo == MitigationCombo.Cheap)
			{
				return PlanCheap;
			}
			if (st.CommittedCombo == MitigationCombo.Big)
			{
				return PlanBig;
			}
		}
		return ComboCheapAvailable(pool, level, st) ? PlanCheap : PlanBig;
	}

	private bool ComboCheapAvailable(List<MitigationSkillDef> pool, int level, MitigationState st)
	{
		foreach (MitigationSkillDef item in pool)
		{
			if (item.Category != MitigationCategory.Rampart && item.Category != MitigationCategory.Minor)
			{
				continue;
			}
			if (!IsEligible(item, level, st, enforceReprisalGate: true) || IsSkillActive(item, level))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// 便宜组（<see cref="PlanCheap"/>）这一发**到底出不出得来** —— 与 <see cref="ComboCheapAvailable"/> 的区别是后者只问
	/// 「铁壁 / 小减有没有就绪的」，前者从头走一遍 <see cref="TryPickPlan"/> 同款门槛
	/// （等级 / 解锁 / 资源 / 反恐吓门 / 生效中 / 目标将死）。
	///
	/// <para>★ 2026-10-16：死刑通道用它决定「锁定还在窗内、但便宜组填不出来」时要不要落 <see cref="PlanBig"/>。
	/// 只做可行性判断、不真的出手，所以不碰缓存（<c>_cached</c>）。</para>
	/// </summary>
	private bool CheapPlanHasPick(List<MitigationSkillDef> pool, int level, MitigationState st)
	{
		foreach (MitigationCategory category in PlanCheap)
		{
			if (IsCategoryCovered(category, pool, level))
			{
				continue;
			}
			foreach (MitigationSkillDef item in CandidatesForCategory(category, pool))
			{
				if (item.AoeOnly)
				{
					continue;
				}
				if (!IsEligible(item, level, st, enforceReprisalGate: true))
				{
					continue;
				}
				if (IsSkillActive(item, level))
				{
					continue;
				}
				if (IsBlockedByTargetDeath(item))
				{
					continue;
				}
				return true;
			}
		}
		return false;
	}

	private MitigationDecision? TryPickPlan(MitigationCategory[] plan, List<MitigationSkillDef> pool, int level, MitigationState st, string situation)
	{
		foreach (MitigationCategory category in plan)
		{
			if (IsCategoryCovered(category, pool, level))
			{
				continue;
			}
			foreach (MitigationSkillDef item in CandidatesForCategory(category, pool))
			{
				if (item.AoeOnly)
				{
					continue;
				}
				if (!IsEligible(item, level, st, enforceReprisalGate: true))
				{
					continue;
				}
				if (IsSkillActive(item, level))
				{
					continue;
				}
				if (IsBlockedByTargetDeath(item))
				{
					continue;
				}
				return new MitigationDecision(item.SkillId, item.Category, $"{situation} 档={item.CategoryName} 怪={st.EnemyCount} 5m={st.EnemiesIn5m} 一仇={st.AggroOnMe}");
			}
		}
		return null;
	}

	private static bool FlavorTriggerOk(int enemies, float selfHpPercent)
	{
		if (enemies >= MitigationDefaults.FlavorMinEnemies)
		{
			return true;
		}
		return !float.IsNaN(selfHpPercent) && selfHpPercent * 100f < (float)MitigationDefaults.FlavorSelfHpPercent;
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

	private bool IsBlockedByTargetDeath(MitigationSkillDef def)
	{
		if (def.CooldownSeconds <= (double)_p.Thresholds.TargetDeathSkipCooldownSeconds)
		{
			return false;
		}
		return TargetDeathPredictor.WillDieWithin(State.Hp, _p.Thresholds.TargetDeathSkipSeconds, 3000);
	}

	private string DescribeSelfCareRules()
	{
		if (_p.SelfCareRules.Count == 0)
		{
			return "无";
		}
		float selfHpPercent = CurrentSelfHpPercent();
		List<string> list = new List<string>(_p.SelfCareRules.Count);
		foreach (MitigationSelfCareRule rule in _p.SelfCareRules)
		{
			string triggered = float.IsNaN(selfHpPercent) ? "读不到" : (rule.Trigger(selfHpPercent) ? "成立" : "不成立");
			MitigationSkillDef def = rule.Def;
			list.Add($"{def.Name}({def.SkillId}) 触发={triggered} 生效中={IsSkillActive(def, MeLevel)}");
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
		stringBuilder.AppendLine($"Enabled={Enabled}（日常模式={_p.IsDailyMode()} 自动减伤QT={_p.AutoMitigationQt()} 停手QT={_p.StopAttackQt()} 姿态={_p.HasTankStance()}/要求{thresholds.RequireTankStance}）");
		stringBuilder.AppendLine($"波次：WaveActive={state.WaveActive} 波内={state.WaveElapsedMs}ms 组合={state.CommittedCombo}{(state.CommittedCombo == MitigationCombo.None ? "" : (state.CommittedComboActive ? "（覆盖窗内）" : "（已过期·按现算）"))}");
		stringBuilder.AppendLine($"信号：怪10m={state.EnemyCount} 怪5m={state.EnemiesIn5m} 一仇={state.AggroOnMe} {thresholds.DamageWindowMs}ms掉血={state.DamageTakenPercent(thresholds.DamageWindowMs):F1}%");
		stringBuilder.AppendLine($"无敌血量={thresholds.InvulnHpAt}% 自己血量={CurrentSelfHpPercent():F1}%");
		int level = MeLevel;
		stringBuilder.AppendLine($"目标形态：{TargetClassifier.Describe(ClassifyTargetProfile())}");
		stringBuilder.AppendLine($"判据：{ThreatContext.Describe(_threat.CurrentCriteria)} 死刑={_threat.IsCastingDeathSentence()} AOE={_threat.IsCastingAoe(thresholds.AoeWarnMs)} 堆(15m)={_threat.PackEnemyCount()}只");
		stringBuilder.AppendLine($"AOE锁：{(_threat.CanUseTeamMitigation() ? "未上锁" : _threat.DescribeAoeLock())}");
		stringBuilder.AppendLine($"站定闸门={_stationary.IsReady} 自保插槽：{DescribeSelfCareRules()}");
		stringBuilder.AppendLine($"技能（{level} 级）：");
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
			return TargetClassifier.Classify(_p.NearbyEnemyCount(TargetClassifier.EngageRadius), sec => _p.TargetSurvivesLongerThan(sec));
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
