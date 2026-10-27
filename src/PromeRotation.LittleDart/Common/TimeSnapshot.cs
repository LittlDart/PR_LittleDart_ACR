using System;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;

namespace LittleDart.Common;

public readonly record struct TimeSnapshot(float GcdRemain, float GcdTotal, float GcdElapsed, bool GcdActive, float AnimLock)
{
	public static float LeadSeconds
	{
		get
		{
			HackSettings hacks = PromeSettings.Instance.Hacks;
			int gcdQueueWindowMs = ((hacks != null) ? hacks.GcdQueueWindowMs : 0);
			if (gcdQueueWindowMs <= 0)
			{
				return 0.35f;
			}
			return (float)Math.Clamp(gcdQueueWindowMs, 50, 1000) / 1000f + 0.05f;
		}
	}

	private const float FallbackLeadSeconds = 0.35f;

	private const float LeadMarginSeconds = 0.05f;

	public static TimeSnapshot Capture()
	{
		float gcdTotal = ActionHelper.GetGcdTotal();
		float gcdElapsed = ActionHelper.GetGcdElapsed();
		float gcdRemain = ActionHelper.GetGcdRemain();
		bool gcdIsActive = ActionHelper.GetGcdIsActive();
		float animationLock = ActionHelper.GetAnimationLock();
		return new TimeSnapshot(gcdRemain, gcdTotal, gcdElapsed, gcdIsActive, animationLock);
	}

	public bool WillBeReady(uint actionId)
	{
		if (Core.Me == null || GameData.IsPlayerOccupied())
		{
			return false;
		}
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(actionId);
		if (adjustedActionId == 0 || !ActionHelper.IsUnlocked(adjustedActionId))
		{
			return false;
		}
		return ActionHelper.GetActionCooldown(adjustedActionId) <= GcdRemain + LeadSeconds;
	}

	public bool ReadyNow(uint actionId)
	{
		if (Core.Me == null || GameData.IsPlayerOccupied())
		{
			return false;
		}
		uint adjustedActionId = ActionHelper.GetAdjustedActionId(actionId);
		if (adjustedActionId == 0 || !ActionHelper.IsUnlocked(adjustedActionId))
		{
			return false;
		}
		return ActionHelper.GetActionCooldown(adjustedActionId) <= 0f;
	}
}
