using Godot;
using System;

public partial class AudioMenu : GridContainer
{
    private AudioManager audioManager;
    private float masterVolume = 0.7f;
	private float effectVolume = 1;
    private float musicVolume = 1;
	private AudioStreamPlayer player;
    //Will handle agjusting the audio with the audio manager
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{ 
        audioManager = GetNode<AudioManager>("/root/AudioManager");
		player = GetNode<AudioStreamPlayer>("Click");
		upDateSettings();

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_master_volume_slider_value_changed(float value)
	{
		masterVolume = value;
		audioManager.setVolume(masterVolume);
		player.Play();
	}

	private void _on_music_volume_slider_value_changed(float value)
	{
		musicVolume = value;
		audioManager.setMusicVolume(musicVolume);
        player.Play();
    }

	private void _on_effect_volume_slider_value_changed(float value)
	{
		effectVolume = value;
		audioManager.setEffectVolume(effectVolume);
        player.Play();
    }

	private void _on_reset_button_pressed()
	{
        masterVolume = 0.7f;
		effectVolume = 1f;
		musicVolume = 1f;
        audioManager.setVolume(masterVolume);
        audioManager.setMusicVolume(musicVolume);
        audioManager.setEffectVolume(effectVolume);
        HSlider bar = GetNode<HSlider>("MasterVolumeSlider");
		bar.Value = 0.7f;
        bar = GetNode<HSlider>("MusicVolumeSlider");
        bar.Value = 1f;
        bar = GetNode<HSlider>("EffectVolumeSlider");
        bar.Value = 1f;
    }

	private void upDateSettings()
	{
        HSlider bar = GetNode<HSlider>("MasterVolumeSlider");
		bar.Value = audioManager.getVolume();
        bar = GetNode<HSlider>("MusicVolumeSlider");
        bar.Value = audioManager.getMusicVolume();
        bar = GetNode<HSlider>("EffectVolumeSlider");
        bar.Value = audioManager.getEffectVolume();
    }

	private void _on_visibility_changed()
	{
		upDateSettings();

    }
}
