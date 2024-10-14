using Godot;
using System;
using System.Collections.Generic;

public partial class SpaceRock : StaticEntity
{
    //TODO scale health with size
	private int health = 5;
    private RockManager rockManager = null;
    private int size = 0;
    private Vector2 moveVector = new Vector2();
    private Random random = new Random();
    private Spaceship spaceship;
    private bool moving = false;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        health = (int)(Scale.X * 2);
        spaceship = GetNode<Spaceship>("/root/World/Shapeship");
        switch (Scale.X)
        {
            case 50:
                size = 0;
                break;
            case 25:
                size = 1;
                break;
            case 10:
                size = 2;
                break;
            case 5:
                size = 3;
                break;
            case 1:
                size = 4;
                break;
            default:
                size = -1;
                break;
        }
        //To deal with issues relating to spawning in rock will be hidden until the first frame it moves
        Hide();

        //Rocks will slowly move in a random direction
        //Host rolls and rpcs the vector to everyone else
        //These are static so the player cant mess with this
        rockManager = GetNode<RockManager>("/root/RockManager");
        
        if (IsMultiplayerAuthority())
        {
            moveVector = new Vector2(random.Next(-1,1),random.Next(-1,1));
            Timer timer = GetNode<Timer>("Timer");
            //Work around for rpc issues
            timer.Start(0.2);
        }
    }
   

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        
        KinematicCollision2D movement = MoveAndCollide(moveVector/Scale);
        //Collision of some kind
        if(movement != null)
        {
            //Flip the movement
            moveVector = new Vector2(moveVector.X*-1, moveVector.Y*-1);
        }
        if(moving == true)
        {
            Rpc(nameof(rpcUpdatePos), GlobalPosition);
        }
       
	}
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpcUpdatePos(Vector2 pos)
    {
        GlobalPosition = pos;
        if(Visible == false)
        {
            Show();
        }

    }

    private void _on_timer_timeout()
    {
        Show();
        moving = true;
    }

    public override void takeDamage(int damage)
    {
        if(!IsMultiplayerAuthority())
        {
            return;
        }
        health = health - damage;
        int num = 0;
        if(health - damage <= 0)
        {
           
            
            num = random.Next(1,6);
            for(int i = 0;i < num; i++)
            {
  
                //Make sure spawn isnt inside of ship
                Vector2 pos = new Vector2(GlobalPosition.X+random.Next(-1,1),GlobalPosition.Y + random.Next(-1, 1));
                
                Rpc(nameof(RPCMakeRock), pos);
            }
            Rpc(nameof(RPCtakeDamge));
        }
       
        

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCtakeDamge()
    {
        EffectManager effect = GetNode<EffectManager>("/root/EffectManager");
        effect.RockExplode(GlobalPosition,this.Scale);
        GetNode<AudioManager>("/root/AudioManager").playRockDeath(GlobalPosition);
          QueueFree();
        
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCMakeRock(Vector2 Pos)
    {
        
        rockManager.createRock(Pos,size + 1);
    }

}
