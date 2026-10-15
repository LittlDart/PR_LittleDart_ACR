using System;
using System.Linq;
using System.Reflection;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using LittleDart.Common.Duty;
using LittleDart.Gunbreaker.GCD;
using LittleDart.Gunbreaker.Hotkey;
using LittleDart.Gunbreaker.Mitigation;
using LittleDart.Gunbreaker.Setting;
using LittleDart.Gunbreaker.Skill;
using PromeRotation.Core;
using PromeRotation.Data;
using PromeRotation.Hosting;
using PromeRotation.LogSystem;
using PromeRotation.Rotation;

namespace LittleDart.Gunbreaker;

public class GnbRotationEventHandler : IRotationEventHandler
{
	private IDisposable? _actionEffectSub;

	private static string BuildStamp
	{
		get
		{
			try
			{
				Assembly assembly = typeof(GnbRotationEventHandler).Assembly;
				string stampValue = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault((AssemblyMetadataAttribute a) => a.Key == "BuildStamp")?.Value;
				return (!string.IsNullOrWhiteSpace(stampValue)) ? stampValue : (assembly.GetName().Version?.ToString() ?? "unknown");
			}
			catch
			{
				return "unknown";
			}
		}
	}

	private void OnActionEffectReceived(LogSystemActionEffectEvent e)
	{
		if (Core.Me != null && e.SourceId == ((IGameObject)Core.Me).EntityId)
		{
			GnbPlanGcd.Engine.Confirm(e.ActionId);
		}
	}

	public void OnUpdate()
	{
	}

	public void OnOutOfBattleUpdate()
	{
		GnbAutoPull.Reconcile();
	}

	public void OnBattleStarted()
	{
		TryAutoTankStanceIn4Man("进入战斗前");
	}

	public void OnBattleUpdate()
	{
		GnbHelper.TickCombatTracking();
		GnbDiagnostics.TickComboSnapshot();
		GnbPlanGcd.Engine.Tick();
		GnbMitigationScheduler.Instance.Tick();
		GnbAutoPull.Reconcile();
	}

	public void OnNoTarget()
	{
	}

	public void OnBattleEnded()
	{
		PromeSettings.Instance.OpenerHasBeenExecuted = false;
		GnbHelper.ResetCountdownPullState();
		GnbHelper.ResetOpenerBridgeState();
		GnbPlanGcd.Engine.Reset("脱战复位（OnBattleEnded）");
		GnbHelper.ResetCombatTracking();
		GnbMitigationScheduler.Instance.Reset();
		PromeSettings.Instance.SetQt("停手", false);
	}

	public void OnTerritoryChanged(ushort territoryId)
	{
		GnbPlanGcd.Engine.Reset("换图复位（OnTerritoryChanged）");
		GnbMitigationScheduler.Instance.Reset();
		TryAutoTankStanceIn4Man("切换地图");
	}

	public void OnEnterRotation()
	{
		ApplyModePreset();
		TryAutoTankStanceIn4Man("启用 ACR 时");
		GnbHotkeyPanels.Register();
		GnbAutoPull.ApplyFromSettings();
		RegisterCommand();
		_actionEffectSub?.Dispose();
		_actionEffectSub = PromeEventBus.OnActionEffect((object)this, (Action<LogSystemActionEffectEvent>)OnActionEffectReceived);
		GnbHelper.Log("欢迎使用 LittleDart 绝枪战士 ACR。 <Build " + BuildStamp + ">");
		GnbHelper.Log("本版已接入 100 级高难轴（倾泻资源 / 无情内 / 无情外 / oGCD 优先级），以及 100 级高难起手（高难模式 + 满 100 级 + QT「单体起手」开着才启动）；弹道只在「倒计时结束后开怪」与起手第 0 格用（决策器侧仍占位）；仍是占位的：命运之环、自动冲刺（**极光 2026-10-15 已实现**，见下面那条加载标记）。打不出来的技能请看 PR 的调试面板（每个决策器都写了为什么不通过）。");
		GnbHelper.Log("★ 本包含 2026-10-12 深夜连击修正 + 起手尾巴改造（A 方案）：① 起手尾巴四格（利刃斩/残暴弹/迅连斩/烈牙）**删除**，起手表止于终结之心，交还决策层按实时状态接手（docs\\18 S1/S3）；② 起手每一格挂 RequiresVerification（PvE 真的会执行的出手确认；**不是** ExecutionGuard —— 那个在 PvE 是空操作）；③ 新增 GnashingPolicy.OpenerBridge（起手收尾桥接档，一次性，窗口 = 自进战起 30 秒）；④ 基础三连续接加「连击窗口 > 0」判据（Combo.Timer）；⑤ GetOpener() 新增「轴在跑 ⇒ 不起手」闸门（全工程唯一轴判定点）。依据见 GnbHelper 与 docs\\18 第 2 节事实表。");
		GnbHelper.Log("★★ 本包含 2026-10-13 起手相位引擎（B 方案 P1）：① 新增 Planners\\GnbPlanEngine.cs（状态驱动的有序步表 + 出手确认 + 超时重发/放弃）；② 新增 Planners\\GnbPlan100Opener.cs（P1 只放**尾巴三步**：迅连斩 / 重建 / 收工边界）；③ 新增 GCD\\GnbPlanGcd.cs 并 **Insert 到 _gcdResolvers 最前面**（只加这一行，既有顺序未动）；④ 事件处理器订阅 PromeEventBus.OnActionEffect 做出手确认（OnExitRotation 对称退订）；⑤ 轴判定仍然只有一处（GnbPlanEngine.AxisIsDriving → GnbHelper.HasTimelineRunning），轴在跑时相位**让位**；⑥ [LittleDart-GNB/Combo] 快照新增「相位=」一格。P1 未上机通过前**不做 P2**（任务单第 6 节）。依据见 docs\\18 S1-S5。");
		GnbHelper.Log("★★ 本包含 2026-10 减伤调度器与 QT 改造：① **公共减伤判断器**落在 `Common\\Mitigation\\`（六层内核：技能池/状态/冷却预测/最低保留/实时交减/外部信号），换职业只换 `MitigationJobProfile` 插槽；② 绝枪插槽 = `Gunbreaker\\Mitigation\\GnbMitigationProfile.cs` + 技能表 `GnbMitigationConfig.cs`（映射：圣盾阵=刚玉之心 / 壁垒=伪装 / 极致防御=大星云 / 圣光幕帘=光之心，低等级形态盾阵=石之心、预警=星云）；③ 10 个减伤决策器接通（含等级互斥形态各自一格），只受 QT「自动减伤」+ 日常模式约束；④ 新 QT「远离闪雷弹」（**高难+日常都可见**，默认开，闸门在 GnbRangedGcd）；⑤ 新 QT「自动减伤」（**日常可见、高难隐藏**，预设随模式）；⑥ 设置窗口页签对齐骑士：战斗 / 起手 / 减伤 / 通用 / 开发。⚠\ufe0f 减伤百分比与持续秒数**不在** Action 表里，GnbMitigationConfig 里标了 TODO(原作者确认)，与骑士侧同样的做法。");
		GnbHelper.Log("★★ 本包含 2026-10「自动拉怪 + 目标选择器」移植：① 新增 `Gunbreaker\\GnbAutoPull.cs`（与 `Paladin\\PldAutoPull.cs` 逐字同构）；② 它**不自己存状态**，读写的是 PR 宿主的 `PromeSettings.AutoPull`（[JsonIgnore]，PR 不落盘）+ `TargetSelectorMode`（PR 自己落盘），保证「ACR 里的开关」与「/prome autopull」是同一件事；③ `GnbSettings` 补 `AutoPullActive` / `AutoPullSelectorMode` 两个字段记住玩家意图，`OnEnterAcr()` 由 `GnbAutoPull.ApplyFromSettings()` 推回宿主（否则每次重载都要重勾）；④ 设置页「战斗」页签（日常）新增「自动拉怪（含目标选择器）」勾选框 + 「目标选择器模式」下拉；⑤ 新增指令 `/LittleDart-GNB autopull 0|1` 与 `autopull selector <模式>`。⚠\ufe0f 绑定规则三条（关拉怪要连选择器一起关、选择器单独设「关」会把拉怪一起关）写在 GnbAutoPull 文件头，改之前先读。");
		GnbHelper.Log("★★ 本包含 2026-10「拉怪 / 主动攻击」改造：① 新 QT「主动攻击」（日常可见 / 高难隐藏，**默认关**）—— 它就是 PR 宿主的 `PromeSettings.AutoPull`（第三方 ACR 界面上叫「主动攻击」，提示语「非战斗状态下自动寻找目标并开始攻击」）；② 新 QT「突进拉怪」（日常可见 / 高难隐藏，日常默认开）—— `GnbTrajectoryOffGcd` 从占位变为实现：脱战 + 有目标 + 不在近战 + 弹道就绪 ⇒ 用弹道贴上去拉仇恨；（★ 2026-10-13 该判据已被替换，见下面那条加载标记）；③ `GnbAutoPull.Reconcile()` 每帧对账「QT ⇄ 宿主」两个入口，非日常时把宿主连目标选择器一起释放；④ 与设置页「战斗（日常）」的「自动拉怪（含目标选择器）」是**同一个开关**（一份状态、两个入口）。⚠\ufe0f 关键前提（已从 impl 核实）：PR 脱战时 `ShouldExecuteAcr()` 在 AutoPull 开着时返回 true ⇒ 走 ExecuteCombat() ⇒ 决策器被问到（impl:10639-10648 / 124977-124984）。**不开主动攻击时，突进拉怪永远不会出手** —— 这不是 bug，是框架的门。");
		GnbHelper.Log("★★ 本包含 2026-10-13「拉怪公共化 + 绝枪对齐骑士」：① 新增 `Common\\Pull\\PullJobProfile.cs`（职业插槽：技能/总闸/门槛/射程/读数/动作工厂）与 `Common\\Pull\\PullResolverCore.cs`（判断 + 出手 + 诊断，一行不看职业）；② 骑士 `IntervenePullOffGcd` 与绝枪 `GnbTrajectoryPullOffGcd` 变成**薄判定器**，只填自己那一格（调停 16461 / 弹道 36934），判据完全同源；③ 绝枪新增设置 `GnbSettings.PullMinAggro`（默认 3，界面在「战斗（日常）」），QT 值不落盘（键名与存档无关）；④ 绝枪弹道拉怪从「脱战 + 有目标 + 不在近战」换成骑士那一套（一仇数 ≥ 门槛 + 挑「不是我一仇 / 没进战斗」的怪 + 目标感知可施放 + NetworkTid 指定目标）⇒ **战斗中也会出手；脱战不再拉怪**（开怪改由「倒计时开怪技能」负责）；⑤ oGCD 位置从列表最前挪到「血壤之后、伤害能力技之前」（对齐骑士的相对位置）。⚠\ufe0f 已知保守点：就绪检查是 `ActionHelper.CanCast`（冷却 > 0 即挡），对 2 层充能的调停/弹道来说「还有层数但下一层在转」会被挡掉 —— 与骑士原行为一致，本次刻意不动。");
		GnbHelper.Log("★★ 本包含 2026-10-13「副本类型判定公共化」：① 新增 `Common\\Duty\\DutyClassifier.cs` = **全工程唯一一份**「在不在副本里 / 几人本」判据（设计人数 = TerritoryType → ContentFinderCondition → ContentMemberType，按地图缓存，判不出来 = 非四人本）；② 消掉四份重复：骑士私有 `DutyMembersPerParty`、两份零调用方的 `GetDutyState()`（已删）、本文件盾姿那条 `Svc.Party.Length > 4` 骨架简化版；两份 `IsInDuty()` 转发也已删除；③ **行为变化（就一处）**：绝枪「四人本自动开王室亲卫」的判据从**实时队伍人数**换成**副本设计人数** —— 以前在「八人本但只到 4 人（有人退本 / 没组满）」时会误判成四人本、替玩家开姿态，现在不会了。⚠\ufe0f M12 的另两条差异（没有玩家开关 / 没有 RecentlyUsed 防重复）**仍然没做**，见 docs\\16。");
		GnbHelper.Log("★ 2026-10-13：① 「主动攻击」的目标选择器**默认值改成「血量最高」**（原「最近」；`GnbAutoPull.FallbackMode` + `GnbSettings.AutoPullSelectorMode` 两处都改了）；⚠\ufe0f 老存档里**已经记住**的选择不会被改写 —— 要换请在设置页「战斗（日常）→ 目标选择器模式」改一次。② 设置页删掉了**全部提示性灰字**（分组说明 / 整段 TextDisabled / 控件副标题 / 时间轴占位行），只保留两处非提示用途：设置读不到时的报错、以及「主动攻击」那一行状态回读（用来看出「假开」）。");
		GnbHelper.Log("★★ 本包含 2026-10-15「减伤判断器 · 自保插槽」：① 公共层新增 `Common\\Mitigation\\MitigationSelfCareRule.cs` + `MitigationJobProfile.SelfCareRules` 插槽 + `MitigationEngine.TryPickSelfCare` 仲裁 —— 排在保命无敌之后、**站定闸门之前**（与无敌同档 ⇒ 血崩时正在走位也能交）；② 绝枪两条规则（`GnbMitigationProfile.BuildSelfCareRules`，阈值常量在 `GnbMitigationConfig`）：**自身血量<50% ⇒ 对自己交刚玉之心**、**自身血量<70% 且身上没有极光 ⇒ 对自己交极光**（顺序 = 优先级，刚玉之心必须在前，否则 40% 血时永远轮不到它）；③ `GnbMitigationConfig` 新增「极光」技能定义（16151 · 45 级 · 60s · buff 1835）与新分类 `MitigationCategory.SelfCare` —— 通用规划器（大减/短减/团队/个人）**永远选不中**它，所以满血时不会被当「个人减」交掉；④ `GnbAuroraOffGcd` 从占位变接通：目标从「最残血的队友」改成**自己**，出手位置从 oGCD 列表末尾挪到刚玉之心旁边（自保组排在血壤/爆破领域/弓形冲波等伤害能力技之前）；⑤ 减伤面板新增「自保插槽」一行（每条规则的触发成不成立 + 血量 + 是否生效中）。⚠\ufe0f 仍受既有闸门约束：QT「自动减伤」+ 日常模式 + 王室亲卫姿态；**高难模式默认不开自动减伤**。");
		GnbHelper.Log("★★ 本包含 2026-10-15「四人本自动盾姿」（与骑士同款）：① 新增设置 `GnbSettings.AutoTankStanceIn4Man`（默认**开**）+ 设置页「日常 · 战斗」第一项勾选框「四人本自动盾姿」（与骑士同名同位）；② `TryAutoTankStance` 更名为 `TryAutoTankStanceIn4Man` 并按骑士写法补齐两道闸门 —— **玩家开关**与 **`RecentlyUsed` 防重复**（用绝枪的技能 ID 16142，不是骑士的 28），返回 bool、日志改成骑士同款 `[ST6] … 已自动开启「王室亲卫」`；③ 技能 ID 差异只有这一处：骑士 钢铁信念 28 / 解除 32065 + buff 79 ↔ 绝枪 王室亲卫 16142 / 解除 32068 + buff 1833（都已封装在 `GnbHelper.SetTankStance` / `HasTankStance` 里）。⚠\ufe0f 行为不变的两条：八人本 / 副本外**不会**自动开（设计人数判据 `DutyClassifier.IsFourManDuty`），出本也**不会**自动关。⚠\ufe0f `ActionHelper.RecentlyUsed` 在本工程**很可能恒为 false**（见 docs\\03 §三 第 1 条），所以那道防重复闸门可能形同不存在 —— 与骑士侧一致，刻意保持同款。");
		GnbHelper.Log("★★ 本包含 2026-10-15「热键面板」（绝枪此前的缺口，见 docs\\21 §3.1 #1）：① 新增 `Gunbreaker\\Hotkey\\` 六个文件 —— `GnbHotkeyPanels`（面板注册 / 注销 / 按名字触发）、`GnbHotkeys`（15 个按钮的名字表 + 把队伍实体翻译成 `ActionTargetType`）、`GnbHotkeyIcon`（自定义图标路径）、外加退避 / LB / 清扫队列三个 `IHotkeyLogic`；② 面板 = **5 列 × 3 行**，顺序按玩家指定：刚玉 → 铁壁 → 伪装 → 大星云 → 亲疏自行 → 挑衅 → 退避 → 弹道 → 闪雷弹 → 血仇 → 极光 → 冲刺 → 超火流星 → LB → 清扫队列；③ 注册 / 注销挂在 `OnEnterRotation` / `OnExitRotation`（与骑士成对同款，PR 重载 ACR 不会残留面板）；④ 新增指令 `/LittleDart-GNB hk <热键名>`（等同点面板按钮）；⑤ ⚠\ufe0f 「极光」按钮打的是**自己**（与自动自保那条同口径）；「给队友 / 给搭档的极光」仍然没有（骑士那套「九宫格支援」面板没搬）。");
		GnbHelper.Log("★★ 本包含 2026-10-15「支援面板 · 六宫格」（补齐上一条 ⑤ 的缺口）：① 新增 `Gunbreaker\\Hotkey\\GnbSupportHotkeyBase.cs` + 六个具体热键 —— **刚玉之心 × 极光**（2 技能）× **搭档坦克 / 治疗 / 血量最低**（3 类目标）= **2 列 × 3 行 = 六宫格**，**刚玉在前、极光在后**，命名规范照骑士的来（`<技能><目标>`，如骑士「干预搭档」↔ 这里「刚玉搭档」）；② 第二个面板标题 `LittleDart · 绝枪支援`，与主面板一起注册 / 注销（`GnbHotkeyPanels` 现在管两个面板，`TryTrigger` / `RegisteredNames` / help 都走两个面板）；③ `GnbHelper` 补 `SupportTargetKind` 枚举 + `GetSupportTarget` / `GetLowestAbsoluteHpPartner` / `GetLowestHpHealer` / `IsHealerJob`（与 `PldHelper` 逐档同构）；④ 六个按钮图标走 `Resources\\Gnb<技能><目标>.png`，并给 csproj 加了 `Resources\\**\\* → 产物目录` 的拷贝规则（以前产物目录里没有 Resources\\，deploy-acr.ps1 会静默跳过图标那一段）；⑤ ⚠\ufe0f 「血量最低」按**绝对 HP** 挑（不是百分比，与骑士侧同口径）；警戒：极光是 Regen，同一目标身上已有极光时再点名不会叠加（游戏侧限制，代码未拦，点错不会报错只是白交）。");
	}

	public void OnExitRotation()
	{
		UnregisterCommand();
		GnbHotkeyPanels.Unregister();
		_actionEffectSub?.Dispose();
		_actionEffectSub = null;
		GnbPlanGcd.Engine.Reset("卸载 ACR 复位（OnExitRotation）");
		GnbHelper.Log("绝枪战士 ACR 已停用，感谢使用。");
	}

	public static bool ApplyModePreset()
	{
		GnbSettings instance = GnbSettings.Instance;
		if (instance == null)
		{
			return false;
		}
		GnbQt.ApplyModePreset(instance.Mode == 1);
		if (!instance.AutoAdjustQtOnModeSwitch)
		{
			return false;
		}
		instance.Save();
		return true;
	}

	private static void RegisterCommand()
	{
		try
		{
			Svc.Commands.RemoveHandler("/LittleDart-GNB");
			Svc.Commands.AddHandler("/LittleDart-GNB", new CommandInfo(GnbCommand.OnCommand)
			{
				HelpMessage = "LittleDart 绝枪战士 ACR：qt / hk / mode / mit / autopull / clear / help（输入 /LittleDart-GNB help 看详情）",
				ShowInHelp = true
			});
			GnbHelper.Log("已注册指令 /LittleDart-GNB");
		}
		catch (Exception exception)
		{
			GnbHelper.Log("注册指令失败：" + exception.Message + "（指令功能不可用，其余功能不受影响）");
		}
	}

	private static void UnregisterCommand()
	{
		try
		{
			Svc.Commands.RemoveHandler("/LittleDart-GNB");
		}
		catch (Exception exception)
		{
			GnbHelper.Log("注销指令失败：" + exception.Message);
		}
	}

	private static bool TryAutoTankStanceIn4Man(string source)
	{
		GnbSettings? instance = GnbSettings.Instance;
		if (instance == null || !instance.AutoTankStanceIn4Man)
		{
			return false;
		}
		if (!DutyClassifier.IsFourManDuty)
		{
			return false;
		}
		if (GnbHelper.HasTankStance())
		{
			return false;
		}
		if (GnbHelper.IsRecentlyCast(GnbSkills.王室亲卫, 3000))
		{
			return false;
		}
		GnbHelper.SetTankStance(on: true);
		GnbHelper.Log("[ST6] " + source + "：检测到四人本且当前无坦克姿态 → 已自动开启「王室亲卫」（八人本 / 副本外不会自动开；出本也不会自动关）");
		return true;
	}
}
