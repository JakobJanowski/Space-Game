using Godot;
using System;


public partial class Display : GridContainer
{
	private long displayStatus;
	private Godot.Environment env;
    private AudioStreamPlayer player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
         env = GetNode<WorldEnvironment>("/root/GlobalWorldEnvironment/WorldEnvironment").Environment;
        player = GetNode<AudioStreamPlayer>("Click");
		upDateSettings();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	private void _on_vsync_button_toggled(bool button_pressed)
	{
        player.Play();
        if (button_pressed)
		{
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
		}
		else
		{
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
        }
	}

	private void _on_brightness_button_value_changed(float value)
	{
        env.AdjustmentBrightness = value;
		player.Play();

    }

	private void _on_reset_button_pressed()
	{
        env.AdjustmentBrightness = 1;
        DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
		HSlider bar = GetNode<HSlider>("BrightnessButton");
		bar.Value = 1;
		
    }

	private void upDateSettings()
	{
        HSlider bar = GetNode<HSlider>("BrightnessButton");
        bar.Value = env.AdjustmentBrightness;
		CheckButton button = GetNode<CheckButton>("VsyncButton");
		if(DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Enabled)
		{
			button.ButtonPressed = true;
		}
		else
		{
			button.ButtonPressed = false;
		}
    }

    private void _on_visibility_changed()
	{
		upDateSettings();

	}
}
