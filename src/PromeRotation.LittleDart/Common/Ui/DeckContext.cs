using System;
using System.Numerics;

namespace LittleDart.Common.Ui;

public sealed class DeckContext
{
	
	
	
	
	
	public string WindowId { get; init; } = "littledart_deck";

	
	public string JobLabel { get; init; } = "";

	
	public string JobShort { get; init; } = "";

	
	
	
	
	public string Title { get; init; } = "";

	
	public Func<bool> IsDailyMode { get; init; } = () => true;

	
	public Action ToggleMode { get; init; } = () => { };

	
	public Action OpenSettings { get; init; } = () => { };

	
	public Func<bool> AutoPullEnabled { get; init; } = () => false;

	
	public Action<bool> SetAutoPull { get; init; } = _ => { };
}
