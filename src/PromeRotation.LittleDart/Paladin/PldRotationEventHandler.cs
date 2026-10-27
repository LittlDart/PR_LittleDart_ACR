using System;
using System.Collections.Generic;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using LittleDart.Common.Duty;
using LittleDart.Paladin.GCD;
using LittleDart.Paladin.Hotkey;
using LittleDart.Paladin.Mitigation;
using LittleDart.Paladin.Setting;
using LittleDart.Paladin.Skill;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Managers;
using PromeRotation.Rotation;
using PromeRotation.Updaters;

namespace LittleDart.Paladin;

public class PldRotationEventHandler : IRotationEventHandler
{
	private bool _gcdWasActive;

	
	
	
	
	
	private const int StrandedQueueTimeoutMs = 1500;

	
	
	
	
	
	private const int StrandedQueueMaxRecoveryAttempts = 2;

	
	private DateTime _strandedQueueSince = DateTime.MinValue;

	
	private int _strandedQueueRecoveryAttempts;

	private const string BuildStampMetadataKey = "BuildStamp";

	private static string BuildStamp
	{
		get
		{
			try
			{
				object[] customAttributes = typeof(PldRotationEventHandler).Assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), inherit: false);
				for (int i = 0; i < customAttributes.Length; i++)
				{
					AssemblyMetadataAttribute assemblyMetadataAttribute = (AssemblyMetadataAttribute)customAttributes[i];
					if (assemblyMetadataAttribute.Key == "BuildStamp")
					{
						return assemblyMetadataAttribute.Value ?? "no-stamp";
					}
				}
				return "no-stamp";
			}
			catch
			{
				return "unknown";
			}
		}
	}

	public void OnUpdate()
	{
		
		
		AoeQueue.Tick();
	}

	public void OnOutOfBattleUpdate()
	{
		PldAutoPull.Reconcile();
	}

	public void OnBattleStarted()
	{
		TryAutoTankStanceIn4Man("进入战斗前");
		_gcdWasActive = false;
		
		ConfiteorChain.Reset();
		ResetStrandedQueueWatchdog();
	}

	public void OnBattleUpdate()
	{
		Diagnostics.TickComboSnapshot();
		MitigationScheduler.Instance.Tick();
		PldSlideCast.Tick();
		UpdateConfiteorDelay();
		PldAutoPull.Reconcile();
		RecoverFromStrandedQueue();
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	private void RecoverFromStrandedQueue()
	{
		try
		{
			bool stuck = GameData.IsInCombat()
				&& !ActionUpdaterRouter.HasActiveCommand()
				&& !ActionQueueManager.HasActionsInGcdQueue()
				&& ActionQueueManager.HasActionsInOffGcdQueue()
				&& ActionHelper.GetGcdRemain() <= 0f;
			if (!stuck)
			{
				_strandedQueueSince = DateTime.MinValue;
				return;
			}

			DateTime now = DateTime.UtcNow;
			if (_strandedQueueSince == DateTime.MinValue)
			{
				_strandedQueueSince = now;
				return;
			}
			if ((now - _strandedQueueSince).TotalMilliseconds < StrandedQueueTimeoutMs)
			{
				return;
			}

			_strandedQueueSince = DateTime.MinValue;
			if (_strandedQueueRecoveryAttempts >= StrandedQueueMaxRecoveryAttempts)
			{
				return;
			}
			_strandedQueueRecoveryAttempts++;
			string leftovers = DescribeNonEmptyQueues();
			if (_strandedQueueRecoveryAttempts == 1)
			{
				PldHelper.Log("[LittleDart-PLD/起手] 检测到动作在 GCD 空闲时取不出（PR 起手标志会因此卡死，见 docs\\45）。第 1 步：清扫普通队列。残留：" + leftovers);
				ActionQueueManager.ClearNormalQueues();
			}
			else
			{
				PldHelper.Log("[LittleDart-PLD/起手] 普通队列清完仍卡着。第 2 步：ActionUpdaterRouter.Reset()。残留：" + leftovers);
				ActionUpdaterRouter.Reset();
			}
		}
		catch (Exception exception)
		{
			_strandedQueueSince = DateTime.MinValue;
			PldHelper.Log("[LittleDart-PLD/起手] 队列残留自救失败：" + exception.Message);
		}
	}

	
	private void ResetStrandedQueueWatchdog()
	{
		_strandedQueueSince = DateTime.MinValue;
		_strandedQueueRecoveryAttempts = 0;
	}

	
	private static string DescribeNonEmptyQueues()
	{
		try
		{
			List<string> parts = new List<string>();
			foreach (KeyValuePair<string, List<string>> queue in ActionQueueManager.GetQueueStatus())
			{
				if (queue.Value.Count > 0)
				{
					parts.Add(queue.Key + "=" + string.Join(",", queue.Value));
				}
			}
			return parts.Count > 0 ? string.Join(" | ", parts) : "(快照为空)";
		}
		catch (Exception exception)
		{
			return "(快照失败：" + exception.Message + ")";
		}
	}

	public void OnNoTarget()
	{
	}

	public void OnBattleEnded()
	{
		PromeSettings.Instance.OpenerHasBeenExecuted = false;
		PromeSettings.Instance.SetQt(PldQt.StopAttack, false);
		ConfiteorDelay.Reset();
		ConfiteorChain.Reset();
		_gcdWasActive = false;
		ResetStrandedQueueWatchdog();
		MitigationScheduler.Instance.Reset();
	}

	public void OnTerritoryChanged(ushort territoryId)
	{
		
		
		
		PromeSettings.Instance.OpenerHasBeenExecuted = false;
		ConfiteorDelay.Reset();
		ConfiteorChain.Reset();
		MitigationScheduler.Instance.Reset();
		_gcdWasActive = false;
		ResetStrandedQueueWatchdog();
		TryAutoTankStanceIn4Man("切换地图");
	}

	public void OnEnterRotation()
	{
		ApplyModePreset();
		TryAutoTankStanceIn4Man("启用 ACR 时");
		PldHotkeyPanels.Register();
		PldAutoPull.ApplyFromSettings();
		RegisterCommand();
		PldHelper.Log("欢迎使用；暂无时间轴适配，接下来会制作极神和绝伊甸轴。 <Build " + BuildStamp + ">");
		PldHelper.Log("★ 2026-10-13：副本类型判定已公共化 —— `Common\\Duty\\DutyClassifier`（全工程唯一一份）。`PldHelper.IsInDuty()` / `GetDutyState()` 已删除（后者零调用方），四人本自动盾姿改调 `DutyClassifier.IsFourManDuty`，本职业行为不变。");
		PldHelper.Log("★ 2026-10-13：① 「主动攻击」的目标选择器**默认值改成「血量最高」**（原「最近」）；⚠\ufe0f 老存档里已经记住的选择不会被改写。② 设置页删掉了全部提示性灰字（分组说明 / 整段 TextDisabled / 控件副标题 / 时间轴占位行），只保留设置读不到时的报错。");
	}

	public void OnExitRotation()
	{
		UnregisterCommand();
		PldHotkeyPanels.Unregister();
		PldHelper.Log("骑士 ACR 已停用,感谢使用。");
	}

	private static void RegisterCommand()
	{
		try
		{
			Svc.Commands.RemoveHandler("/LittleDart-PLD");
			Svc.Commands.AddHandler("/LittleDart-PLD", new CommandInfo(PldCommand.OnCommand)
			{
				HelpMessage = "LittleDart 骑士 ACR：qt / hk / mode / mit / clear / help（输入 /LittleDart-PLD help 看详情）",
				ShowInHelp = true
			});
			PldHelper.Log("已注册指令 /LittleDart-PLD");
		}
		catch (Exception exception)
		{
			PldHelper.Log("注册指令失败：" + exception.Message + "（指令功能不可用，其余功能不受影响）");
		}
	}

	private static void UnregisterCommand()
	{
		try
		{
			Svc.Commands.RemoveHandler("/LittleDart-PLD");
		}
		catch (Exception exception)
		{
			PldHelper.Log("注销指令失败：" + exception.Message);
		}
	}

	private void UpdateConfiteorDelay()
	{
		bool gcdIsActive = ActionHelper.GetGcdIsActive();
		if (gcdIsActive && !_gcdWasActive)
		{
			if (ActionHelper.RecentlyUsed(PldSkills.沥血剑, 1000))
			{
				ConfiteorDelay.StartDelay();
			}
			else
			{
				ConfiteorDelay.Tick();
			}
		}
		_gcdWasActive = gcdIsActive;
	}

	private static bool TryAutoTankStanceIn4Man(string source)
	{
		if (!PldSettings.Instance.AutoTankStanceIn4Man)
		{
			return false;
		}
		if (!DutyClassifier.IsFourManDuty)
		{
			return false;
		}
		if (PldHelper.HasTankStance())
		{
			return false;
		}
		if (ActionHelper.RecentlyUsed(PldSkills.钢铁信念, 3000))
		{
			return false;
		}
		PldHelper.SetTankStance(on: true);
		PldHelper.Log("[ST6] " + source + "：检测到四人本且当前无坦克姿态 → 已自动开启「钢铁信念」（八人本 / 副本外不会自动开；出本也不会自动关）");
		return true;
	}

	public static bool ApplyModePreset()
	{
		PldSettings instance = PldSettings.Instance;
		if (instance == null)
		{
			return false;
		}
		bool appliedModeIsDaily = instance.Mode != 0;
		PldQt.ApplyModePreset(appliedModeIsDaily);
		if (!instance.AutoAdjustQtOnModeSwitch)
		{
			return false;
		}
		instance.Save();
		PldHelper.ToastModeBanner(appliedModeIsDaily);
		return true;
	}
}
