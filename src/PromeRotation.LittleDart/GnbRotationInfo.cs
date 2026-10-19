namespace LittleDart;

public static class GnbRotationInfo
{
	public const uint JobId = 37u;

	public const string Name = "LittleDart·绝枪战士";

	public const string Author = "LittleDart";

	/// <summary>
	/// 版本号：**必须**与 <see cref="RotationInfo.Version"/> 逐字相同（同一 dll 多职业的硬校验）。
	/// 不再单独写一份字面量 —— 2026-10-18 正是因为这里写成 1.0.5、骑士写成 1.0.3，
	/// PR 把整个 LittleDart 包拒了（见 RotationInfo.Version 上方注释里的日志证据）。
	/// </summary>
	public const string Version = RotationInfo.Version;
}
