using Godot;
using System;

public partial class EnemySpawner : Node2D
{
	[Export]
	private PackedScene EvilSpaceFish;
	private int nextspawntime;
	private Random random = new Random();
	private Timer timer;
	private bool Targetted = false;
	private int health = 20;
	EnemyManager enemyManager;
	int enemycounter = 0;

    [Signal]
    public delegate void FinishTaskEventHandler(int num);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        enemyManager = GetNode<EnemyManager>("/root/EnemyManager");
        nextspawntime = random.Next(3,10);
		timer = GetNode<Timer>("Timer");
		timer.WaitTime = nextspawntime;
		timer.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_timer_timeout()
	{
		GD.Print("Timeout");
		//Only the host can spawn
		if (!IsMultiplayerAuthority())
		{
			return;
		}
		//Spawn
		int count = GetChildren().Count;
		if(count < 8)
		{
          

            
            int sign1 = (random.Next(0,2)*2-1) * random.Next(-100, 100);
			int sign2 = (random.Next(0,2)*2-1) * random.Next(-100, 100);
            
            Rpc(nameof(spawnforOthers), sign1,sign2);
        }
		
        nextspawntime = random.Next(3, 10);
        timer.WaitTime = nextspawntime;
    }
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void spawnforOthers(int pos1, int pos2)
	{
        EvilSpaceFish fish;
		//Note call deffered caused problems due to the fish not being created in time and then crashing when a rpc appmtped to move it
		//Add child does it instantly so the timing matches up again
		//Seems to be the best of both worlds but do pay attention to this
        CallDeferred("add_child", fish = (EvilSpaceFish)EvilSpaceFish.Instantiate());
		//AddChild(fish = (EvilSpaceFish)EvilSpaceFish.Instantiate());
        fish.Position = new Vector2(pos1, pos2);
        fish.setPortalPos(GlobalPosition);
		//We will run out of names once the int limit is reached. That will require 70+ years worth of constant gameplay
		fish.Name = enemycounter.ToString();
		enemycounter = enemycounter + 1;
		enemyManager.addtoEnemyList(fish.Name);
		

    }

    public void takeDamage(int damage)
    {
		if(Targetted == true)
		{
            Rpc(nameof(RPCtakeDamge), damage);
           
        }
		
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCtakeDamge(int damage)
	{
        health = health - damage;
        if (health <= 0)
        {
            EmitSignal(SignalName.FinishTask,3);
            QueueFree();
        }
    }

	public void makeVunerable()
	{
		Targetted = true;

    }

	
}
