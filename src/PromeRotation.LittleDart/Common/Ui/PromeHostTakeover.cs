using System;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.Logging;
using PromeRotation;

namespace LittleDart.Common.Ui;

public static class PromeHostTakeover
{
	private static bool _applied;

	private static bool _hostQtWasOpen;

	private static bool _hostSettingsWasOpen;

	private static bool _hostCaptured;

	private static bool _tickHooked;

	
	
	
	
	public static void Refresh()
	{
		bool want = InterfaceSettings.UseShellWindow;
		if (want == _applied)
		{
			if (want)
			{
				
				LittleDartQtWindow.Instance.IsOpen = true;
			}
			return;
		}
		if (want)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	private static void Enable()
	{
		try
		{
			if (Plugin.Instance is { } plugin)
			{
				
				_hostQtWasOpen = plugin.QTWindow?.IsOpen == true;
				_hostSettingsWasOpen = plugin.SettingsWindow?.IsOpen == true;
				_hostCaptured = true;
				if (plugin.QTWindow != null)
				{
					plugin.QTWindow.IsOpen = false;
				}
				if (plugin.SettingsWindow != null)
				{
					plugin.SettingsWindow.IsOpen = false;
				}
			}
			LittleDartQtWindow.Register();
			LittleDartQtWindow.Instance.IsOpen = true;
			if (!_tickHooked)
			{
				Svc.Framework.Update += Enforce;
				_tickHooked = true;
			}
			_applied = true;
			PluginLog.Information("[LittleDart/界面] 已接管 PR 的 QT 面板（设置窗口只收起一次、不每帧压制）：改用 LittleDart 自建三窗口");
		}
		catch (Exception exception)
		{
			PluginLog.Error("[LittleDart/界面] 接管 PR 宿主窗口失败：" + exception);
			_applied = false;
		}
	}

	
	public static void Disable()
	{
		if (_tickHooked)
		{
			Svc.Framework.Update -= Enforce;
			_tickHooked = false;
		}
		try
		{
			LittleDartQtWindow.Unregister();
			if (_hostCaptured && Plugin.Instance is { } plugin)
			{
				if (plugin.QTWindow != null)
				{
					plugin.QTWindow.IsOpen = _hostQtWasOpen;
				}
				if (plugin.SettingsWindow != null)
				{
					plugin.SettingsWindow.IsOpen = _hostSettingsWasOpen;
				}
			}
		}
		catch (Exception exception)
		{
			PluginLog.Error("[LittleDart/界面] 关闭宿主窗口接管时出错：" + exception);
		}
		_hostCaptured = false;
		if (_applied)
		{
			PluginLog.Information("[LittleDart/界面] 已取消接管，PR 的 QT 面板与设置窗口状态已恢复");
		}
		_applied = false;
	}

	
	
	
	
	
	private static void Enforce(IFramework framework)
	{
		try
		{
			if (Plugin.Instance is not { } plugin)
			{
				return;
			}
			if (plugin.QTWindow is { IsOpen: true } qtWindow)
			{
				qtWindow.IsOpen = false;
			}
		}
		catch (Exception exception)
		{
			PluginLog.Warning("[LittleDart/界面] 压制 PR QT 面板时出错（将停止接管）：" + exception.Message);
			Disable();
		}
	}
}
