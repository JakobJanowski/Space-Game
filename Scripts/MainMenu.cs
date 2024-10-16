using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

public partial class MainMenu : Control
{
	//Default Port
	private int PORT = 8549;
	[Export]
	private string address = "127.0.0.1";
	
	private PanelContainer mainMenu;
	private PanelContainer startMenu;
    private PanelContainer Lobby;
    private PanelContainer Attributions;
    private settings_menu settings;
    //If isSever then do not spawn in a player
    private bool isSever = false;
	private bool useUpnp = false;
	
	private AudioStreamPlayer audioStreamPlayer;
	private ENetMultiplayerPeer peer;
	private Upnp upnp;

	private PlayerManager playerManager;

	

    private AudioStreamPlayer player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		mainMenu = GetNode<PanelContainer>("MainMenu");
		startMenu = GetNode<PanelContainer>("Start Menu");
		Lobby = GetNode<PanelContainer>("Lobby");
        Attributions = GetNode<PanelContainer>("Attributions");
        settings = GetNode<settings_menu>("Settings Menu");
        playerManager = GetNode<PlayerManager>("/root/PlayerManager");
		audioStreamPlayer = GetNode<AudioStreamPlayer>("Music");
        player = GetNode<AudioStreamPlayer>("Click");
        audioStreamPlayer.Play();
		
        Multiplayer.PeerConnected += peerConnected;
        Multiplayer.PeerDisconnected += peerDisconnected;
        Multiplayer.ConnectedToServer += connectedToServer;
        Multiplayer.ConnectionFailed += connectionFailed;
       

    }

    

    private void connectionFailed()
    {
        GD.Print("Connection Failed");
    }

    private void connectedToServer()
    {
        GD.Print("Connected to Server");
    }
    
    private void peerDisconnected(long id)
    {
        GD.Print("Player Disconnected: " + id.ToString());
		if (Multiplayer.IsServer())
		{
			Rpc(nameof(playerDisconnected),id);
		}
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void playerDisconnected(long id)
	{
		playerManager.removeFromPlayerList(id);
		
		
        updatelobby();
    }

    private void peerConnected(long id)
    {
        GD.Print("Player Connected: " + id.ToString());

		//Host rpc
		if (Multiplayer.IsServer())
		{
			
			
			Rpc(nameof(newplayerConnected),playerManager.getPlayerList(),id);
		}
		

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void newplayerConnected(Godot.Collections.Dictionary<long,string> playerlist,long id)
	{
		//Update everyones playerlist
		playerManager.updatePlayerlist(playerlist);
		//If the id is not already in the hosts list
		if(!playerlist.ContainsKey(id))
		{
			//Add to everyones list
            playerManager.addToPlayerList(id);
			//Then if you dont already have a id
			if(playerManager.getYourPlayer() == null) 
			{
				//Set it
				playerManager.setYourPlayer(id.ToString());
			}
        }
        updatelobby();

        //Now everyones player list is accurate work everyone can work out their id,

    }



	public void _on_start_game_button_pressed()
	{
		mainMenu.Hide();
		startMenu.Show();
		
	}

	private void _on_back_button_pressed()
	{
		startMenu.Hide();
		mainMenu.Show();
	}

	private void _on_u_pn_p_button_pressed()
	{
		LineEdit portnum = GetNode<LineEdit>("Start Menu/LeftMargin/VBoxContainer/PortLineEdit");
		try
		{
			PORT = portnum.Text.Trim().ToInt();
		}
		catch(Exception ex)
		{
			PORT = 8549;
		}
		
		useUpnp = true;
		LaunchSever();
        
    }

	private void _on_without_upnp_pressed()
	{
		LineEdit portnum = GetNode<LineEdit>("Start Menu/LeftMargin/VBoxContainer/PortLineEdit");
		try
		{
			PORT = portnum.Text.Trim().ToInt();
		}
		catch (Exception ex)
		{
			PORT = 8549;
		}
		LaunchSever();
        
    }

	private void _on_local_button_pressed()
	{
		//Will alawys launch on default port
		LaunchSever();
		
	}
	//Launches the sever and the game
	private void LaunchSever()
	{
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(PORT, 8);
		if (error != Error.Ok)
		{
			GD.Print("Error: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		//Load into Lobby
		
		if(useUpnp == true)
		{
			upnpSetup();
			
		}
		//The severs player should always be 1 even if they dont have a real player
        playerManager.setYourPlayer("1");
        if (isSever == false)
		{
            //Add host to player list
            
            playerManager.addToPlayerList(1);
        }
        startMenu.Hide();
        Lobby.Show();
        updatelobby();

    }
   
    public void _on_join_game_button_pressed()
	{
		peer = new ENetMultiplayerPeer();
		
		LineEdit addressbox = GetNode<LineEdit>("MainMenu/MarginContainer/VBoxContainer/AddressEntry");
		LineEdit portnum = GetNode<LineEdit>("MainMenu/MarginContainer/VBoxContainer/PortEntry");
		try
		{
			PORT = portnum.Text.Trim().ToInt();
		}
		catch (Exception ex)
		{
			PORT = 8549;
		}
		var error = peer.CreateClient(addressbox.Text, PORT, 8);
		
		if (error != Error.Ok)
		{
			GD.Print("Error: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		isSever = false;
        
        mainMenu.Hide();
        Lobby.Show();
		updatelobby();
        //Handle peer conecting
    }

	public void _on_quit_game_button_pressed()
	{
		GetTree().Quit();
	}

	

	private void upnpSetup()
	{
		upnp = new Upnp();
		int discoverResult = 0;
		//Try to discover upnp, return if fail
		try
		{
			 discoverResult = upnp.Discover();
		}
		catch (Exception)
		{
			GD.Print("Somthing went wrong, UPNP discover result: " + discoverResult);
			return;
		}
		//Try to get gateway and verify its valid, return if unable
		try
		{
			upnp.GetGateway();
			upnp.GetGateway().IsValidGateway();
		}
		catch (Exception)
		{
			GD.Print("Invalid gateway");
			return;
		}
		int result = upnp.AddPortMapping(PORT);
		if(result != 0)
		{
			GD.Print("Failed to get port mapping");
			return;
		}
		GD.Print("Succsess ip:" + upnp.QueryExternalAddress());
	}

	public void _on_check_button_pressed()
	{
		player.Play();

        if (isSever == true)
		{
			isSever = false;
		}
		else
		{
			isSever = true;
		}
	}
	
	public void _on_load_world_button_pressed()
	{
		if (Multiplayer.IsServer())
		{
            Rpc(nameof(startGame));
        }
		
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void startGame()
    {
		
		//Starts the game for all players
        World scene = ResourceLoader.Load<PackedScene>("res://Scenes/World.tscn").Instantiate<World>();
        GetTree().Root.AddChild(scene);
		//Things for sever to handle
		if (Multiplayer.IsServer())
		{
			if(useUpnp == true)
			{
                scene.usingUPnP(upnp);

            }
            Multiplayer.MultiplayerPeer.RefuseNewConnections = true;

        }
		

		scene.closeServer += Scene_closeServer;
			
		//Call localy
		scene.spawnPlayer();

		audioStreamPlayer.StreamPaused = !audioStreamPlayer.StreamPaused;
        this.Hide();
    }

    private void Scene_closeServer()
    {
        //TODO look into it not causing 20 errors
        Rpc(nameof(rpcDestroyWorld));
		_on_cancel_sever_button_pressed();
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpcDestroyWorld()
	{
        EnemyManager enemy = GetNode<EnemyManager>("/root/EnemyManager");
        enemy.stopTimer();
        //Show menu
        Show();
        GetNode<MoneyManager>("/root/MoneyManager").resetMoney();
		GetNode<EnemyManager>("/root/EnemyManager").reset();
        GetNode<RockManager>("/root/RockManager").reset();
        audioStreamPlayer.StreamPaused = !audioStreamPlayer.StreamPaused;
        //Kill world
        World world = GetNode<World>("/root/World");
		world.QueueFree();
	}

    public void _on_cancel_sever_button_pressed()
	{
		if(Multiplayer.IsServer())
		{
			//Force all connected players out of the server
			Rpc(nameof(rpcLeverServer));

			//Seemingly this line does the trick
            peer.Host.Destroy();
            //Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
			playerManager.updatePlayerlist(new Godot.Collections.Dictionary<long, string>());
            playerManager.setYourPlayer(null);
            peer = null;
			
			Lobby.Hide();
			mainMenu.Show();
			useUpnp = false;
		}
		else
		{

			leaveServer();


        }
	}
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpcLeverServer()
	{
		leaveServer();

    }

    //Leaver Server
	private void leaveServer()
	{
        peer.Close();
        //peer.Host.Destroy();
        Multiplayer.MultiplayerPeer = null;
        playerManager.updatePlayerlist(new Godot.Collections.Dictionary<long, string>());
        playerManager.setYourPlayer(null);
		GetNode<MoneyManager>("/root/MoneyManager").resetMoney();
        peer = null;
        isSever = false;
        Lobby.Hide();
        mainMenu.Show();
    }
   
	private void updatelobby()
	{
		RichTextLabel playerlist = GetNode<RichTextLabel>("Lobby/HSplitContainer/MarginContainer/PlayerList");
		playerlist.Text = "Players:\n";
		foreach(var player in playerManager.getPlayerList()) 
		{
			playerlist.Text = playerlist.Text + player.Value + "\n";
			//GD.Print(playerManager.getPlayerNameID().IndexOf(playerManager.getYourPlayer()));
		}
	}

	private void _on_enter_name_button_pressed()
	{
        string text = GetNode<TextEdit>("Lobby/HSplitContainer/MarginContainer2/VBoxContainer/NameEdit").Text;

        Rpc(nameof(RPCsetPlayerName), playerManager.getYourPlayer(),text);
        
        
        updatelobby();

    }
	
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void RPCsetPlayerName(string id,string newname)
	{

		playerManager.updatePlayerName(long.Parse(id), newname);
		updatelobby();
    }
    
	private void _on_setting_button_pressed()
	{
		mainMenu.Hide();
		settings.Show();
	}

	private void _on_attributions_pressed()
	{
		mainMenu.Hide();
		Attributions.Show();
	}

	private void _on_go_back_to_meny_pressed()
	{
		mainMenu.Show();
		Attributions.Hide();
	}



}
