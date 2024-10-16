using Godot;
using System;
using System.Reflection.Emit;

public partial class LaserBeamTuret : Weapon
{
    

    //Is this character being controled, to avoid more than one person controling the same thing
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

    private Resource crosshair = ResourceLoader.Load("res://Sprites/Crosshairs-PublicDomain/cross_whole.png");
    private Timer timer;
    private bool cooldown = false;
    private Godot.Label label;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        var toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl");
        toggleWeapons.ToggleWeaponControl += _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl2");
        toggleWeapons.ToggleWeaponControl += _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl3");
        toggleWeapons.ToggleWeaponControl += _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl4");
        toggleWeapons.ToggleWeaponControl += _on_weapons_control_toggle_weapon_control;
        camera2D = GetNode<Camera2D>("TurretCamera");
        turret = GetNode<CharacterBody2D>("Turret");
        spaceship = GetNode<Spaceship>("/root/World/Shapeship");
        Target = GetNode<Node2D>("Target");
        timer = GetNode<Timer>("Timer");
        label = GetNode<Godot.Label>("Turret/Label");

    }

    public override void _ExitTree()
    {
        var toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl");
        toggleWeapons.ToggleWeaponControl -= _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl2");
        toggleWeapons.ToggleWeaponControl -= _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl3");
        toggleWeapons.ToggleWeaponControl -= _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl4");
        toggleWeapons.ToggleWeaponControl -= _on_weapons_control_toggle_weapon_control;
        base._ExitTree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (cooldown == true)
        {
            label.Text = Math.Round(timer.TimeLeft, 2).ToString();
        }
        else
        {
            label.Text = "";
        }
        if (controled == true)
        {

            Vector2 mousepos = GetLocalMousePosition();
            //GD.Print(turret.Position.X - mousepos.X);
            //If the X of the turret posistion - the x of the mouse is less than 0
            //aka if the X of the mouse is above the turret then rotate
            //This limits rotation to 180 degrees
            //Note X is up down here i think fun times

            if ((turret.Position.X - mousepos.X) < 0)
            {
                Vector2 globalMousePos = GetGlobalMousePosition();
                
                var dist = mousepos.DistanceTo(Target.Position);
                if(dist < 500)
                {
                    Input.SetCustomMouseCursor(crosshair, Input.CursorShape.Arrow, new Vector2(16, 16));

                }
                else
                {
                    Input.SetCustomMouseCursor(null);
                }
                //If the hoist move turret normaly 
                if (IsMultiplayerAuthority())
                {
                    turret.LookAt(globalMousePos);
                    Target.Position = turret.Position + new Vector2((float)Math.Sin(turret.Rotation), (float)Math.Cos(turret.Rotation)) * (Target.Position - turret.Position);
                }
                //Else rpc to host to get them to move the turret
                else
                {
                    Rpc(nameof(moveTurret), globalMousePos);
                }

                //Since the turret is in a valid posistion we now check for shooting
                if (Input.IsActionPressed("LeftClick"))
                {
                    if(cooldown == false)
                    {
                        firing = true;
                        Rpc(nameof(Fire), mousepos);
                    }
                    else
                    {
                        if(Laser != null)
                        {
                            firing = false;
                            Rpc(nameof(stopFiring));
                        }
                    
                    }
                    
                  

                }
                else
                {
                    if(firing == true)
                    {
                        firing = false;
                        Rpc(nameof(stopFiring));
                    }
                    
                }
            }
            else
            {
                Input.SetCustomMouseCursor(null);
            }

        }
        else
        {

            if (firing == true)
            {
                firing = false;
                Rpc(nameof(stopFiring));
            }
        }

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void Fire(Vector2 mousepos)
    {
        if(Laser == null)
        {
            Laser newlaser;

            GetNode("/root/World/Shapeship").AddChild(newlaser = (Laser)bullet.Instantiate());
            
            newlaser.Position = Position;
            newlaser.Rotation = Rotation;
            newlaser.updateCastPoint(mousepos,Target.Position, turret.Rotation,damage);
            newlaser.hookUpCooldown(this);
            updateLaserVariable(newlaser);
        }
        else
        {
            Laser.Position = Position;
            Laser.Rotation = Rotation;
            Laser castLaser = (Laser)Laser;     
            castLaser.updateCastPoint(mousepos, Target.Position, turret.Rotation, damage);
        }
        

    }
    public void startCooldown()
    {
        GD.Print("started");
        cooldown = true;
        timer.Start(determineShotSpeed()); 
    }

    private double determineShotSpeed()
    {
        double val = 3 - (0.4 * shotspeed) - (0.1 * engineBonus);
        if (val <= 0)
        {
            val = 0.1;
        }
        return val;
    }

    //A workaround rpc not allowing for return
    private void updateLaserVariable(Node2D newLaser)
    {
       
        Laser = newLaser;
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void stopFiring()
    {
        Laser.QueueFree();
        Laser = null;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void moveTurret(Vector2 mousepos)
    {
        turret.LookAt(mousepos);
    }

    private void _on_timer_timeout()
    {
        cooldown = false;
    }

    //Toggle the current control
    public void _on_weapons_control_toggle_weapon_control(int weaponnum)
    {
        switch (weaponnum)
        {
            case 1:
                if (this.Name != "Weapon1")
                {
                    return;
                }
                break;
            case 2:
                if (this.Name != "Weapon2")
                {
                    return;
                }
                break;
            case 3:
                if (this.Name != "Weapon3")
                {
                    return;
                }
                break;
            case 4:
                if (this.Name != "Weapon4")
                {
                    return;
                }
                break;
        }
        if (controled == false)
        {

            controled = true;
            camera2D.MakeCurrent();
            Rpc(nameof(updateOtherControlled));

        }
        else
        {
            controled = false;
            Rpc(nameof(updateOtherControlled));
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void updateOtherControlled()
    {
        if (othercontrolled == false)
        {
            othercontrolled = true;
        }
        else
        {
            othercontrolled = false;
        }
    }

  
    public override bool getifControlled()
    {
        return othercontrolled;
    }

    public override void setDamage(int Damage)
    {
        damage = Damage;
    }

    public override void setshots(int Shots)
    {
        shots = Shots;
    }

    public override void setshotspeed(int Shotspeed)
    {
   
        shotspeed = Shotspeed;
    }

    public override void setEngineBonus(int EngineBonus)
    {
        engineBonus = EngineBonus;
    }

    public override int getDamage()
    {
        return damage;
    }

    public override int getshots()
    {
        return shots;
    }

    public override int getshotspeed()
    {
        return shotspeed;
    }

    public override int getEngineBonus()
    {
        return engineBonus;
    }
}
