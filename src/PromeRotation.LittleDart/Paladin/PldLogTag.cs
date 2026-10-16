using System;

namespace LittleDart.Paladin;

public static class PldLogTag
{
	public const string Plain = "[LittleDart-PLD]";

	public const uint LittleDartRgb = 7119068u;

	public const uint PldRgb = 65535u;

	private const ushort MessageColorKey = 24;

	private const string TagHead = "[LittleDart-PLD";

	public static ushort KeyLittleDart => 24;

	public static ushort KeyPld => 24;

	public static string BuildChatMessage(string taggedMessage)
	{
		if (!taggedMessage.StartsWith("[LittleDart-PLD", StringComparison.Ordinal))
		{
			return "[LittleDart-PLD] " + taggedMessage;
		}
		return taggedMessage;
	}
}
