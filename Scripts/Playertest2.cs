using Godot;
using System;
using System.Xml.Linq;

public partial class Playertest2 : CharacterBody2D
{
	//Control the players speed
	[Export]
	private int _speed = 300;
	//Is this character being controled, to avoid more than one person controling the same thing
	private bool controled = true;
	//This characters camera
	private Camera2D camera2D;

	private ToggleSpaceship toggleSpaceship;
	private ToggleWeapons toggleWeapons;
	private EngineUI engineUI;
	private PlayerManager playerManager;
	private AnimatedSprite2D _animatedSprite;
	
	private bool inMainMenu = false;
	private bool inTaskMenu = false;
	private Control mainmenu;
	private Control Taskmenu;

	private string PlayerName = "";
	public override void _EnterTree()
	{
        
        string name = Name.ToString();
        
        SetMultiplayerAuthority(name.ToInt());
        GetNode<Label>("Name").Text = name;
        
		
		//GD.Print("Player name:" + name);
		
	}

	public void setPlayerName(string name)
	{

		PlayerName = name;
        GetNode<Label>("Name").Text = PlayerName;
    }

	public override void _Ready()
	{
		base._Ready();
		_animatedSprite = GetNode<AnimatedSprite2D>("Sprite");
       
        if (!IsMultiplayerAuthority())
		{
			return;
		}

		camera2D = GetNode<Camera2D>("Camera2D");
		camera2D.MakeCurrent();
		toggleSpaceship = GetNode<ToggleSpaceship>("/root/World/Shapeship/Navigation");
		toggleSpaceship.TogglePlayerControl += () => toggle_player_control();

		toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl");
		toggleWeapons.ToggleWeaponControl +=  toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl2");
        toggleWeapons.ToggleWeaponControl += toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl3");
        toggleWeapons.ToggleWeaponControl += toggle_weapon_control;

        toggleWeapons = GetNode<ToggleWeapons>("/root/World/Shapeship/WeaponsControl4");
        toggleWeapons.ToggleWeaponControl += toggle_weapon_control;

		engineUI = GetNode<EngineUI>("/root/World/Shapeship/EngineUI");
		engineUI.TogglePlayerControl += () => toggle_player_control();

        playerManager = GetNode<PlayerManager>("/root/PlayerManager");
        playerManager.setYourPlayer(this.Name);


        mainmenu = GetNode<Control>("/root/World/Interface/GameMenu");
		Taskmenu = GetNode<Control>("/root/World/Interface/TaskMenu");
      


	}
	//Work out players velocity
	public void GetInput()
	{
		Vector2 inputDir = Input.GetVector("Left", "Right", "Up", "Down");
		//To open a menu no other menu must be open and you must be controlling your own character
		if (Input.IsActionJustPressed("OpenMainMenu") && inTaskMenu == false)
		{
			if(inMainMenu == false)
			{
				mainmenu.Show();
				inMainMenu = true;
				playerManager.toggleMenuStatus(inMainMenu);
			}
			else
			{
				mainmenu.Hide();
				inMainMenu= false;
                playerManager.toggleMenuStatus(inMainMenu);
            }
		}

		if (Input.IsActionJustPressed("OpenGameMenu") && inMainMenu == false && controled == true) 
		{
			if (inTaskMenu == false)
			{
				Taskmenu.Show();
				inTaskMenu = true;
                playerManager.toggleMenuStatus(inTaskMenu);
            }
			else
			{
				Taskmenu.Hide();
				inTaskMenu = false;
                playerManager.toggleMenuStatus(inTaskMenu);
            }
		}

		if(Input.IsActionJustPressed("OpenMainMenu") && inMainMenu == false && controled == true)
		{
            if (inTaskMenu == true)
			{
                Taskmenu.Hide();
                inTaskMenu = false;
                playerManager.toggleMenuStatus(inTaskMenu);
            }

        }

		
		if (Input.IsActionPressed("Left"))
		{
			_animatedSprite.Play("Walk Left");
		}
		else if (Input.IsActionPressed("Right"))
		{
			_animatedSprite.Play("Walk Right");
		}
		else if (Input.IsActionPressed("Up")) 
		{
			_animatedSprite.Play("Walk Up");
		}
		else if (Input.IsActionPressed("Down"))
		{
			_animatedSprite.Play("Walk Down");
		}
		else
		{
			_animatedSprite.Stop();
		}
		Velocity = inputDir * _speed;
		Velocity = Velocity.Rotated(GlobalRotation);
		//If in main menu then donot display menu
		if (inMainMenu== true || inTaskMenu == true)
		{
			Velocity = new Vector2(0,0);
		}
	  
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsMultiplayerAuthority())
		{
			return;
		}
		if (controled == true)
		{
			GetInput();
			MoveAndSlide();
			
		}
	}
	//Switch control to or away from this character
	private void toggle_player_control()
	{
		if (!IsMultiplayerAuthority())
		{
			return;
		}
		if(inMainMenu == true || inTaskMenu == true)
		{
			return;
		}
		if (controled == false)
		{
			controled = true;
			camera2D.MakeCurrent();
		}
		else
		{
			controled = false;
		}
	}

	private void toggle_weapon_control(int weaponid)
	{
		if (!IsMultiplayerAuthority())
		{
			return;
		}
        if (inMainMenu == true || inTaskMenu == true)
        {
            return;
        }
        if (controled == false)
		{
			controled = true;
			camera2D.MakeCurrent();
		}
		else
		{
			controled = false;
		}
	}



}
