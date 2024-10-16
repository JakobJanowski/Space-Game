using Godot;
using System;

public abstract partial class Weapon : Node2D
{//Is this character being controled, to avoid more than one person controling the same thing
    private bool controled = false;

    private bool othercontrolled = false;
    //This characters camera
    private Camera2D camera2D;
    //The moveable part of the turret
    private CharacterBody2D turret;

    private bool firing = false;

    [Export]
    private PackedScene bullet;

    private Spaceship spaceship;

    private Node2D Laser = null;
    private Node2D Target;

    private int damage = 1;
    private int shots = 1;
    private int shotspeed = 1;
    private int engineBonus = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        
    }

    public override void _ExitTree()
    {
       
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
       

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void Fire(Vector2 mousepos)
    {
       

    }
    //A workaround rpc not allowing for return
    private void updateLaserVariable(Node2D newLaser)
    {

        
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void stopFiring()
    {
        
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void moveTurret(Vector2 mousepos)
    {
      
    }

    //Toggle the current control
    public void _on_weapons_control_toggle_weapon_control(int weaponnum)
    {
        
    }




    public abstract bool getifControlled();

    public abstract void setDamage(int Damage);

    public abstract int getDamage();

    public abstract void setshots(int Shots);

    public abstract int getshots();

    public abstract void setshotspeed(int Shotspeed);

    public abstract int getshotspeed();
    public abstract void setEngineBonus(int EngineBonus);

    public abstract int getEngineBonus();

}
