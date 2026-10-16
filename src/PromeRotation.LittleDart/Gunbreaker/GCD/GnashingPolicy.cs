namespace LittleDart.Gunbreaker.GCD;

/// <summary>
/// 子弹连（烈牙 16146 起头）的起头闸门策略。
/// ⚠️ 2026-10-14：原 <c>AoeCombo</c>（群体轴）档已删除 ——
/// 玩家口径「群怪阈值成立时只打群体技能」，而子弹连是单体三段连。判据见 <c>docs\23</c>。
/// 本枚举现在**只有单体轴**的六种窗口，全部走 <c>GnbGnashingFangGcd.CheckStartGate</c>。
/// </summary>
public enum GnashingPolicy
{
	OpenerBridge,
	NoMercyContinue,
	NoMercyPrep,
	Default,
	SonicFirst,
	Level100Window
}
