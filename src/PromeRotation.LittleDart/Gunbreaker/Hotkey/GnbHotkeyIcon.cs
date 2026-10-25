using System;
using System.Collections.Generic;
using System.IO;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace LittleDart.Gunbreaker.Hotkey;

/// <summary>
/// 自定义按钮图标的路径解析（只有「清扫队列」这种**不是技能**的按钮需要）。
///
/// <para>图标放在 ACR 的部署目录里：<c>&lt;插件配置目录&gt;\ACR\LittleDart\Resources\&lt;文件名&gt;</c>
/// —— 骑士与绝枪是**同一个 ACR 包**（作者目录同为 <c>LittleDart</c>），所以两边解析的是同一个目录。</para>
///
/// <para>找不到图标**不是错误**：面板注册时会退化成「技能自带图标 / 空图标」，
/// 只打一条警告。缓存按文件名做（每帧查一次盘没必要）。</para>
///
/// <para>⚠️ 与骑士 <c>PldHotkeyIcon</c> 是**同构的两份**（日志前缀不同）。没有合并到 Common 层：
/// 两职业各自的热键目录本来就是平行的，合并会让「删掉一个职业」这件事变得不干净。</para>
/// </summary>
internal static class GnbHotkeyIcon
{
	private static readonly Dictionary<string, string?> Cache = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

	public static string? Resolve(string fileName)
	{
		if (Cache.TryGetValue(fileName, out string? cachedPath))
		{
			return cachedPath;
		}
		string? resolvedPath = null;
		try
		{
			string candidatePath = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "ACR", "LittleDart", "Resources", fileName);
			if (File.Exists(candidatePath))
			{
				resolvedPath = candidatePath;
			}
		}
		catch (Exception ex)
		{
			PluginLog.Warning("[LittleDart-GNB/热键] 图标路径解析失败（" + fileName + "）：" + ex.Message);
		}
		if (resolvedPath == null)
		{
			PluginLog.Warning("[LittleDart-GNB/热键] 找不到图标 Resources\\" + fileName + "，该按钮改用技能自带图标");
		}
		Cache[fileName] = resolvedPath;
		return resolvedPath;
	}
}
