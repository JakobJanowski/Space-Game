using Godot;
using System;

public partial class InnerShield : StaticEntity
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void takeDamage(int damage)
    {
		if(IsMultiplayerAuthority())
		{
			Rpc(nameof(rpctakeDamage), damage);
		}
		
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpctakeDamage(int damage)
	{
        Spaceship ship = (Spaceship)GetParent();
        ship.takeDamage(damage);

    }
}
