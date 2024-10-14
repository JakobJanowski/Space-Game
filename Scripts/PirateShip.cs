using Godot;
using System;
using System.Diagnostics.Metrics;
using System.Reflection;
using static Godot.TextServer;

public partial class PirateShip : Entity
{
    private Node2D Target;
    private int numInRange = 0;
    private float idealDist = 1000;
    private float minDist = 900;
    int health = 50;

    private PlayerShield shield1;
    private PlayerShield shield2;
    private PlayerShield shield3;
    private AnimatedSprite2D animatedSprite;
    private bool startedMoving = false;

    private AudioStreamPlayer2D engine;

    int activeshield = 0;

    [Signal]
    public delegate void TargetFoundEventHandler();

    [Signal]
    public delegate void TargetLostEventHandler();

    private Vector2 Startpos;
    private Vector2 PatrolTarget;
    private Timer timer;


    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        shield1 = GetNode<PlayerShield>("Shield1");
        shield2 = GetNode<PlayerShield>("Shield2");
        shield3 = GetNode<PlayerShield>("Shield3");
        //Turn off all shields
        shield1.toggleExists();
        shield2.toggleExists();
        shield3.toggleExists();
        animatedSprite = GetNode<AnimatedSprite2D>("Sprite2D");
        engine = GetNode<AudioStreamPlayer2D>("Engine");
        engine.StreamPaused = true;
        TargetFound += GetNode<World>("/root/World").addTargets;
        TargetLost += GetNode<World>("/root/World").removeTargets;
        Startpos = GlobalPosition;
        PatrolTarget = GlobalPosition; 
    }

    public override void _ExitTree()
    {
        if (Target != null)
        {
            EmitSignal(SignalName.TargetLost);
        }
    }


    public override void _Process(double delta)
    {
        //If target is close enough move within range
        //Make sure guns are pointing to it
        //Guns are in the side so aim to point in that direction
        //Its about 110 degress i suspect
        //Keep shield up in correct direction

        //Only host move this
        if(IsMultiplayerAuthority())
        {
            //If target is within range
            if(Target != null)
            {
                if(engine.StreamPaused == true)
                {
                    engine.StreamPaused = false;
                }
                //GD.Print(GlobalPosition.DirectionTo(Target.GlobalPosition));
                //GD.Print(GlobalPosition.DistanceTo(Target.GlobalPosition));
                //Move towards target to be within ideal but above min dist
                if (GlobalPosition.DistanceTo(Target.GlobalPosition) > idealDist)
                {
                    //Spin to face the ship then move forward
                    float direction = GlobalPosition.AngleToPoint(Target.GlobalPosition);
                   
                    //GD.Print(GlobalRotation);
                    if (!(GlobalRotation >= (direction - 0.25) && GlobalRotation <= (direction + 0.25)))
                    {
                        //double directionDegrees = Math.Abs(direction * (180 / Math.PI));
                        
                        //Rotate towards player
                        Rotation = Mathf.LerpAngle(GlobalRotation, direction, 0.0085f);
                        if(direction > 0)
                        {
                            if (startedMoving == false)
                            {
                                animatedSprite.Stop();
                                animatedSprite.Play("Spin1");
                                Rpc(nameof(rpcPlayAnimation), "Spin1");
                            }
                            
                        }
                        else
                        {
                            if (startedMoving == false)
                            {
                                animatedSprite.Stop();
                                animatedSprite.Play("Spin2");
                                Rpc(nameof(rpcPlayAnimation), "Spin2");
                            }
                           
                        }
                        Velocity = new Vector2(0, 0);
                        //startedMoving = false;

                    }
                    else
                    {
                        //Move towards player
                        Velocity = GlobalPosition.DirectionTo(Target.GlobalPosition) * 150;
                        MoveAndSlide();
                    }

                }
                else if (GlobalPosition.DistanceTo(Target.GlobalPosition) <= minDist)
                {
                    //Move away from players and rotate to shoot them
                    Velocity = -(GlobalPosition.DirectionTo(Target.GlobalPosition)) * 150;
                    MoveAndSlide();
                    float direction = (float)(GlobalPosition.AngleToPoint(Target.GlobalPosition) + 1.85f);
                    //Spin me right round 
                    if (numInRange < 2)
                    {

                        Rotation = Mathf.LerpAngle(GlobalRotation, direction, 0.005f);
                        if (direction > 0)
                        {
                            if(startedMoving == false)
                            {
                                animatedSprite.Stop();
                                animatedSprite.Play("Spin1");
                                Rpc(nameof(rpcPlayAnimation), "Spin1");
                            }
                            
                        }
                        else
                        {
                            if (startedMoving == false)
                            {
                                animatedSprite.Stop();
                                animatedSprite.Play("Spin2");
                                Rpc(nameof(rpcPlayAnimation), "Spin2");
                            }
                          
                        }

                    }
                }
                else
                {
                    float direction = (float)(GlobalPosition.AngleToPoint(Target.GlobalPosition) +1.85f);
                    //Spin me right round 
                    if (numInRange < 2)
                    {
                        //Rotate to shoot at the players, this does a good enough job at it  
                        Rotation = Mathf.LerpAngle(GlobalRotation, direction, 0.005f);
                        if (direction > 0)
                        {
                            if (startedMoving == false)
                            {
                                animatedSprite.Stop();
                                animatedSprite.Play("Spin1");
                                Rpc(nameof(rpcPlayAnimation), "Spin1");
                            }
                            
                        }
                        else
                        {
                            if (startedMoving == false)
                            {
                                animatedSprite.Stop();
                                animatedSprite.Play("Spin2");
                                Rpc(nameof(rpcPlayAnimation), "Spin2");
                            }
                           
                        }
                        Velocity = new Vector2(0, 0);




                    }
                    else
                    {
                        Velocity = new Vector2(50, 0).Rotated(Rotation);
                        MoveAndSlide();
                        //Slowly move forward
                    }
                }
                Rpc(nameof(RPCMoveShip), GlobalPosition, GlobalRotation);
                //Animation
                //Animation Time
                if (Velocity.X != 0 || Velocity.Y != 0)
                {

                    if (startedMoving == false)
                    {
                        animatedSprite.Stop();
                        startedMoving = true;
                        animatedSprite.Play("Inbetween");
                        Rpc(nameof(rpcPlayAnimation), "Inbetween");

                    }
                    else
                    {
                        if (animatedSprite.IsPlaying() == false)
                        {
                            animatedSprite.Play("Moving");
                            Rpc(nameof(rpcPlayAnimation), "Moving");
                        }
                    }
                }
                else
                {
                    if (startedMoving == true)
                    {
                        animatedSprite.Stop();
                        startedMoving = false;
                        animatedSprite.PlayBackwards("Inbetween");
                        Rpc(nameof(rpcPlayAnimation), "InbetweenR");
                    }
                    else
                    {
                        if (animatedSprite.IsPlaying() == false)
                        {
                            animatedSprite.Play("default");
                            Rpc(nameof(rpcPlayAnimation), "default");
                        }
                    }
                }


                //Shield Up
                var Nodes = GetNode<Node2D>("Points").GetChildren();
                int count = 0;
                int num = 0;
                double dist = Mathf.Inf;
                foreach(Node2D Node in Nodes) 
                {
                    count = count + 1;
                    double dist2 = Node.GlobalPosition.DistanceTo(Target.GlobalPosition);
                    if(dist2 < dist) 
                    {
                        dist = dist2;
                        num = count;

                    }
                }
                //By default 0 will turn off shield otherwise turn on
                if(num != activeshield)
                {
                    activeshield = num;
                    Rpc(nameof(RPCTurnonShield), num);
                }
                
            }
            else
            {
                if (GlobalPosition.DistanceTo(PatrolTarget) > 10)
                {
                    LookAt(PatrolTarget);
                    var direction = GlobalPosition.DirectionTo(PatrolTarget);
                    Velocity = direction * 50f;
                    MoveAndSlide();
                    Rpc(nameof(RPCMoveShip), GlobalPosition, GlobalRotation);
                }
                if (engine.StreamPaused == false)
                {
                    engine.StreamPaused = true;
                }
            }
        }
    }

    private void _on_timer_timeout()
    {
       
        var random = new Random();
        //Pick a number that is either -1 or 1 * a random number between 0 and 1000, that is the posistion we are aiming for
        float sign1 = (random.Next(0, 2) * 2 - 1) * random.Next(0,1001);
        float sign2 = (random.Next(0, 2) * 2 - 1) * random.Next(0, 1001);
        PatrolTarget = new Vector2(Startpos.X+sign1,Startpos.Y+sign2);
        GD.Print(PatrolTarget);
        timer.Start(10);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void rpcPlayAnimation(string name)
    {
        //Stop what your doing and play something new
        //animatedSprite.Stop();
        if(name == "InbetweenR")
        {
            animatedSprite.PlayBackwards("Inbetween");
        }
        else
        {
            animatedSprite.Play(name);
        }
       
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void RPCMoveShip(Vector2 pos,float rot)
    {
        GlobalPosition = pos;
        GlobalRotation = rot;
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RPCTurnonShield(int num)
    {
        shield1.toggleActive(false);
        shield2.toggleActive(false);
        shield3.toggleActive(false);
        shield1.toggleExists();
        shield2.toggleExists();
        shield3.toggleExists();
        //Toggle the active shield
        switch (num)
        {
            case 1:
                shield1.toggleActive(true);
                shield1.toggleExists();
                break;
            case 2:
                shield2.toggleActive(true);
                shield2.toggleExists();
                break;
            case 3:
                shield3.toggleActive(true);
                shield3.toggleExists();
                break;
            default:
                
                break;
        }
    }


    private void _on_area_2d_body_entered(Node2D body)
	{
        if (body.IsInGroup("SpaceShip"))
        {
            Target = body;
            EmitSignal(SignalName.TargetFound);
        }
    }

	private void _on_area_2d_body_exited(Node2D body)
	{
        if (body.IsInGroup("SpaceShip"))
        {
            Target = null;
            EmitSignal(SignalName.TargetLost);
        }
    }

    private void _on_laser_1_enemy_in_range(bool inRange)
    {
        toggleInRange(inRange);
    }

    private void _on_laser_2_enemy_in_range(bool inRange)
    {
        toggleInRange(inRange);
    }

    private void _on_laser_3_enemy_in_range(bool inRange)
    {
        toggleInRange(inRange);
    }

    private void toggleInRange(bool inRange)
    {
        if(inRange == true)
        {
            numInRange = numInRange + 1;
        }
        else
        {
            numInRange = numInRange - 1;
        }
    }

    public override void takeDamage(int damage)
    {
       
        Rpc(nameof(rpctakeDamage), damage);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpctakeDamage(int damage)
    {
        health = health - damage;
        GD.Print(health);
        if (health <= 0)
        {
            EffectManager effect = GetNode<EffectManager>("/root/EffectManager");
            effect.deathExplosion(GlobalPosition);
            GetNode<AudioManager>("/root/AudioManager").playShipDeath(GlobalPosition);
            QueueFree();

        }
    }
}
