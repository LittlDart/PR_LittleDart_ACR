using System;

namespace LittleDart.Gunbreaker;

public static class GnbLogTag
{
	public const string Plain = "[LittleDart-GNB]";

	public const uint LittleDartRgb = 7119068u;

	public const uint GnbRgb = 65535u;

	private const ushort MessageColorKey = 24;

	private const string TagHead = "[LittleDart-GNB";

	public static ushort KeyLittleDart => 24;

	public static ushort KeyGnb => 24;

	public static string BuildChatMessage(string taggedMessage)
	{
		if (!taggedMessage.StartsWith("[LittleDart-GNB", StringComparison.Ordinal))
		{
			return "[LittleDart-GNB] " + taggedMessage;
		}
		return taggedMessage;
	}
}
