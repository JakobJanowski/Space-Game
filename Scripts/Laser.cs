using Godot;
using System;
using System.Security.AccessControl;

public partial class Laser : Node2D
{
    //Code based on https://www.youtube.com/watch?v=dg0CQ6NPDn8
    //I translated it to c#

    private Line2D line;
    private RayCast2D rayCast;
    private Vector2 CastPoint;
    private Vector2 Startpos;
    private GpuParticles2D impactParticle;
    private GpuParticles2D laserSparcle;

    private int damage = 1;
    private int shotspeed = 5;
    private int engineBonus = 1;
    private bool cooldown = false;
    private AudioStreamPlayer2D effect;
    private LaserBeamTuret creator;

    [Signal]
    public delegate void hitSomthingEventHandler();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rayCast = GetNode<RayCast2D>("LaserRayCast");
        line = GetNode<Line2D>("Line2D");
        impactParticle = GetNode<GpuParticles2D>("HitPoint");
        laserSparcle = GetNode<GpuParticles2D>("LaserSpark");
        effect = GetNode<AudioStreamPlayer2D>("Effect");
        effect.Play();
        

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        if (!cooldown)
        {
            var castPoint = CastPoint;

            //If collide then use collide
            //If mouse is closer than collide use mouse

            var points = line.Points;

            points[0] = castPoint;
            points[1] = Startpos;

            var dist = Startpos.DistanceTo(castPoint);

            //If beyond range do up to range
            if (dist >= 500)
            {
                points[0] = rayCast.TargetPosition.Rotated(rayCast.Rotation);
            }
            //If hitting somthing do up to there

            if (rayCast.IsColliding())
            {
                //If collide then use collide
                //If mouse is closer than collide use mouse
                var collision = rayCast.ToLocal(rayCast.GetCollisionPoint()).Rotated(rayCast.Rotation);
                var dist2 = Startpos.DistanceTo(collision);


                if (dist > dist2)
                {
                    points[0] = collision;
                    Node2D collidedObject = (Node2D)rayCast.GetCollider();
                    if(cooldown == false)
                    {
                        if (collidedObject.IsInGroup("Enemy") || collidedObject.IsInGroup("EnemySpawn"))
                        {
                            if (IsMultiplayerAuthority())
                            {
                                if (collidedObject.IsInGroup("Enemy"))
                                {
                                    Entity entity = (Entity)collidedObject;
                                    entity.takeDamage(damage);
                                    cooldown = true;
                                    Timer timer = GetNode<Timer>("Timer");

                                    timer.Start(determineShotSpeed());
                                    Rpc(nameof(hideLaser), collidedObject.GlobalPosition);
          
                                    EffectManager effect = GetNode<EffectManager>("/root/EffectManager");
                                    effect.hitExplosion(collidedObject.GlobalPosition);
                                    EmitSignal(SignalName.hitSomthing);

                                }
                                else if (collidedObject.IsInGroup("EnemySpawn") || collidedObject.IsInGroup("Shield") || collidedObject.IsInGroup("Rock"))
                                {
                                    StaticEntity entity = (StaticEntity)collidedObject;
                                    entity.takeDamage(damage);
                                    cooldown = true;
                                    Timer timer = GetNode<Timer>("Timer");
                                    timer.Start(determineShotSpeed());
                                    Rpc(nameof(hideLaser), collidedObject.GlobalPosition);
                                    EffectManager effect = GetNode<EffectManager>("/root/EffectManager");
                                    effect.hitExplosion(collidedObject.GlobalPosition);
                                    EmitSignal(SignalName.hitSomthing);
                                }


                            }
                        }
                    }
                   

                }



            }
            line.Points = points;
            //Only render effects if line is visable
            if (line.Visible)
            {
                impactParticle.Position = points[0];
                laserSparcle.Position = new Vector2(points[0].X * 0.5f, points[0].Y * 0.5f);
                laserSparcle.Rotation = rayCast.Rotation;
                ParticleProcessMaterial mat = (ParticleProcessMaterial)laserSparcle.ProcessMaterial;
                mat.EmissionBoxExtents = new Vector3(points[0].Length() * 0.5f, 1, 1);
            }
           
            
        }
        else
        {
            line.Hide();
        }
        
    }

    private double determineShotSpeed()
    {
        return 3 - ((0.2 * engineBonus) * (0.5 * shotspeed));
    }

    public void updateCastPoint(Vector2 Pos,Vector2 startpos,float Rot,int Damage) 
    {
        CastPoint = Pos;
        Startpos = startpos;
        rayCast.Rotation = Rot;
        damage = Damage;
       
       
        
      
    }

    public void hookUpCooldown(LaserBeamTuret Creator)
    {
        //Force connect to creator
        creator = Creator;
        hitSomthing += creator.startCooldown;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void hideLaser(Vector2 effectpos)
    {
        EmitSignal(SignalName.hitSomthing);
        EffectManager effect = GetNode<EffectManager>("/root/EffectManager");
        effect.hitExplosion(effectpos);
        cooldown = true;
        Timer timer = GetNode<Timer>("Timer");

        timer.Start(determineShotSpeed());
        line.Hide();

    }

    private void _on_timer_timeout()
    {
       
        cooldown = false;
        line.Show();
    }

    //Todo Use tween to animate
}