using System;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace LittleDart.Gunbreaker;

public static class GnbFeedback
{
	private const string Prefix = "[LittleDart]";

	public static void Info(string message)
	{
		GnbHelper.Log(message);
		Print(message, isError: false);
	}

	public static void Fail(string message)
	{
		GnbHelper.Log(message);
		Print(message, isError: true);
	}

	private static void Print(string message, bool isError)
	{
		try
		{
			if (isError)
			{
				Svc.Chat.PrintError("[LittleDart] " + message, (string)null, (ushort?)null);
			}
			else
			{
				Svc.Chat.Print("[LittleDart] " + message, (string)null, (ushort?)null);
			}
		}
		catch (Exception ex)
		{
			PluginLog.Warning("[LittleDart-GNB] 聊天输出失败：" + ex.Message);
		}
	}
}
