using Godot;
using System;
using System.Collections.Generic;

public partial class ToggleWeapons : Area2D
{

    [Signal]
    public delegate void ToggleWeaponControlEventHandler(int weaponid);


    private bool playerInRange = false;
  
    [Export]
    private PackedScene UiText;

    private List<String> playersInRange;

    private PlayerManager playerManager;

    private bool iscontrollingWeapons = false;
    //To tell if someone else is using the ship
    private bool weaponsInUse = false;
    // Called when the node enters the scene tree for the first time.

    [Export]
    private int count = 1;

    private Control infopopup;
    public override void _Ready()
	{
        playersInRange = new List<String>();
        playerManager = GetNode<PlayerManager>("/root/PlayerManager");
        infopopup = GetNode<Control>("/root/World/Interface/InfoPopUp/Container");

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (playerInRange == true)
        {
            
            if (playersInRange.Contains(playerManager.getYourPlayer())) 
            {
                //If the interact key is pressed while a player is within range toggle control of the ship
                if (Input.IsActionJustReleased("Interact") || (Input.IsActionJustPressed("OpenMainMenu") && iscontrollingWeapons == true))
                {
                    //Stop if menu is open
                    if(playerManager.getMenuStatus() == true)
                    {
                        return;
                    }
                    if (!weaponsInUse || iscontrollingWeapons == true) 
                    {
                        //Todo add for alt weapons
                        EmitSignal(SignalName.ToggleWeaponControl, count);
                        if (iscontrollingWeapons == false)
                        {
                            Label Desciption = (Label)infopopup.GetChild(0);
                            Desciption.Text = "Weapons";
                            Label PressE = (Label)infopopup.GetChild(1);
                            PressE.Text = "Press E to Exit";
                            iscontrollingWeapons = true;
                        }
                        else
                        {
                            Label Desciption = (Label)infopopup.GetChild(0);
                            Desciption.Text = "Weapons";
                            Label PressE = (Label)infopopup.GetChild(1);
                            PressE.Text = "Press E to Use";
                            iscontrollingWeapons = false;
                            Input.SetCustomMouseCursor(null);
                        }
                        Rpc(nameof(toggleWeaponOwner));
                    }
                    
                }
            }
            

        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void toggleWeaponOwner()
    {
        if (weaponsInUse == false)
        {

            weaponsInUse = true;
        }
        else
        {
            weaponsInUse = false;
        }
    }

    //Toggle whether a player is in range of this 
    private void _on_body_entered(PhysicsBody2D body)
    {
        playersInRange.Add(body.Name);

        if (playersInRange.Contains(playerManager.getYourPlayer()))
        {
            infopopup.Show();
            Label Desciption = (Label)infopopup.GetChild(0);
            Desciption.Text = "Weapons";
            Label PressE = (Label)infopopup.GetChild(1);
            PressE.Text = "Press E to Use";
            
           

        }
        
        playerInRange = true;
        

    }

    private void _on_body_exited(PhysicsBody2D body)
    {
        if (playerManager.getYourPlayer() == body.Name)
        {
            infopopup.Hide();

        }
        //var popup = GetTree().GetNodesInGroup("PopupText")[0];
        //Then it exists
        //if (popup != null)
        // {
        //
        // }
        playersInRange.Remove(body.Name);
        //Only if no players are in range toggle boolean
        if(playersInRange.Count == 0) 
        {
            playerInRange = false;
        }
        
        
    }
}
