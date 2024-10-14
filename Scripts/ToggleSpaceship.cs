using Godot;
using System;
using System.Collections.Generic;

public partial class ToggleSpaceship : Area2D

{
	[Signal]
	public delegate void TogglePlayerControlEventHandler();


	private bool playerInRange = false;

	[Export]
	private PackedScene UiText;
    private Control infopopup;

    private List<String> playersInRange;

	private PlayerManager playerManager;
	//To tell if you are using the ship
	private bool iscontrollingShip = false;
	//To tell if someone else is using the ship
	private bool shipInUse = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playersInRange = new List<String>();
		playerManager = GetNode<PlayerManager>("/root/PlayerManager");
        infopopup = GetNode<Control>("/root/World/Interface/InfoPopUp/Container");

    }
  
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(playerInRange == true)
		{
			//Issue is this returns 1 for everyone
			if(playersInRange.Contains(playerManager.getYourPlayer()))
			{
				//If the interact key is pressed while a player is within range toggle control of the ship
				if (Input.IsActionJustReleased("Interact") || (Input.IsActionJustPressed("OpenMainMenu") && iscontrollingShip == true))
				{
                    //Stop if menu is open
                    if (playerManager.getMenuStatus() == true)
                    {
                        return;
                    }
                    if (!shipInUse || iscontrollingShip == true) 
					{
						EmitSignal(SignalName.TogglePlayerControl);
						if (iscontrollingShip == false)
						{
                            Label Desciption = (Label)infopopup.GetChild(0);
                            Desciption.Text = "Navigation";
                            Label PressE = (Label)infopopup.GetChild(1);
                            PressE.Text = "Press E to Exit";
                            iscontrollingShip = true;
						}
						else
						{
                            Label Desciption = (Label)infopopup.GetChild(0);
                            Desciption.Text = "Navigation";
                            Label PressE = (Label)infopopup.GetChild(1);
                            PressE.Text = "Press E to Use";
                            iscontrollingShip = false;
						}
						Rpc(nameof(toggleShipOwner));
					}
				   
					
				}
			}
			
			
		}
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void toggleShipOwner()
	{
		if(shipInUse == false) 
		{
			shipInUse = true;
		}
		else
		{
			shipInUse = false;
		}
	}
	
	private void _on_body_entered(PhysicsBody2D body)
	{
		playersInRange.Add(body.Name);

		if (playersInRange.Contains(playerManager.getYourPlayer()))
		{
			infopopup.Show();
            Label Desciption = (Label)infopopup.GetChild(0);
            Desciption.Text = "Navigation";
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
		playersInRange.Remove(body.Name);
		//Only if no players are in range toggle boolean
		if (playersInRange.Count == 0)
		{
			playerInRange = false;
		}
	}


}

	
