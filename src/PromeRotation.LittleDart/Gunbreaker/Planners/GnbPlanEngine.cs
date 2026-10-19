using System;
using System.Collections.Generic;
using System.Linq;
using ECommons.Logging;
using PromeRotation.Data;
using PromeRotation.Managers;
using PromeRotation.Resolvers;

namespace LittleDart.Gunbreaker.Planners;

public sealed class GnbPlanEngine
{
	public const long VerifyTimeoutMs = 2000L;

	private const int MaxDispatchAttempts = 2;

	private const int NoStep = -1;

	private readonly GnbPlanStep[] _steps;

	private readonly int[] _remaining;

	private readonly uint[] _handoffActionIds;

	private int _activeStepId = NoStep;

	private int _pendingStepId = NoStep;

	private uint _pendingActionId;

	private int _attempts;

	private long _pendingSinceTicks;

	private bool _awaitingConfirm;

	private bool _stoodDown;

	private string _stopReason = "尚未复位";

	public IReadOnlyList<GnbPlanStep> Steps => _steps;

	public int CurrentIndex => _activeStepId;

	public int CurrentStepNumber
	{
		get
		{
			if (_activeStepId != NoStep)
			{
				return _activeStepId + 1;
			}
			return 0;
		}
	}

	public bool IsActive
	{
		get
		{
			if (!_stoodDown)
			{
				return _activeStepId != NoStep;
			}
			return false;
		}
	}

	public bool IsStandingBy => AxisIsDriving();

	public bool IsStoodDown => _stoodDown;

	public string Status
	{
		get
		{
			if (IsStandingBy)
			{
				return "让位：轴在跑（docs18 S4）";
			}
			if (_stoodDown)
			{
				return "已停用：" + _stopReason;
			}
			if (!GnbHelper.IsPostOpenerPlanWindow())
			{
				return "待命：不在「起手刚跑完」的窗口里";
			}
			if (_activeStepId == NoStep)
			{
				return "待命：没有前置条件成立的步";
			}
			string confirmSuffix = (_awaitingConfirm ? $"（等确认第 {_pendingStepId + 1} 步，第 {_attempts} 次）" : string.Empty);
			return $"相位尾巴 第 {_activeStepId + 1}/{_steps.Length} 步：{_steps[_activeStepId].Name}{confirmSuffix}";
		}
	}

	public GnbPlanEngine(IReadOnlyList<GnbPlanStep> steps, params uint[] handoffActionIds)
	{
		_steps = steps.ToArray();
		_remaining = new int[_steps.Length];
		for (int i = 0; i < _steps.Length; i++)
		{
			_remaining[i] = Math.Max(1, _steps[i].IntentCount);
		}
		_handoffActionIds = ((handoffActionIds != null && handoffActionIds.Length > 0) ? handoffActionIds.ToArray() : Array.Empty<uint>());
	}

	
	public static bool AxisIsDriving()
	{
		return GnbHelper.HasTimelineRunning();
	}

	
	
	
	public CheckResult CheckCurrent()
	{
		int stepId = EvaluateStep(out string reason);
		if (stepId == NoStep)
		{
			return new CheckResult(false, reason);
		}
		GnbPlanStep gnbPlanStep = _steps[stepId];
		return new CheckResult(true, $"相位尾巴 第 {stepId + 1}/{_steps.Length} 步：{gnbPlanStep.Name}" + (_awaitingConfirm ? $"（第 {_attempts} 次发出，等确认）" : string.Empty));
	}

	
	
	
	private int EvaluateStep(out string reason)
	{
		if (IsStandingBy)
		{
			reason = "让位：轴在跑（轴自己塞意图，我们不抢这一格）";
			return NoStep;
		}
		if (_stoodDown)
		{
			reason = "相位引擎已停用：" + _stopReason;
			return NoStep;
		}
		if (!GnbHelper.IsPostOpenerPlanWindow())
		{
			reason = "不是「起手刚跑完」的相位窗口（交给常规决策器）";
			return NoStep;
		}
		if (ActionQueueManager.HasActionsInGcdQueue())
		{
			reason = "起手队列还没排空（等框架把起手跑完）";
			return NoStep;
		}
		if (_awaitingConfirm)
		{
			reason = string.Empty;
			return _pendingStepId;
		}
		if (CanRedispatchPending())
		{
			reason = string.Empty;
			return _pendingStepId;
		}
		int selected = SelectStep();
		reason = ((selected == NoStep) ? "相位表里没有前置条件成立的步" : string.Empty);
		return selected;
	}

	private bool CanRedispatchPending()
	{
		if (_pendingStepId == NoStep || _pendingStepId >= _steps.Length)
		{
			return false;
		}
		bool preconditionMet;
		try
		{
			preconditionMet = _steps[_pendingStepId].Precondition();
		}
		catch (Exception preconditionException)
		{
			PluginLog.Warning("[LittleDart-GNB] 相位步「" + _steps[_pendingStepId].Name + "」重发前复查前置条件抛异常：" + preconditionException.Message);
			return false;
		}
		if (!preconditionMet)
		{
			return false;
		}
		return GetIntentActionId(_pendingStepId) == _pendingActionId;
	}

	private int SelectStep()
	{
		for (int i = 0; i < _steps.Length; i++)
		{
			if (_remaining[i] <= 0)
			{
				continue;
			}
			bool preconditionMet;
			try
			{
				preconditionMet = _steps[i].Precondition();
			}
			catch (Exception preconditionException)
			{
				PluginLog.Warning("[LittleDart-GNB] 相位步「" + _steps[i].Name + "」的前置条件抛异常：" + preconditionException.Message);
				continue;
			}
			if (!preconditionMet)
			{
				continue;
			}
			try
			{
				if (_steps[i].Intent() == null)
				{
					continue;
				}
				return i;
			}
			catch (Exception intentException)
			{
				PluginLog.Warning("[LittleDart-GNB] 相位步「" + _steps[i].Name + "」的意图抛异常：" + intentException.Message);
			}
		}
		return NoStep;
	}

	
	
	
	public PAction? GetAction()
	{
		int stepId = EvaluateStep(out _);
		if (stepId == NoStep)
		{
			return null;
		}
		int activeStepId = stepId;
		PAction intentAction;
		try
		{
			intentAction = _steps[activeStepId].Intent();
		}
		catch (Exception intentException)
		{
			PluginLog.Warning("[LittleDart-GNB] 相位步「" + _steps[activeStepId].Name + "」的意图抛异常：" + intentException.Message);
			return null;
		}
		if (intentAction == null)
		{
			return null;
		}
		
		
		if (!_awaitingConfirm && _pendingStepId != NoStep && !CanRedispatchPending())
		{
			ClearPending();
		}
		_activeStepId = activeStepId;
		intentAction.RequiresVerification = true;
		if (_pendingStepId != activeStepId)
		{
			_remaining[activeStepId]--;
		}
		BeginAwaitConfirm(activeStepId, intentAction.ActionId);
		return intentAction;
	}

	private void BeginAwaitConfirm(int stepId, uint actionId)
	{
		_attempts = ((_pendingStepId != stepId) ? 1 : (_attempts + 1));
		_pendingStepId = stepId;
		_pendingActionId = actionId;
		_awaitingConfirm = true;
		_pendingSinceTicks = Environment.TickCount64;
	}

	public bool Confirm(uint actionId)
	{
		if (!_awaitingConfirm || _pendingStepId == NoStep)
		{
			return false;
		}
		if (_pendingActionId != actionId)
		{
			return false;
		}
		bool isHandoffAction = IsHandoffActionId(_pendingActionId);
		bool allStepsExhausted = true;
		int[] remaining = _remaining;
		for (int i = 0; i < remaining.Length; i++)
		{
			if (remaining[i] > 0)
			{
				allStepsExhausted = false;
				break;
			}
		}
		ClearPending();
		_activeStepId = NoStep;
		if (isHandoffAction | allStepsExhausted)
		{
			_stoodDown = true;
			_stopReason = (isHandoffAction ? "相位尾巴已交回（交接动作已出手）" : "相位尾巴已跑完，交回常规决策器");
		}
		return true;
	}

	private uint? GetIntentActionId(int stepId)
	{
		try
		{
			return _steps[stepId].Intent()?.ActionId;
		}
		catch (Exception intentException)
		{
			PluginLog.Warning("[LittleDart-GNB] 相位步「" + _steps[stepId].Name + "」的意图抛异常：" + intentException.Message);
			return null;
		}
	}

	private bool IsHandoffActionId(uint actionId)
	{
		uint[] handoffActionIds = _handoffActionIds;
		for (int i = 0; i < handoffActionIds.Length; i++)
		{
			if (handoffActionIds[i] == actionId)
			{
				return true;
			}
		}
		return false;
	}

	
	
	
	public void Tick()
	{
		HandleVerifyTimeout();
		
		
		if (!_stoodDown && !_awaitingConfirm)
		{
			_activeStepId = EvaluateStep(out _);
		}
	}

	private void HandleVerifyTimeout()
	{
		if (_pendingStepId == NoStep)
		{
			return;
		}
		long now = Environment.TickCount64;
		if (now - _pendingSinceTicks < VerifyTimeoutMs)
		{
			return;
		}
		GnbPlanStep gnbPlanStep = _steps[_pendingStepId];
		if (gnbPlanStep.Optional)
		{
			PluginLog.Warning($"[LittleDart-GNB] 相位步「{gnbPlanStep.Name}」（可选）{VerifyTimeoutMs}ms 内没有出手确认，跳过。");
			ClearPending();
			_activeStepId = NoStep;
			return;
		}
		
		if (_awaitingConfirm && _attempts < MaxDispatchAttempts)
		{
			PluginLog.Warning($"[LittleDart-GNB] 相位步「{gnbPlanStep.Name}」{VerifyTimeoutMs}ms 内没有出手确认，重发一次。");
			_awaitingConfirm = false;
			_pendingSinceTicks = now;
			_activeStepId = _pendingStepId;
			return;
		}
		PluginLog.Warning("[LittleDart-GNB] 相位步「" + gnbPlanStep.Name + "」重发后仍然没有出手确认，交回常规决策器。");
		ClearPending();
		_activeStepId = NoStep;
		_stoodDown = true;
		_stopReason = "起手确认超时，交回决策器";
	}

	public void Reset(string reason)
	{
		_stopReason = reason;
		_stoodDown = false;
		for (int i = 0; i < _steps.Length; i++)
		{
			_remaining[i] = Math.Max(1, _steps[i].IntentCount);
		}
		ClearPending();
		_activeStepId = NoStep;
	}

	private void ClearPending()
	{
		_awaitingConfirm = false;
		_pendingStepId = NoStep;
		_pendingActionId = 0u;
		_pendingSinceTicks = 0L;
		_attempts = 0;
	}
}
