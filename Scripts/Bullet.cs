using Godot;
using System;

public partial class Bullet : Area2D
{
    [Export]
    private int speed = 800;
    [Export]
    private int damage = 1;

    private double maxdistance;
    private double currentdistance = 0;


    private float rotationdir;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        maxdistance = speed * 4;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
        float indepspeed = speed * (float)delta;
        Vector2 newvector = new Vector2(1, 0).Rotated(Rotation);
        Position = Position + (newvector * indepspeed);
        //Add on speed to current distance
        currentdistance = currentdistance + indepspeed;
        //Once it exceeds the maximum allowed distance destroy the bullet
        if (currentdistance > maxdistance)
        {
            QueueFree();
        }
    }

    public void startmoving(Vector2 startpos, Vector2 mousepos, float rotation,float rotDir,int Damage)
    {
        rotationdir = rotDir;
        Rotation = rotation;
        Position = startpos;
        //LookAt(mousepos);
        Rotation = Rotation + (rotDir*0.5f);
        damage = Damage;

    }

    private void _on_body_entered(PhysicsBody2D body)
    {
        if (body.IsInGroup("Enemy") || body.IsInGroup("SpaceShip") )
        {
            if (IsMultiplayerAuthority())
            {
                
                Entity entity = (Entity)body;
                entity.takeDamage(damage);
            }
           
        }
        else if (body.IsInGroup("EnemySpawn") || body.IsInGroup("Shield") || body.IsInGroup("Rock"))
        {
            if (IsMultiplayerAuthority())
            {
                StaticEntity entity = (StaticEntity)body;
                entity.takeDamage(damage);
            }
        }
        EffectManager effect = GetNode<EffectManager>("/root/EffectManager");
        effect.hitExplosion(GlobalPosition);
        //Destroy Bullet
        QueueFree();
    }
}
