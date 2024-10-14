using Godot;
using System;

public partial class PlayerShield : StaticEntity
{
    [Export]
    private int health = 10;

    private bool working = true;
    private bool active = false;
    private Timer timer;
    private AudioStreamPlayer2D Break;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        Break = GetNode<AudioStreamPlayer2D>("Break");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void takeDamage(int damage)
    {

        if (IsMultiplayerAuthority())
        {
            Rpc(nameof(rpctakeDamage), damage);
        }

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpctakeDamage(int damage)
    {
        
        health -= damage;
        if (health <= 0)
        {
            if(working == true)
            {
                working = false;
                disableShield();
                timer.Paused = false;
                timer.Start(5);
            }
           
        }

      
       
        

    }

    public bool getIfWorking()
    {
        return working;
    }

    private void _on_timer_timeout()
    {
        health = 10;
        working = true;
        toggleExists();
        timer.Paused = true;
    }

    private void disableShield()
    {
        Break.Play();
        this.Hide();
        Node sh = GetChild(1);
        if(sh.Name == "CollisionShape2D")
        {
            CollisionShape2D collisionShape2D = sh as CollisionShape2D;
            collisionShape2D.CallDeferred("set", "disabled", true);
        }
        else
        {
            CollisionPolygon2D collision = sh as CollisionPolygon2D;
            collision.CallDeferred("set", "disabled", true);
        }
       
        
    }

    

    public void toggleExists()
    {
        if (active == true)
        {
            this.Show();
            Node sh = GetChild(1);
            if (sh.Name == "CollisionShape2D")
            {
                CollisionShape2D collisionShape2D = sh as CollisionShape2D;
                collisionShape2D.CallDeferred("set", "disabled", false);
            }
            else
            {
                CollisionPolygon2D collision = sh as CollisionPolygon2D;
                collision.CallDeferred("set", "disabled", false);
            }

        }
        else
        {
            Hide();
            Node sh = GetChild(1);
            if (sh.Name == "CollisionShape2D")
            {
                CollisionShape2D collisionShape2D = sh as CollisionShape2D;
                collisionShape2D.CallDeferred("set", "disabled", true);
            }
            else
            {
                CollisionPolygon2D collision = sh as CollisionPolygon2D;
                collision.CallDeferred("set", "disabled", true);
            }
        }
    }

    public void toggleActive(bool isactive)
    {
        if (isactive == false)
        {
            active = false;

        }
        else
        {
            active = true;
        }
    }
}
