using System.Collections.Generic;
using LittleDart.Paladin.Setting;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Rotation;

namespace LittleDart.Paladin.Opener;

public abstract class PldOpenerBase : IOpener
{
	private const int InterveneInCountDownMs = 300;

	public abstract string OpenerName { get; }

	public abstract List<PAction> InCombatSequence { get; }

	public virtual void InitializeCountdown(CountDownHandler countdownHandler)
	{
		PldSettings instance = PldSettings.Instance;
		if (instance.CurtainCallInCountdown)
		{
			countdownHandler.AddAction(10000, PldAction.Self(3540u, (ActionType)1));
		}
		if (instance.OpenerPreReadMs > 0)
		{
			countdownHandler.AddAction(instance.OpenerPreReadMs, PldAction.Enemy(7384u, (ActionType)0));
		}
		countdownHandler.AddAction(300, PldAction.Enemy(16461u, (ActionType)1));
	}

	// ★ 起手里的「开场爆发药」那一格：QT「开场爆发药」开着 **且** 背包里有药时才插进序列。
	//   骑士**五个起手**（70 / 80 / 90 / 100 / 100FRU）全都调这一个方法 ⇒ 药 ID 全工程只有这一处来源。
	//
	//   ★ 药的 ID 一律走 PR 的**通用接口** GameData.GetBestPotionId()（PromeRotation.Core）——
	//     与绝枪那边（GnbOpenerBase.AddPotionIfEnabled）是逐字同一套口径：
	//     · 它按当前职业给出一张「从新到旧」的药水表，返回背包里第一个有货的；
	//     · HQ 优先，且 PR 用 `itemId + 1000000` 表示 HQ（PR 自己的 UsePotionAction 就是这么造的，impl:43652）；
	//     · 新版本药水上市时由 PR 本体更新那张表 ⇒ 本 ACR 一行都不用改。
	//     骑士（JobId 19）与绝枪同属坦克 ⇒ 取到的是同一列「刚力之幻药 / 刚力之宝药」（12 阶，见 docs\13 §3.2）。
	//   ⚠️ 不要退回 PldSkills 里写死的药水 ID（宝药2级 44162 …）：那是原作者的快照常量，版本一过就过期。
	//   ⚠️ 也不要自己读背包 / 自己判 HQ —— 那等于把 PR 的通用接口抄一份出来当第二真相源。
	//
	//   ⚠️ 与绝枪的**有意差别**：这一格**不挂** RequiresVerification —— 骑士这张起手表整体都没挂，
	//     而绝枪是按 docs\19 A-1/A-2 逐格挂的（含药那一格）。要动就整张表一起动。
	//   ⚠️ 药插在列表的哪个位置由**调用方**决定（本方法只管加一格，不管位置）：
	//     70 / 80 / 90 / 100 四个起手插在第 3 格（先锋剑 → 暴乱剑 之后的那个 oGCD 窗口），
	//     100FRU 插在第 5 格（先锋剑 → 厄运流转 → 偿赎剑 → 暴乱剑 之后）。
	//     顺序 = 列表位置（起手整体是一个 ActionGroupCommand 队列，框架严格按序逐格取）。
	protected static void AddPotionIfEnabled(List<PAction> sequence)
	{
		if (!PromeSettings.Instance.GetQt(PldQt.UsePotion))
		{
			return;
		}
		uint bestPotionId = GameData.GetBestPotionId();
		if (bestPotionId == 0)
		{
			return;
		}
		sequence.Add(PldAction.On(bestPotionId, ActionType.Item, ActionTargetType.Self));
	}
}
