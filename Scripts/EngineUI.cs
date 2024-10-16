using Godot;
using System;
using System.Collections.Generic;

public partial class EngineUI : Node2D
{

    [Signal]
    public delegate void TogglePlayerControlEventHandler();

    [Signal]
    public delegate void ShieldUpEventHandler(int shield);

    [Signal]
    public delegate void EngineValueChangeEventHandler(int weapon,int value);


    private List<String> playersInRange;
    private bool playerInRange = false;
    private bool isUsingEngine = false;
    private bool engineisUse = false;
    private PlayerManager playerManager;
    private Control infopopup;
    private AudioStreamPlayer Click;
    bool pressing = false;
    bool mouseInRange = false;

    int energy = 100;
    int sheild = 0;
    int w1value = 1;
    int w2value = 1;
    int w3value = 1;
    int w4value = 1;

    private CanvasLayer menu;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		
        playersInRange = new List<String>();
        playerManager = GetNode<PlayerManager>("/root/PlayerManager");
        infopopup = GetNode<Control>("/root/World/Interface/InfoPopUp/Container");
        menu = GetNode<CanvasLayer>("CanvasLayer");
        Click = GetNode<AudioStreamPlayer>("Click");
    }


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		

        if (playerInRange == true)
        {
            //Issue is this returns 1 for everyone
            if (playersInRange.Contains(playerManager.getYourPlayer()))
            {
                //If the interact key is pressed while a player is within range toggle control of the ship
                if (Input.IsActionJustReleased("Interact") || (Input.IsActionJustPressed("OpenMainMenu") && isUsingEngine == true))
                {
                    //Stop if menu is open
                    if (playerManager.getMenuStatus() == true)
                    {
                        return;
                    }
                    if (!engineisUse || isUsingEngine == true)
                    {
                        EmitSignal(SignalName.TogglePlayerControl);
                        if (isUsingEngine == false)
                        {
                            Label Desciption = (Label)infopopup.GetChild(0);
                            Desciption.Text = "Engines";
                            Label PressE = (Label)infopopup.GetChild(1);
                            PressE.Text = "Press E to Exit";
                            isUsingEngine = true;
                            menu.Show();
                        }
                        else
                        {
                            Label Desciption = (Label)infopopup.GetChild(0);
                            Desciption.Text = "Engines";
                            Label PressE = (Label)infopopup.GetChild(1);
                            PressE.Text = "Press E to Use";
                            isUsingEngine = false;
                            menu.Hide();
                        }
                        Rpc(nameof(toggleShipOwner));
                    }


                }
            }


        }
        energy = 100;
        Label powerlabel = GetNode<Label>("CanvasLayer/MarginContainer/Panel/HSplitContainer/VBoxContainer/PowerLabel");
        if(sheild != 0)
        {
            energy = energy - 25;
        }
        int sum = w1value+w2value+w3value+w4value-4;
        sum = sum * 10;
        energy = energy - sum;
        powerlabel.Text = "Power Available: " + energy + "%";
    }

    private void _on_weapon_1_slider_value_changed(float value)
    {
        if (isUsingEngine)
        {
            Click.Play();
            Rpc(nameof(rpcWeapon1Slider), value);
        }
        

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void rpcWeapon1Slider(float value)
    {
        Slider slider = GetNode<Slider>("CanvasLayer/MarginContainer/Panel/HSplitContainer/VBoxContainer/Weapon1Slider");
        if (energy <= 5)
        {
            if (value > w1value)
            {
                
                slider.Value = w1value;
            }
            else
            {
                w1value = (int)value;
                EmitSignal(SignalName.EngineValueChange, 1, w1value);
            }

        }
        else
        {
            w1value = (int)value;
            EmitSignal(SignalName.EngineValueChange, 1, w1value);
        }
        slider.Value = w1value;
    }

    private void _on_weapon_2_slider_value_changed(float value)
    {
        if (isUsingEngine)
        {
            Click.Play();
            Rpc(nameof(rpcWeapon2Slider), value);
        }
        
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void rpcWeapon2Slider(float value)
    {
        Slider slider = GetNode<Slider>("CanvasLayer/MarginContainer/Panel/HSplitContainer/VBoxContainer/Weapon2Slider");
        if (energy <= 5)
        {
            if (value > w2value)
            {
                
                slider.Value = w2value;
            }
            else
            {
                w2value = (int)value;
                EmitSignal(SignalName.EngineValueChange, 2, w2value);
            }
        }
        else
        {
            w2value = (int)value;
            EmitSignal(SignalName.EngineValueChange, 2, w2value);
        }
        slider.Value = w2value;
    }

    private void _on_weapon_3_slider_value_changed(float value)
    {
        if (isUsingEngine)
        {
            Click.Play();
            Rpc(nameof(rpcWeapon3Slider), value);
        }
        
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void rpcWeapon3Slider(float value)
    {
        Slider slider = GetNode<Slider>("CanvasLayer/MarginContainer/Panel/HSplitContainer/VBoxContainer/Weapon3Slider");
        if (energy <= 5)
        {
            if (value > w3value)
            {
                
                slider.Value = w3value;
            }
            else
            {
                w3value = (int)value;
                EmitSignal(SignalName.EngineValueChange, 3, w3value);
            }
        }
        else
        {
            w3value = (int)value;
            EmitSignal(SignalName.EngineValueChange, 3, w3value);
        }
        slider.Value = w3value;
    }

    private void _on_weapon_4_slider_value_changed(float value)
    {
        if(isUsingEngine)
        {
            Click.Play();
            Rpc(nameof(rpcWeapon4Slider), value);
        }
        
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void rpcWeapon4Slider(float value)
    {
        Slider slider = GetNode<Slider>("CanvasLayer/MarginContainer/Panel/HSplitContainer/VBoxContainer/Weapon4Slider");
        if (energy <= 5)
        {
            if (value > w4value)
            {
                slider = GetNode<Slider>("CanvasLayer/MarginContainer/Panel/HSplitContainer/VBoxContainer/Weapon4Slider");
                slider.Value = w4value;
            }
            else
            {
                w4value = (int)value;
                EmitSignal(SignalName.EngineValueChange, 4, w4value);
            }
        }
        else
        {
            w4value = (int)value;
            EmitSignal(SignalName.EngineValueChange, 4, w4value);
        }
        
        slider.Value = w4value;
    }


    private void _on_center_button_pressed()
    {
        
        EmitSignal(SignalName.ShieldUp, 0);
        sheild = 0;
        Rpc(nameof(updateShield), 0);
    }

    private void _on_top_button_pressed()
    {
        if(energy > 25)
        {
            EmitSignal(SignalName.ShieldUp, 1);
            sheild = 1;
            Rpc(nameof(updateShield), 1);
        }
        
    }

    private void _on_down_button_pressed()
    {
        if (energy > 25)
        {
            EmitSignal(SignalName.ShieldUp, 2);
            sheild = 2;
            Rpc(nameof(updateShield), 2);
        }
        
    }

    private void _on_left_button_pressed()
    {
        if (energy > 25)
        {
            EmitSignal(SignalName.ShieldUp, 3);
            sheild = 3;
            Rpc(nameof(updateShield), 3);
        }
        
    }

    private void _on_right_button_pressed()
    {
        if (energy > 25)
        {
            EmitSignal(SignalName.ShieldUp, 4);
            sheild = 4;
            Rpc(nameof(updateShield), 4);
        }
       
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void updateShield(int shield)
    {
        switch (shield) 
        {
            case 0:
                EmitSignal(SignalName.ShieldUp, 0);
                sheild = 0;
                break;
            case 1:
                EmitSignal(SignalName.ShieldUp, 1);
                sheild = 1;
                break;
            case 2:
                EmitSignal(SignalName.ShieldUp, 2);
                sheild = 2;
                break;
            case 3:
                EmitSignal(SignalName.ShieldUp, 3);
                sheild = 3;
                break;
            case 4:
                EmitSignal(SignalName.ShieldUp, 4);
                sheild = 4;
                break;

        }
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void toggleShipOwner()
    {
        if (engineisUse == false)
        {
            engineisUse = true;
        }
        else
        {
            engineisUse = false;
        }
    }

    



    private void _on_area_2d_body_entered(PhysicsBody2D body)
	{
        playersInRange.Add(body.Name);

        if (playersInRange.Contains(playerManager.getYourPlayer()))
        {
            infopopup.Show();
            Label Desciption = (Label)infopopup.GetChild(0);
            Desciption.Text = "Engine";
            Label PressE = (Label)infopopup.GetChild(1);
            PressE.Text = "Press E to Use";

        }

        playerInRange = true;
    }

    private void _on_area_2d_body_exited(PhysicsBody2D body)
	{
        if (playerManager.getYourPlayer() == body.Name)
        {
            infopopup.Hide();

        }
        playersInRange.Remove(body.Name);
        //Only if no players are in range toggle boolean
        if (playersInRange.Count == 0)
        {
            playerInRange = false;
        }
    }
}
