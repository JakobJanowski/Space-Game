using Godot;
using System;

public partial class WeaponBase : Weapon
{
    //Is this character being controled, to avoid more than one person controling the same thing
    private bool controled = false;

    private bool othercontrolled = false;
    //This characters camera
    private Camera2D camera2D;
    //The moveable part of the turret
    private CharacterBody2D turret;

    private bool cooldown = false;

    private int damage = 1;
    private int shots = 1;
    private int shotspeed = 1;
    private int engineBonus = 1;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //The order shoudnt change
        camera2D = GetNode<Camera2D>("TurretCamera");
        turret = GetNode<CharacterBody2D>("Turret");
        var toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl");
        toggleWeapons.ToggleWeaponControl += _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl2");
        toggleWeapons.ToggleWeaponControl += _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl3");
        toggleWeapons.ToggleWeaponControl += _on_weapons_control_toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl4");
        toggleWeapons.ToggleWeaponControl += _on_weapons_control_toggle_weapon_control;

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        

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
            //Send a signal so everyone knows this is being controlled by someone
            Rpc(nameof(updateOtherControlled));
        }
        else
        {
            controled = false;
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
        return;
    }

    public override bool getifControlled()
    {
        return othercontrolled;
    }

    public override void setDamage(int Damage)
    {
        damage = damage + Damage;
    }

    public override void setshots(int Shots)
    {
        shots = shots + Shots;
    }

    public override void setshotspeed(int Shotspeed)
    {
        shotspeed = shotspeed + Shotspeed;
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

    public override void setEngineBonus(int EngineBonus)
    {
        engineBonus = EngineBonus;
    }

    public override int getEngineBonus()
    {
        return engineBonus;
    }
}
