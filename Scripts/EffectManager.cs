using Godot;
using System;
using System.Diagnostics.Metrics;

public partial class EffectManager : Node2D
{

	[Export]
	PackedScene RockExplosion;

    [Export]
    PackedScene SmokeEmmision;

    [Export]
    PackedScene HitExplosion;

    [Export]
    PackedScene DeathExplosion;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	//Spawn a explosion at given corodinate
	public void RockExplode(Vector2 Globalpos,Vector2 scale)
	{
		float scalebonus = 1;
		switch (scale.X)
		{
			case 50:
				scalebonus = 30;
				break;
			case 25:
				scalebonus = 15;
				break;
			case 10:
				scalebonus = 7;
				break;
			case 5:
				scalebonus = 2;
				break;
			case 1:
				scalebonus = -0.5f;
				break;
		}
	
		RockExplosion rockExplosion;
		GetNode("/root/World").AddChild(rockExplosion = (RockExplosion)RockExplosion.Instantiate());
		rockExplosion.GlobalPosition = Globalpos;
		//Alter size as needed
		ParticleProcessMaterial rockmat = (ParticleProcessMaterial)rockExplosion.ProcessMaterial;

        rockmat.ScaleMax = 1 + scalebonus;
        for (int i = 0;i < rockExplosion.GetChildren().Count; i= i + 1)
		{
			Node child = rockExplosion.GetChild(i);
			if(child.Name == "EX1" || child.Name == "EX2")
			{
				GpuParticles2D ch = (GpuParticles2D)child;
                ParticleProcessMaterial childmat = (ParticleProcessMaterial)ch.ProcessMaterial;
				
                childmat.ScaleMax = 1 + scalebonus;

            }
		}
		rockExplosion.Scale = scale;
        
		//rockExplosion.Scale = scale;
        rockExplosion.ZIndex = rockExplosion.ZIndex + 1;
	}

	public void smokeEmmision(Vector2 globalPos,float rot)
	{
        RockExplosion Smokemission;
        GetNode("/root/World").AddChild(Smokemission = (RockExplosion)SmokeEmmision.Instantiate());
        Smokemission.GlobalPosition = globalPos;
		Smokemission.GlobalRotation = rot;
    }

    public void hitExplosion(Vector2 globalPos)
    {
        RockExplosion Hitemmision;
        GetNode("/root/World").AddChild(Hitemmision = (RockExplosion)HitExplosion.Instantiate());
        Hitemmision.GlobalPosition = globalPos;
		Hitemmision.Lifetime = 0.75f;
        
    }

	public void deathExplosion(Vector2 globalPos)
	{
        RockExplosion deathexplode;
        GetNode("/root/World").AddChild(deathexplode = (RockExplosion)DeathExplosion.Instantiate());
        deathexplode.GlobalPosition = globalPos;
        
    }

}
