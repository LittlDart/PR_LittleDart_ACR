using System;
using System.Linq;
using System.Reflection;

namespace LittleDart.Common.Ui;

internal static class UiBuildInfo
{
	private const string Key = "BuildStamp";

	public static string Stamp { get; } = Read();

	private static string Read()
	{
		try
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string? value = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
				.FirstOrDefault((AssemblyMetadataAttribute attribute) => attribute.Key == Key)
				?.Value;
			return string.IsNullOrWhiteSpace(value) ? "未知" : value!;
		}
		catch
		{
			return "未知";
		}
	}
}
