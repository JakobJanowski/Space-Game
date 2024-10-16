using Godot;
using System;

public partial class GameMenu : Control
{
	//Needs to know who the player is
	private World world;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		world = (World)GetNode<Node2D>("/root/World");
		

    }


	
	//This triggers the world game over code
	//Doubles as the quit button
	//This DOES cause console errors
	//I think its fine cos its the server freaking out before it realises what has happend
	//Keep and eye on effects
	private void _on_quit_button_pressed()
	{
		
		world.triggerGameOver();
	}

	private void _on_setting_button_pressed()
	{
		PanelContainer settings = GetNode<PanelContainer>("Settings Menu");
        PanelContainer main = GetNode<PanelContainer>("Main Menu");
        settings.Show();
		main.Hide();
	}

	
}
