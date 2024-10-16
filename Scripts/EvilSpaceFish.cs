using Godot;
using System;


public partial class EvilSpaceFish : Entity
{
    [Signal]
    public delegate void FishDiedEventHandler();

    [Signal]
    public delegate void TargetFoundEventHandler();

    [Signal]
    public delegate void TargetLostEventHandler();

    private int health = 1;
    private Node2D Target;
    private Vector2 PatrolTarget;
    private const float speed = 100f;
    //Counter to start losing target
    private int counter = -1;
    private Vector2 PortalPos;
    private bool chasingShip = false;
    private bool nibbeled = false;
    private bool startmoving = false;
    private Timer timer;
    private EnemyManager enemyManager;
    private AnimatedSprite2D animatedSprite;
    private bool hasTargeted = false;

    //All entities other than the ship should be controlled by the host only
    public override void _Ready()
    {
        enemyManager = GetNode<EnemyManager>("/root/EnemyManager");
        PatrolTarget = GlobalPosition;
        timer = GetNode<Timer>("Timer");
        timer.WaitTime = 2;
        timer.Start();
        //This is interesting since im force connecting to it.
        //With id known this ealeir this is interesting

        TaskHandler handler = GetNode<TaskHandler>("/root/World/Environment/TaskHandler");
        FishDied += handler.fishdied;
        animatedSprite = GetNode<AnimatedSprite2D>("Sprite2D");

        TargetFound += GetNode<World>("/root/World").addTargets;
        TargetLost += GetNode<World>("/root/World").removeTargets;
    }

    public override void _ExitTree()
    {
        if(chasingShip == true)
        {
            EmitSignal(SignalName.TargetLost);
        }
        GetNode<AudioManager>("/root/AudioManager").playFishDeath(GlobalPosition);
    }

    public void setPortalPos(Vector2 portalPos)
    {
        PortalPos = portalPos;
    }

    public override void _PhysicsProcess(double delta)
    {
        //Only run for host, host will rpc anything important
        if (!IsMultiplayerAuthority() || startmoving == false)
        {
            return;
        }
        if (chasingShip == true)
        {
            if (counter > 0)
            {
                counter = counter - 1;
            }
            if (nibbeled == false)
            {
                LookAt(Target.Position);
                var direction = GlobalPosition.DirectionTo(Target.Position);
                Velocity = direction * speed;
                MoveAndSlide();
                Rpc(nameof(updatePosistion), Position, Target.Position, nibbeled);
            }
            else
            {

                LookAt(Target.Position);
                RotationDegrees = RotationDegrees + 180;
                var direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
                direction = new Vector2(-direction.X, -direction.Y);
                direction.Rotated(Rotation);
                Velocity = direction * speed;
                MoveAndSlide();
                Rpc(nameof(updatePosistion), Position, Target.Position, nibbeled);


            }
            if(counter <= 250 && counter != -1)
            {
                if(hasTargeted == true)
                {
                    //RPC this
                    Rpc(nameof(RPCTargetLost));
                }
            }
            if (counter == 0)
            {
                
                Target = null;
                PatrolTarget = GlobalPosition;
                chasingShip = false;
                counter = -1;
                
            }
        }
        else
        {
            if (GlobalPosition.DistanceTo(PatrolTarget) > 10)
            {
                LookAt(PatrolTarget);
                var direction = GlobalPosition.DirectionTo(PatrolTarget);
                Velocity = direction * speed;
                MoveAndSlide();
                Rpc(nameof(updatePosistion), Position, PatrolTarget, nibbeled);
            }
           
            
           
        }


    }

    private void _on_timer_timeout()
    {
        //Perminantly set to true
        startmoving = true;
        if (chasingShip == false)
        {
            Random random = new Random();
            int num = random.Next(0, 15);

            //Target will be random location close to the spawn
            float sign1 = (random.Next(0, 2) * 2 - 1) * random.Next(-150, 150);
            float sign2 = (random.Next(0, 2) * 2 - 1) * random.Next(-150, 150);
            sign1 = sign1 + PortalPos.X;
            sign2 = sign2 + PortalPos.Y;
            PatrolTarget = new Vector2(sign1, sign2);
            timer.Start(num);


        }
        else
        {
            nibbeled = false;
            Random random = new Random();
            int num = random.Next(0, 15);
            timer.Start(num);
        }

    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void updatePosistion(Vector2 pos, Vector2 target, bool nib)
    {
        if(animatedSprite.IsPlaying() == false)
        {
            animatedSprite.Play("Moving");
        }
       
        LookAt(target);
        if (nib == true)
        {
            
            RotationDegrees = RotationDegrees + 180;
        }
        Position = pos;
        
    }

    public override void takeDamage(int damage)
    {
        Rpc(nameof(rpctakeDamage));
        
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpctakeDamage()
    {
        health = health - 1;
        if (health <= 0)
        {
            enemyManager.removefromEnemyList(Name);
            EmitSignal(SignalName.FishDied);
            QueueFree();
            
        }
    }

    private void _on_detection_radius_body_entered(Node2D body)
    {
        
        if (body.IsInGroup("SpaceShip"))
        {
            chasingShip = true;
            if(hasTargeted == false)
            {
                hasTargeted = true;
                EmitSignal(SignalName.TargetFound);
            }
           
            Target = body;
            counter = -1;
        }
    }

    private void _on_detection_radius_body_exited(Node2D body)
    {
        if (body.IsInGroup("SpaceShip"))
        {
            counter = 500;
           
        }
    }

    private void _on_nibble_radius_body_entered(Node2D body)
    {
        
        if (body.IsInGroup("SpaceShip"))
        {
            nibbeled = true;
            Spaceship ship = (Spaceship)body;
            ship.takeDamage(1);

            timer.Start(3);
        }
        else if (body.IsInGroup("Shield"))
        {
            nibbeled = true;
            timer.Start(3);
            StaticEntity entity = (StaticEntity)body;
            entity.takeDamage(1);
        }
    }

    private void _on_nibble_animation_radius_body_entered(Node2D body)
    {
      
        animatedSprite.Stop();
        animatedSprite.Play("Bite");
     
           
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCTargetLost()
    {
        EmitSignal(SignalName.TargetLost);
        hasTargeted = false;
    }




}
