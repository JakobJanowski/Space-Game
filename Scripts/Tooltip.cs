using Godot;
using System;

public partial class Tooltip : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
	}

	private void _on_timer_timeout()
	{
		Show();

	}

	
}
