using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using LittleDart.Gunbreaker.Setting;
using PromeRotation.Data;
using PromeRotation.Helpers;
using PromeRotation.Rotation;

namespace LittleDart.Gunbreaker.Opener;

public class GnbOpener100 : GnbOpenerBase
{
	public const int RequiredLevel = 100;

	private readonly bool _trajectoryOpener;

	private const int PullAtCountdownMs = 300;

	public override string OpenerName => "100级高难起手";

	public override List<PAction> InCombatSequence => BuildSequence();

	public GnbOpener100(bool trajectoryOpener)
	{
		_trajectoryOpener = trajectoryOpener;
	}

	public override void InitializeCountdown(CountDownHandler countdownHandler)
	{
		uint actionId = (GnbSettings.Instance?.PullAction ?? 0) switch
		{
			1 => 16143u, 
			2 => 0u, 
			_ => 36934u, 
		};
		if (actionId == 0)
		{
			return;
		}
		bool isGcd = actionId == 16143;
		countdownHandler.AddAction(300, (Func<PAction>)(() =>
		{
			if (!GnbHelper.HasValidTarget())
			{
				return (PAction)null;
			}
			if (!GnbHelper.IsInMeleeRange() && GnbHelper.GetDistanceToTarget() > GnbHelper.GetActionRange(actionId))
			{
				return (PAction)null;
			}
			if (!ActionHelper.IsUnlocked(actionId) || !ActionHelper.CanCast(actionId, (IBattleChara)null))
			{
				return (PAction)null;
			}
			if (!isGcd)
			{
				GnbHelper.NotifyTrajectoryPull();
			}
			return GnbAction.Of(actionId, isGcd ? ActionType.Gcd : ActionType.OffGcd);
		}));
	}

	private List<PAction> BuildSequence()
	{
		List<PAction> list = new List<PAction>();
		if (_trajectoryOpener)
		{
			list.Add(V(GnbAction.Of(36934u, (ActionType)1)));
		}
		list.Add(V(GnbAction.Of(16137u, (ActionType)0)));
		list.Add(V(GnbAction.Enemy(16164u, (ActionType)1)));
		list.Add(V(GnbAction.Of(16139u, (ActionType)0)));
		// ★★ 开场爆发药与无情（均为 oGCD）：**必须在同一个 oGCD 窗口里，且药排在无情之后**。
		//
		//   窗口由前后两个 GCD 圈定 —— 上一格是「残暴弹」（16139），下一格是「烈牙」（16146），
		//   这两格 oGCD 夹在中间 ⇒ 天然同窗口。顺序由**列表位置**决定：起手整体是一个
		//   ActionGroupCommand（Queue，impl:89133-89149），框架严格按队列顺序逐格取，
		//   所以在列表里「无情在前、药在后」= 游戏里先无情再喝药。
		//
		//   为什么药要晚于无情（2026-10-15 玩家口径）：这一格的药带 RequiresVerification，
		//   确认不通过时会重试（3 次、每次闸门 100ms·n，impl:11055-11093）；药排在前面就等于
		//   让「无情」这个爆发窗口的总开关去等药的确认。药后置 ⇒ 最坏情况只影响后面那一格 GCD。
		//
		//   ⚠️ 药的那一格由 AddPotionIfEnabled 自己挂 RequiresVerification（它可能什么都不加 ——
		//      QT 关着 / 背包没药），所以这里**不要**再用「改 list 最后一格」那种写法。
		list.Add(V(GnbAction.Self(16138u, (ActionType)1)));
		GnbOpenerBase.AddPotionIfEnabled(list);
		list.Add(V(GnbAction.Of(16146u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16165u, (ActionType)1)));
		list.Add(V(GnbAction.Of(16156u, (ActionType)1)));
		list.Add(V(GnbAction.Of(25760u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16159u, (ActionType)1)));
		list.Add(V(GnbAction.Of(16153u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16147u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16157u, (ActionType)1)));
		list.Add(V(GnbAction.Of(16150u, (ActionType)0)));
		list.Add(V(GnbAction.Of(16158u, (ActionType)1)));
		list.Add(V(GnbAction.Of(36937u, (ActionType)0)));
		list.Add(V(GnbAction.Of(36938u, (ActionType)0)));
		list.Add(V(GnbAction.Of(36939u, (ActionType)0)));
		return list;
	}

	private static PAction V(PAction action)
	{
		action.RequiresVerification = true;
		return action;
	}
}
