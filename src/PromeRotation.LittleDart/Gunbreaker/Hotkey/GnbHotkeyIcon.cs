using System;
using System.Collections.Generic;
using System.IO;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace LittleDart.Gunbreaker.Hotkey;

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
