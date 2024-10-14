using Godot;
using Godot.Collections;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public partial class LaserTuret : Weapon
{
    //Is this character being controled, to avoid more than one person controling the same thing
    private bool controled = false;

    private bool othercontrolled = false;
    //This characters camera
    private Camera2D camera2D;
    //The moveable part of the turret
    private CharacterBody2D turret;

    private bool cooldown = false;

    [Export]
    private PackedScene bullet;
    private Spaceship spaceship;

    
    private int damage = 1;
    private int shots = 1;
    private int shotspeed = 1;
    private int engineBonus = 1;

    private AudioStreamPlayer2D player;

   
    private Resource crosshair = ResourceLoader.Load("res://Sprites/Crosshairs-PublicDomain/cross_whole.png");
    private Node2D Target;
    private Label label;
    private Timer timer;

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
        timer = GetNode<Timer>("Timer");
        timer.WaitTime = determineShotSpeed();
        Target = GetNode<Node2D>("Target");
        player = GetNode<AudioStreamPlayer2D>("Effect");
        label = GetNode<Label>("Turret/Label");

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
        if(cooldown == true)
        {
            label.Text = Math.Round(timer.TimeLeft,2).ToString();
        }
        else
        {
            label.Text = "";
        }
        if(controled == true)
        {
           
            Vector2 mousepos = GetLocalMousePosition();
            //GD.Print(turret.Position.X - mousepos.X);
            //If the X of the turret posistion - the x of the mouse is less than 0
            //aka if the X of the mouse is above the turret then rotate
            //This limits rotation to 180 degrees
            //Note X is up down here i think fun times

            if((turret.Position.X - mousepos.X) < 0)
            {
                Vector2 globalMousePos = GetGlobalMousePosition();
                //If the hoist move turret normaly 
                if (IsMultiplayerAuthority()) 
                {
                    
                    turret.LookAt(globalMousePos);

                    Rpc(nameof(moveTarget));


                }
                //Else rpc to host to get them to move the turret
                else
                {
                    Rpc(nameof(moveTurret), globalMousePos);
                    Rpc(nameof(moveTarget));
                }
                //Therefore Moving
                if (spaceship.getRotationDir() != 0)
                {
                    if (turret.Rotation < -0.7 || turret.Rotation > 0.7)
                    {
                        Input.SetCustomMouseCursor(null);
                        return;
                    }
                }
                Input.SetCustomMouseCursor(crosshair,Input.CursorShape.Arrow,new Vector2(16,16));
                //Since the turret is in a valid posistion we now check for shooting
                if (Input.IsActionPressed("LeftClick"))
                {
                    //Create a new bullet at the posistion of this and make it start moving
                   if(cooldown == false)
                    {

                        timer.Start(determineShotSpeed());
                        cooldown = true;
                        waitforShots();

                    }
                   


                }
            }
            else
            {
                Input.SetCustomMouseCursor(null);
            }
            
        }
        
	}

    private async void waitforShots()
    {
        await Task.Delay(300/shots);
        for (int i = 0; i < shots; i = i + 1)
        {
            Rpc(nameof(Fire), GetGlobalMousePosition());
            await Task.Delay(300);
        }
        
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    private void Fire(Vector2 mousepos)
    {
        timer.Start(determineShotSpeed());
        cooldown = true;
        player.Play();
        Bullet newbullet;
        GetNode("/root/World").AddChild(newbullet = (Bullet)bullet.Instantiate());

        newbullet.startmoving(Target.GlobalPosition, mousepos,turret.GlobalRotation, spaceship.getRotationDir(),damage);
        EffectManager effect = GetNode<EffectManager>("/root/EffectManager");
        effect.smokeEmmision(Target.GlobalPosition, turret.GlobalRotation);


    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void moveTurret(Vector2 mousepos)
    {
        turret.LookAt(mousepos);
        
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void moveTarget()
    {
        int radius = 45;
        float xpos = (float)(turret.Position.X + radius * Math.Cos(turret.Rotation));
        float ypos = (float)(turret.Position.Y + radius * Math.Sin(turret.Rotation));
        Target.Position = new Vector2(xpos, ypos);
        //GD.Print(spaceship.getRotationSpeed());
    }

    //Toggle the current control
    public void _on_weapons_control_toggle_weapon_control(int weaponnum)
	{
        switch (weaponnum)
        {
            case 1:
                if(this.Name != "Weapon1")
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
                if(this.Name != "Weapon4")
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

    private void _on_timer_timeout()
    {
        cooldown = false;
        

    }

    public override bool getifControlled()
    {
        GD.Print(othercontrolled);
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
