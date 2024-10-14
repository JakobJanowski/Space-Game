using Godot;
using System;

public partial class LostCrate : Node2D
{

    [Signal]
    public delegate void taskProgressEventHandler(int num, int progress);
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	//Check for ship, if ship send signal and quefree
	private void _on_area_2d_2_body_entered(Node2D body)
	{
		//Only the host check
		if (IsMultiplayerAuthority())
		{
			GD.Print(body.GetGroups());
			if (body.IsInGroup("SpaceShip"))
			{
				Rpc(nameof(progressTask));
            }
			
		}
        
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void progressTask()
	{
        //TODO rpc
        EmitSignal(SignalName.taskProgress, 1, 1);
        //Yuck
        QueueFree();
    }

	



}
