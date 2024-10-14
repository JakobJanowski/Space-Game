using Godot;
using System;

public partial class Spaceship : Entity
{
	//Control the ships speed
	[Export]
	private int _speed = 300;
	//Is this character being controled, to avoid more than one person controling the same thing
	private bool controled = false;
	//This characters camera
	private Camera2D camera2D;

	private int health = 100;

	[Export]
	public int Speed { get; set; } = 400;

	[Export]
	public float RotationSpeed { get; set; } = 1.5f;

	private float _rotationDirection;
	private float rotationspeed = 0;

	private RichTextLabel shiphealth;
    private EngineUI engineUI;
	private AnimatedSprite2D animatedSprite;

	private bool StartedMoving = false;

	[Signal]
	public delegate void GameOverEventHandler();

    private AudioStreamPlayer2D engine;
	private bool playEngineSound = false;

    public override void _Ready()
	{
		base._Ready();
        
        shiphealth = GetNode<RichTextLabel>("/root/World/Interface/Interface/Panel/RichTextLabel");
        camera2D = GetNode<Camera2D>("Camera2D");
        engineUI = GetNode<EngineUI>("/root/World/Shapeship/EngineUI");
        engineUI.ShieldUp += EngineUI_ShieldUp;

        PlayerShield shield1 = GetNode<PlayerShield>("Shield1");
        shield1.Hide();
		shield1.toggleExists();
        PlayerShield shield2 = GetNode<PlayerShield>("Shield2");
        shield2.Hide();
        shield2.toggleExists();
        PlayerShield shield3 = GetNode<PlayerShield>("Shield3");
        shield3.Hide();
        shield3.toggleExists();
        PlayerShield shield4 = GetNode<PlayerShield>("Shield4");
        shield4.Hide();
        shield4.toggleExists();

        animatedSprite = GetNode<AnimatedSprite2D>("Sprite2D");
        engine = GetNode<AudioStreamPlayer2D>("Engine");
		engine.StreamPaused = true;
    }
	//Work out ships velocity
	public void GetInput()
	{
		_rotationDirection = Input.GetAxis("Left", "Right");
		Velocity = Transform.X * Input.GetAxis("Down", "Up") * Speed;

	}
	
	public override void _PhysicsProcess(double delta)
	{
		

        if (controled == true)
		{
			
			GetInput();
			Rotation += _rotationDirection * RotationSpeed * (float)delta;
            rotationspeed = _rotationDirection * RotationSpeed * (float)delta;
            //Note may break in actualy multiplayer
            MoveAndSlide();

            Rpc("moveShip", Velocity, Rotation,GlobalPosition);
            
			

			
		}
		
		
		//Only play spin animation for spining on the spot
		if(_rotationDirection == 1 && StartedMoving == false)
		{
            playEngineSound = true;
            animatedSprite.Stop();
            animatedSprite.Play("Spin1");
            Rpc(nameof(rpcPlayAnimation), "Spin1");
        }
		else if (_rotationDirection == -1 && StartedMoving == false)
		{
            playEngineSound = true;
            animatedSprite.Stop();
            animatedSprite.Play("Spin2");
            Rpc(nameof(rpcPlayAnimation), "Spin2");
        }
		else
		{
            playEngineSound = false;
        }
	

		//Animation Time
		if(Velocity.X != 0 || Velocity.Y != 0)
		{
			playEngineSound = true;
			if(StartedMoving == false)
			{
				animatedSprite.Stop();
				StartedMoving = true;
				animatedSprite.Play("Stop-Move");
				Rpc(nameof(rpcPlayAnimation), "Stop-Move");

			}
			else
			{
				if(animatedSprite.IsPlaying() == false)
				{
					animatedSprite.Play("Moving");
                    Rpc(nameof(rpcPlayAnimation), "Moving");
                }
			}
		}
		else
		{
			
            if (StartedMoving == true)
            {
                animatedSprite.Stop();
                StartedMoving = false;
                animatedSprite.Play("Move-Stop");
                Rpc(nameof(rpcPlayAnimation), "Move-Stop");
            }
            else
            {
                if (animatedSprite.IsPlaying() == false)
                {
                    animatedSprite.Play("default");
                    Rpc(nameof(rpcPlayAnimation), "default");
                    playEngineSound = false;
                }
            }
        }
		if(playEngineSound == true)
		{
			engine.StreamPaused = false;
		}
		else
		{
			engine.StreamPaused = true;
		}

    }
	//Transfer mode reliable ensures this is called through tcp
	//TODO ensure a desyc CANNOT occur at all costs
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void moveShip(Vector2 v,float r,Vector2 actualPos)
	{
		Velocity = v;
		Rotation = r;
		
		GlobalPosition = actualPos;
		
	}
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = false)]
    private void rpcPlayAnimation(string name)
	{
		//Stop what your doing and play something new
		//animatedSprite.Stop();
		animatedSprite.Play(name);
	}

	//Switch control to and away from the ship
	private void _on_navigation_toggle_player_control()
	{
		if(controled == false)
		{
			controled = true;
			camera2D.MakeCurrent();
		}
		else
		{
			controled = false;
		}
	}

	public float getRotationDir()
	{
		return _rotationDirection;
	}
	
	public override void takeDamage(int damage)
    {
		if(IsMultiplayerAuthority())
		{
            health = health - damage;
            shiphealth.Text = "Ship Integrity: " + health + "%";
			Rpc(nameof(RPCtakeDamage),damage);
			if(health <= 0)
			{
				//Do cool shit
				EmitSignal(SignalName.GameOver);
			}
        }
		
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RPCtakeDamage(int damage)
    {
        health = health - damage;
        shiphealth.Text = "Ship Integrity: " + health + "%";
    }

    public int getHealth()
	{
		return health;
	}

    public void repairShip(int amount)
    {
        health = health + amount;
        shiphealth.Text = "Ship Integrity: " + health + "%";
    }

    private void EngineUI_ShieldUp(int shield)
    {
        PlayerShield shield1 = GetNode<PlayerShield>("Shield1");
		shield1.toggleActive(false);
		shield1.toggleExists();
        PlayerShield shield2 = GetNode<PlayerShield>("Shield2");
        shield2.toggleActive(false);
        shield2.toggleExists();
        PlayerShield shield3 = GetNode<PlayerShield>("Shield3");
        shield3.toggleActive(false);
        shield3.toggleExists();
        PlayerShield shield4 = GetNode<PlayerShield>("Shield4");
        shield4.toggleActive(false);
        shield4.toggleExists();
		switch(shield)
		{
			case 1:
				if (shield1.getIfWorking())
				{
                    shield1.toggleActive(true);
                    shield1.toggleExists();
                }
				break;
			case 2:
                if (shield2.getIfWorking())
				{
                    shield2.toggleActive(true);
                    shield2.toggleExists();
                }           
                break;
			case 3:
                if (shield3.getIfWorking())
				{
                    shield3.toggleActive(true);
                    shield3.toggleExists();
                }
                   
                break;
			case 4:
				if (shield4.getIfWorking())
				{
                    shield4.toggleActive(true);
                    shield4.toggleExists();
                }
               
                break;
		}
    }
	

	
}
