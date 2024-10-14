using Godot;
using System;

public partial class PirateSpawner : Node2D
{
    [Export]
    PackedScene Ship;

	private Random random;
	private Spaceship spaceship;
    private PlayerManager playermanager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        playermanager = GetNode<PlayerManager>("/root/PlayerManager");
		AddChild(Ship.Instantiate());
		random = new Random();
		spaceship = GetNode<Spaceship>("/root/World/Shapeship");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (IsMultiplayerAuthority()) { 
            //IF spaceship is far way enough respawn the pirate ship at a very very low chance
            if (GetChildCount() == 0)
            {
                if (GlobalPosition.DistanceTo(spaceship.GlobalPosition) > 4000)
                {
                    int i = random.Next(1, 60001);
                    if (i >= 60000)
                    {
                        GD.Print("Spawned");
                        AddChild(Ship.Instantiate());
                    }
                }
            }
        }
		
		
	}
}
