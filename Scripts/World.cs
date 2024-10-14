using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class World : Node2D
{
	private int PORT = 8549;
	private bool isUPnP = false;
	private Upnp upnp = null;
    private PlayerManager playerManager;
	private Spaceship shapeship;

	private AudioStreamPlayer Music1;
    private AudioStreamPlayer Music2;
    private AudioStreamPlayer Music3;
    private AudioStreamPlayer BattleMusic;

    private bool inBattle = false;
	private int targets = 0;

    [Signal]
	public delegate void closeServerEventHandler();


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

        playerManager = GetNode<PlayerManager>("/root/PlayerManager");
        Multiplayer.PeerDisconnected += peerDisconnected;
        shapeship = GetNode<Spaceship>("Shapeship");
        shapeship.GameOver += Shapeship_GameOver;
		EnemyManager enemy = GetNode<EnemyManager>("/root/EnemyManager");
		enemy.startTimer();
		Music1 = GetNode<AudioStreamPlayer>("Music1");
        Music2 = GetNode<AudioStreamPlayer>("Music2");
        Music3 = GetNode<AudioStreamPlayer>("Music3");
        BattleMusic = GetNode<AudioStreamPlayer>("BattleMusic");
		BattleMusic.Play();
		BattleMusic.StreamPaused = true;
        playMusic();

    }

    private void Shapeship_GameOver()
    {
		//Do cool shit
		
		EmitSignal(SignalName.closeServer);
		
    }

	public void triggerGameOver()
	{
		Shapeship_GameOver();

    }

    private void peerDisconnected(long id)
	{
		GD.Print("Player Disconnected: " + id.ToString());
		//Bit ugly way todo it, maybe use playermanager instead?
		Node2D disconnectedPlayer = GetNodeOrNull<Playertest2>("/root/World/Shapeship/"+id.ToString());
		if(disconnectedPlayer != null)
		{
			disconnectedPlayer.QueueFree();
		}
		//Add somthing to remove the player character
	}

	
	

	public void usingUPnP(Upnp _upnp)
	{
		isUPnP = true;
		upnp = _upnp;
	}

	

	public void spawnPlayer()
	{
		
		
        
        foreach (var player in playerManager.getPlayerList())
		{
            var playerCharacter = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn").Instantiate<Playertest2>();
           
			playerCharacter.Name = player.Key.ToString();
			/*
			string name = playerManager.getspecificPlayerName(player.ToString());
			
			if(name != "")
			{
                //playerCharacter.setPlayerName(name);
            }
            */
            shapeship.AddChild(playerCharacter, true);
			playerCharacter.Position = new Vector2(0,0);
			playerCharacter.setPlayerName(player.Value);
			
        }
		
		
		

		if (!playerManager.getPlayerList().ContainsKey(1))
		{
            if (GetNodeOrNull<Playertest2>("Shapeship/1") != null)
            {
                var hostToRemove = GetNode<Playertest2>("Shapeship/1");
                hostToRemove.QueueFree();

            }
        }


	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if ((shapeship.getHealth() <= 0))
        {
			GD.Print("Game over");
			//QueueFree();
        }
		if(Music1.Playing == false && Music2.Playing == false && Music3.Playing == false && BattleMusic.Playing == false)
		{
			playMusic();

        }

    }

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			if (isUPnP == true)
			{
				upnp.DeletePortMapping(PORT);
			}
			GetTree().Quit(); // default behavior
		}
			
	}

	private void _on_music_1_finished()
	{
		playMusic();
    }

	private void _on_music_2_finished()
	{
        playMusic();
    }

	private void _on_music_3_finished()
	{
        playMusic();
    }

	private void playMusic()
	{
        var random = new Random();
        int num = random.Next(0, 3);
        switch (num)
        {
            case 0:
                Music1.Play();
                break;
            case 1:
                Music2.Play();
                break;
            case 2:
                Music3.Play();
                break;
        }
    }

	public void addTargets()
	{
		
		targets = targets + 1;
		toggleBattleMusic();

    }

	public void removeTargets() 
	{
        
        targets = targets - 1;
        toggleBattleMusic();
    }

	private void toggleBattleMusic()
	{
        GD.Print(targets);
        if (targets > 0)
		{
			Music1.StreamPaused = true;
            Music2.StreamPaused = true;
            Music3.StreamPaused = true;
			BattleMusic.StreamPaused = false;
        }
		else
		{
            Music1.StreamPaused = false;
            Music2.StreamPaused = false;
            Music3.StreamPaused = false;
            BattleMusic.StreamPaused = true;
        }
	}

}
