using System.Collections.Generic;

namespace LittleDart;

public static class Changelog
{
	
	public sealed record Entry(string Version, string Date, string Note);

	
	public static readonly IReadOnlyList<Entry> Entries = new Entry[]
	{
		new Entry("1.0.10", "2026-10-27",
			"修复不同分辨率下的显示问题，增减部分QT。"),
		new Entry("1.0.9", "2026-10-25",
			"新增赛博主题和实验性跟随 PR 主题。"),
		new Entry("1.0.8", "2026-10-20",
			"修复骑士在 Lv.81-90 的悔罪异常判定。"),
		new Entry("1.0.7", "2026-10-19",
			"SDK 接口规范化，精简 QT。"),
		new Entry("1.0.6", "2026-10-19",
			"循环逻辑重构，适配 PR 不同主题，修复已知问题；绝枪输出轴部分判据参考 Nag0miACR。"),
		new Entry("1.0.3", "2026-10-16",
			"修复骑士日常模式下的连招问题。"),
		new Entry("1.0.2", "2026-10-16",
			"重构减伤逻辑，修复 AOE 判定，增加闪雷弹距离判定。"),
		new Entry("1.0.0", "2026-10-15",
			"新增绝枪战士职业，修复骑士移植问题。"),
		new Entry("0.8.0", "2026-10-10",
			"创建工作流，移植骑士 ACR。"),
	};
}
