using Godot;
using System;

public partial class RockManager : Node
{
    int counter = 0;

    [Export]
    private PackedScene Rock;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void reset()
    {
        counter = 0;
    }

	public void createRock(Vector2 Pos,int scale)
	{
        
        if (scale < 5)
		{
           
            SpaceRock newRock;
            GetNode("/root/World").CallDeferred("add_child", newRock = (SpaceRock)Rock.Instantiate());
            newRock.GlobalPosition = Pos;
            newRock.Name = counter.ToString();
            counter = counter + 1;
            switch (scale)
            {
                case 1:
                    newRock.Scale = new Vector2(25, 25);
                    break;
                case 2:
                    newRock.Scale = new Vector2(10, 10);
                    break;
                case 3:
                    newRock.Scale = new Vector2(5, 5);
                    break;
                case 4:
                    newRock.Scale = new Vector2(1, 1);
                    break;
            }
        }
        
    }
}
