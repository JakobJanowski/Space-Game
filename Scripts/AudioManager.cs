using Godot;
using System;

public partial class AudioManager : Node2D
{
	

	//Volume is a percentage
	private float volume = 0.7f;
	//All music starts at -20db and up to +40 is added 20 is the default
	private float musicVolume = 1f;
	private float effectVoume = 1f;
    private AudioStreamPlayer player;
    private AudioStreamPlayer2D FishDeath;
    private AudioStreamPlayer2D RockDeath;
    private AudioStreamPlayer2D ShipDeath;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        upDateAudioBys();
        player = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        FishDeath = GetNode<AudioStreamPlayer2D>("FishDeath");
        RockDeath = GetNode<AudioStreamPlayer2D>("RockDeath");
        ShipDeath = GetNode<AudioStreamPlayer2D>("ShipDeath");
        connectButton(GetTree().Root);
        GetTree().NodeAdded += onNodeAdded;
    }

	public void setVolume(float _volume)
	{
		volume = _volume;
        upDateAudioBys();


    }

    public void setMusicVolume(float _volume)
    {
        musicVolume = _volume;
        upDateAudioBys();


    }

    public void setEffectVolume(float _volume)
    {
        effectVoume = _volume;
        upDateAudioBys();


    }

    public float getVolume()
    {
        return volume;
    }

    public float getMusicVolume()
    {
        return musicVolume;
    }

    public float getEffectVolume()
    {
        return effectVoume;
    }

    private void upDateAudioBys()
	{
        AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(volume));
        AudioServer.SetBusVolumeDb(1, Mathf.LinearToDb(musicVolume));
        AudioServer.SetBusVolumeDb(2, Mathf.LinearToDb(effectVoume));
    }

    //Code for connetion adapted to c# from here
    //https://gamedev.stackexchange.com/questions/184354/add-a-sound-to-all-the-buttons-in-a-project

    private void onNodeAdded(Node node)
    {
        if(node.GetType() == typeof(Button))
        {
            connectChild(node);
        }
    }

    private void connectButton(Node root)
    {
        foreach(Node child in root.GetChildren())
        {
            if(child.GetType() == typeof(Button))
            {
                connectChild(child);
            }
            connectButton(child);
        }
    }

    private void connectChild(Node child)
    {
        Button ch = (Button)child;
        ch.Pressed += Click;
    }

    private void Click()
    {
        player.Play();
    }

    public void playFishDeath(Vector2 pos)
    {
        FishDeath.GlobalPosition = pos;
        FishDeath.Play();
    }

    public void playRockDeath(Vector2 pos)
    {
        RockDeath.GlobalPosition = pos;
        RockDeath.Play();
    }

    public void playShipDeath(Vector2 pos)
    {
        ShipDeath.GlobalPosition = pos;
        ShipDeath.Play();
    }






}
