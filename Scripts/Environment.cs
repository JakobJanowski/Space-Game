using Godot;
using System;
using System.Collections.Generic;

public partial class Environment : Node2D
{

	
	private Spaceship spaceship;
	private List<Node2D> segments = new List<Node2D>();
	private const int disttoLoad = 5500;

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
        spaceship = GetNode<Spaceship>("/root/World/Shapeship");
		foreach(var child in GetChildren())
		{
			if (child.IsInGroup("Segment"))
			{
                segments.Add((Node2D)child);
            }
			
		}
		
    }

    

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		//Loop through each segment and if the spaceship is close enough show the segment
		foreach(var segment in segments)
		{
			//GD.Print(segment.Name + " " + spaceship.GlobalPosition.DistanceTo(segment.GlobalPosition));
			if(spaceship.GlobalPosition.DistanceTo(segment.GlobalPosition) < disttoLoad)
			{
				if(!segment.Visible) 
				{
                    segment.Show();
                }
			}
			else
			{
                if (segment.Visible)
                {
                    segment.Hide();
                }
            }
		}
	}

	
}
