using Godot;
using System;

public partial class DeliveryDestination : Node2D
{
    [Signal]
    public delegate void FinishTaskEventHandler(int num);
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    //Check for ship, if ship send signal and quefree
    private void _on_area_2d_body_entered(Node2D body)
    {
        //Only the host check
        if (IsMultiplayerAuthority())
        {
            GD.Print(body.GetGroups());
            if (body.IsInGroup("SpaceShip"))
            {
                
                Rpc(nameof(finishTask));
            }

        }

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void finishTask()
    {
        //TODO rpc
        EmitSignal(SignalName.FinishTask, 1);
        //Yuck
        QueueFree();
    }
}
