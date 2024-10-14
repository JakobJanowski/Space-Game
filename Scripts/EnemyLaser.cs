using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class EnemyLaser : Node2D
{
	//Needs to check target
	private Node2D Target = null;
    private Node2D ShootStart = null;
    private CharacterBody2D Turret;
	private float baseRotationAngle = 0;
	private Timer timer;
	private bool cooldown = false;
    private AudioStreamPlayer2D player;

    [Signal]
    public delegate void EnemyInRangeEventHandler(bool inRange);

    

    [Export]
    private PackedScene bullet;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Turret = GetNode<CharacterBody2D>("Turret");
        ShootStart = GetNode<Node2D>("Target");
        baseRotationAngle = Turret.RotationDegrees;
        timer = GetNode<Timer>("Timer");
        player = GetNode<AudioStreamPlayer2D>("Effect");

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Target != null)
		{
			//Turret look at target
			Turret.LookAt(Target.GlobalPosition);
            
            Rpc(nameof(moveTarget));
            if (IsMultiplayerAuthority())
            {
                if (cooldown == false)
                {
                    cooldown = true;
                    timer.Start(1.5);
                    Rpc(nameof(RpcShoot));
                }
            }
           
		}
		else
		{
			Turret.RotationDegrees = baseRotationAngle;

        }
	}
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcShoot()
	{
        player.Play();
        Bullet newbullet;
        GetNode("/root/World").AddChild(newbullet = (Bullet)bullet.Instantiate());

        newbullet.startmoving(ShootStart.GlobalPosition, Target.GlobalPosition, Turret.GlobalRotation, 0, 1);
        EffectManager effect = GetNode<EffectManager>("/root/EffectManager");
        effect.smokeEmmision(ShootStart.GlobalPosition, ShootStart.GlobalRotation);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void moveTarget()
    {
        int radius = 45;
        float xpos = (float)(Turret.Position.X + radius * Math.Cos(Turret.Rotation));
        float ypos = (float)(Turret.Position.Y + radius * Math.Sin(Turret.Rotation));
        ShootStart.Position = new Vector2(xpos, ypos);
        //GD.Print(spaceship.getRotationSpeed());
    }


    private void _on_gun_range_body_entered(Node2D body)
	{
        if (body.IsInGroup("SpaceShip"))
        {
            Target = body;
            EmitSignal(SignalName.EnemyInRange, true);
        }
        
    }

	private void _on_gun_range_body_exited(Node2D body) 
	{
        if (body.IsInGroup("SpaceShip"))
        {
            Target = null;
            EmitSignal(SignalName.EnemyInRange, false);
        }
    }

    private void _on_timer_timeout()
    {
        cooldown = false;
    }
}
