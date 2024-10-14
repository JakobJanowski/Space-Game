using Godot;
using System;
using System.Collections.Generic;

public partial class TaskHandler : Node2D
{

    private TaskMenu taskmenu;
    [Export]
    private PackedScene LostCrate;

    [Export]
    private PackedScene CrateDestination;
    //num is task type num, progess is task stage
    [Signal]
    public delegate void taskProgressEventHandler(int num, int progress);

    [Signal]
    public delegate void updateMapEventHandler(Node2D createdobj);

    [Signal]
    public delegate void TaskOverEventHandler();

    private List<Node2D> CrateSpawns = new List<Node2D>();
    private List<Node2D> Destinations = new List<Node2D>();

    private bool Hunting = false;
    private int Hunted = 0;
    //Copy to update visual
    private string huntingText = 
           "Defeat Hostiles\n" +
           "\n" +
           "Attacks in this area have grown too high. You've been tasked with eliminate a number of hostile entities to reduce their ability to conduct further attacks";

    Random random;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        random = new Random();
        taskmenu = GetNode<TaskMenu>("/root/World/Interface/TaskMenu");
        taskmenu.startTask += Taskmenu_startTask;

        Node2D list = GetNode<Node2D>("/root/World/Delivery Locations");
        foreach (var node in list.GetChildren())
        {
            Destinations.Add((Node2D)node);
        }
       

        list = GetNode<Node2D>("/root/World/Lost Crate Spawns");
        foreach (var node in list.GetChildren())
        {
            CrateSpawns.Add((Node2D)node);
        }
        
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    private void Taskmenu_startTask(int num)
    {
        if(IsMultiplayerAuthority())
        {
            switch (num)
            {
                case 1:
                    //Lost cargo
                    //Spawn cargo
                    LostCrate crate;
                    GetNode("/root/World").AddChild(crate = (LostCrate)LostCrate.Instantiate());
                    int randomnum = random.Next(0, CrateSpawns.Count);
                    crate.GlobalPosition = CrateSpawns[randomnum].GlobalPosition;
                    crate.taskProgress += progressTask;
                    Rpc(nameof(rpcTaskmenu_startTask), num, randomnum);
                    EmitSignal(SignalName.updateMap, crate);
                    break;
                case 2:
                    Hunting = true;
                    Rpc(nameof(rpcTaskmenu_startTask), num, 0);
                    break;

                case 3:
                    //Get the 
                    Node2D spanwers = GetNode<Node2D>("/root/World/EnemySpawners");
                    var children = spanwers.GetChildren();
                    int spawner = random.Next(0,(int)children.Count);
                    EnemySpawner chosenSpawn = (EnemySpawner)children[spawner];
                    chosenSpawn.makeVunerable();
                    chosenSpawn.FinishTask += finishTask;
                    EmitSignal(SignalName.updateMap, chosenSpawn);
                    Rpc(nameof(rpcTaskmenu_startTask), num, spawner);
                    //RPC
                    break;
                default:
                    //Just in case
                    break;
            }
        }
       
    }

   

    private void progressTask(int num, int progress)
    {
        if(IsMultiplayerAuthority()) 
        {
            switch (num)
            {
                case 1:
                    //Lost cargo
                    //Spawn cargo
                    DeliveryDestination dest;
                    //Note if it ever complains about calldefferd use this
                    GetNode("/root/World").CallDeferred("add_child", dest = (DeliveryDestination)CrateDestination.Instantiate());
                    int randomnum = random.Next(0, CrateSpawns.Count);
                    dest.GlobalPosition = CrateSpawns[randomnum].GlobalPosition;

                    dest.FinishTask += finishTask;
                    Rpc(nameof(rpcprogressTask), num,progress, randomnum);
                    EmitSignal(SignalName.taskProgress, 1, 1);
                    EmitSignal(SignalName.updateMap, dest);
                    break;
                default:
                    //Just in case
                    break;
            }
        }
        
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false,TransferMode =MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpcTaskmenu_startTask(int num,int randnum)
    {
        switch (num)
        {
            case 1:
                //Lost cargo
                //Spawn cargo
                LostCrate crate;
                GetNode("/root/World").AddChild(crate = (LostCrate)LostCrate.Instantiate());
                crate.GlobalPosition = CrateSpawns[randnum].GlobalPosition;
                crate.taskProgress += progressTask;
                EmitSignal(SignalName.updateMap, crate);
                break;
            case 2:
                Hunting = true;
                break;
            case 3:
                //Get the 
                Node2D spanwers = GetNode<Node2D>("/root/World/EnemySpawners");
                var children = spanwers.GetChildren();
                EnemySpawner chosenSpawn = (EnemySpawner)children[randnum];
                chosenSpawn.makeVunerable();
                chosenSpawn.FinishTask += finishTask;
                EmitSignal(SignalName.updateMap, chosenSpawn);
                
                break;
            default:
                //Just in case
                break;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpcprogressTask(int num,int progress,int randomnum)
    {
        switch (num)
        {
            case 1:
                //Lost cargo
                //Spawn cargo
                DeliveryDestination dest;
                //Note if it ever complains about calldefferd use this
                GetNode("/root/World").CallDeferred("add_child", dest = (DeliveryDestination)CrateDestination.Instantiate());
                dest.GlobalPosition = CrateSpawns[randomnum].GlobalPosition;

                dest.FinishTask += finishTask;
                EmitSignal(SignalName.taskProgress, 1, 1);
                EmitSignal(SignalName.updateMap, dest);
                break;
            default:
                //Just in case
                break;
        }
    }

    private void finishTask(int num)
    {
        GD.Print("Finished");
        switch (num)
        {
            case 1:
                EmitSignal(SignalName.taskProgress, 1, 2);
                break;
            case 2:
                EmitSignal(SignalName.taskProgress, 2, 1);
                Hunting = false;
                Hunted = 0;
                break;
            case 3:
                EmitSignal(SignalName.taskProgress, 3,1);
                EmitSignal(SignalName.TaskOver);
                break;
        }
        
    }

    public void fishdied()
    {
        if(Hunting == true)
        {
            Hunted = Hunted + 1;
            GetNode<RichTextLabel>("/root/World/Interface/TaskMenu/MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes/HSplitContainer/TaskDesLabel").Text =
                huntingText + "\n" + "\n" + Hunted + "/20";
            GetNode<AudioStreamPlayer>("/root/World/Interface/TaskMenu/Boing").Play();
            if(Hunted >= 20)
            {
                finishTask(2);
            }
        }
        
    }

}

