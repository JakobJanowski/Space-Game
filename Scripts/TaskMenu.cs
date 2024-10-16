using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class TaskMenu : Control
{

	private MarginContainer missionContainer;
    private MarginContainer upgradesContainer;
    private MarginContainer mapContainer;
    //Menu count 0 = missions
    //count 1 = upgrades
    //count 2 = map
    int menucount = 0;

  

    private MoneyManager moneyManager;
    private const int swapCost = 100;
    private const int upgradeCost = 50;

    private Dictionary<String, int[]> Upgrades;
    
    //Store weapons, to be overidden with an actual type later
    private Node2D Weapon1;
    private Node2D Weapon2;
    private Node2D Weapon3;
    private Node2D Weapon4;
    
    //The weapon the menu is affecting
    private Node2D ActiveWeapon;
    private int weaponCount = 1;

    private Spaceship shapeship;

    [Signal]
    public delegate void startTaskEventHandler(int num);
    //Initial Tasks
    private ItemList list;
    private EngineUI engineUI;

    private AudioStreamPlayer click;
    private AudioStreamPlayer Boing;

    
    public override void _Ready()
    {
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");
        list = GetNode<ItemList>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskList");
        list.SetItemTooltip(0, "Recover Cargo then deliver it to the Drop off point");
        list.SetItemTooltip(1, "Defeat 20 Hostile Entities");
        list.SetItemTooltip(2, "Go to and destroy the marked Monster Nest");
        click = GetNode<AudioStreamPlayer>("Click");
        Boing = GetNode<AudioStreamPlayer>("Boing");
        //Store data relating to upgrade data in the dictionary
        //The string array will be in the form on [type,strength,shotspeed,numofshots]
        //Type is 0 for base, 1 for railgun,2 for laser,3 for missle
        Upgrades = new Dictionary<string, int[]>
        {
            { "Weapon1", new int[] { 1, 1, 1, 1 } },
            { "Weapon2", new int[] { 0, 1, 1, 1 } },
            { "Weapon3", new int[] { 0, 1, 1, 1 } },
            { "Weapon4", new int[] { 0, 1, 1, 1 } }
        };
        
        missionContainer = GetNode<MarginContainer>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer");
        upgradesContainer = GetNode<MarginContainer>("MarginContainer2/PanelContainer/VSplitContainer/UpgradeContainer");
        mapContainer = GetNode<MarginContainer>("MarginContainer2/PanelContainer/VSplitContainer/MapContainer");
        updateWeaponList();
        ActiveWeapon = Weapon1;
        shapeship = GetNode<Spaceship>("/root/World/Shapeship");

        TaskHandler handler = GetNode<TaskHandler>("/root/World/Environment/TaskHandler");
        handler.taskProgress += Handler_taskProgress;

        engineUI = GetNode<EngineUI>("/root/World/Shapeship/EngineUI");
        engineUI.EngineValueChange += EngineUI_EngineValueChange;
    }

    private void EngineUI_EngineValueChange(int weapon, int value)
    {
        //Set the engine bonus to fire speed
        switch(weapon)
        {
            case 1:
                Weapon weapon1 = (Weapon)Weapon1;
                weapon1.setEngineBonus(value);
                break;
            case 2:
                Weapon weapon2 = (Weapon)Weapon1;
                weapon2.setEngineBonus(value);
                break;
            case 3:
                Weapon weapon3 = (Weapon)Weapon1;
                weapon3.setEngineBonus(value);
                break;
            case 4:
                Weapon weapon4 = (Weapon)Weapon1;
                weapon4.setEngineBonus(value);
                break;

        }
    }

    private void updateWeaponList()
    {
        var weapons = GetTree().GetNodesInGroup("Weapons");
        foreach (var weapon in weapons)
        {
            switch (weapon.Name)
            {
                case "Weapon1":
                    Weapon1 = (Node2D)weapon;
                    break;
                case "Weapon2":
                    Weapon2 = (Node2D)weapon;
                    break;
                case "Weapon3":
                    Weapon3 = (Node2D)weapon;
                    break;
                case "Weapon4":
                    Weapon4 = (Node2D)weapon;
                    break;
            }
        }
        
    }

    public void _on_mission_button_pressed()
	{
        ChangeCurrentui(0, menucount);

    }

	public void _on_upgrades_button_pressed()
	{
        ChangeCurrentui(1, menucount);
    }

	public void _on_map_button_pressed()
	{
        ChangeCurrentui(2, menucount);
    }
    //Swap out the current ui using a int for the desired ui and the old ui
    private void ChangeCurrentui(int newui,int oldui)
    {
        switch (oldui) 
        { 
            case 0:
                missionContainer.Hide();
                break;
            case 1:
                upgradesContainer.Hide();
                break;
            case 2:
                mapContainer.Hide();
                break;
        }
        switch (newui)
        {
            case 0:
                missionContainer.Show();
                menucount = 0;
                break;
            case 1:
                upgradesContainer.Show();
                menucount = 1;
                break;
            case 2:
                mapContainer.Show();
                menucount = 2;
                break;
        }
    }

    private void toggleActiviveWeapon(int newWeapon)
    {
        //Switch activate Weapon to the needed weapon type
      
        switch(newWeapon)
        {
            case 1:
                ActiveWeapon = Weapon1;
                weaponCount = 1;
                break;
            case 2:
                ActiveWeapon = Weapon2;
                weaponCount = 2;
                break;
            case 3:
                ActiveWeapon = Weapon3;
                weaponCount = 3;
                break;
            case 4:
                ActiveWeapon = Weapon4;
                weaponCount = 4;
                break;
        }
        Label label = GetNode<Label>("MarginContainer2/PanelContainer/VSplitContainer/UpgradeContainer/HSplitContainer/Control Container/VBoxContainer/TypeLabel");
        switch(ActiveWeapon.GetType().Name)
        {
            case "LaserTuret":
                label.Text = "Railgun";
                break;
            case "LaserBeamTuret":
                label.Text = "Laser";
                break;
            case "MissleTuret":
                label.Text = "Missle Launcher";
                break;
            default:
                label.Text = "No Weapon";
                break;
        }
        UpdateUpgradeUi();
    }

    //Task Section

    private void _on_item_list_item_activated(int index)
    {
        click.Play();
        if (list.GetItemText(index).ToLower().Trim() == "recover lost cargo - $200 reward" && list.Visible == true)
        {
           
            
            Rpc(nameof(startnumTask), 1);
        }
        else if (list.GetItemText(index).ToLower().Trim() == "defeat hostiles - $300 reward" && list.Visible == true)
        {
            Rpc(nameof(startnumTask), 2);
        }
        else if (list.GetItemText(index).ToLower().Trim() == "exterminate nest - $400 reward" && list.Visible == true)
        {
            Rpc(nameof(startnumTask), 3);
        }
        
    }
    //For every to start the task
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void startnumTask(int num)
    {
        list.Hide();
        GetNode<Panel>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes").Show();
        GetNode<TextureRect>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes/HSplitContainer/TaskIcon").Texture = list.GetItemIcon(num-1);
        switch (num)
        {
            case 1:
                GetNode<RichTextLabel>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes/HSplitContainer/TaskDesLabel").Text =
           "Recover Lost Cargo\n" +
           "\n" +
           "The [REDACTED] company has lost some valuable supplies around here, recover the supplies then head to the drop off point";
                
                break;
            case 2:
                GetNode<RichTextLabel>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes/HSplitContainer/TaskDesLabel").Text =
           "Defeat Hostiles\n" +
           "\n" +
           "Attacks in this area have grown too high. You've been tasked with eliminate a number of hostile entities to reduce their ability to conduct further attacks\n"+
           "\n"+
           "0/20";
                break;
            case 3:
                GetNode<RichTextLabel>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes/HSplitContainer/TaskDesLabel").Text =
           "Destroy Nest\n" +
           "\n" +
           "A Vunerable Monster Nest has been located. Your job is to destroy it once and for all";
                break;
        }
       
        EmitSignal(SignalName.startTask, num);
    }

    private void Handler_taskProgress(int num, int progress)
    {
        Boing.Play();
        switch (num)
        {
            
            case 1:

                switch (progress)
                {
                    //Picked up crate
                    case 1:
                        GetNode<RichTextLabel>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes/HSplitContainer/TaskDesLabel").Text =
                "Recover Lost Cargo\n" +
                "\n" +
                "The supplies have been collected, now just deliver them to the drop off point";
                        break;
                    //Deliverd crate
                    case 2:
                        list.Show();
                        GetNode<Panel>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes").Hide();
                        moneyManager.addMoney(4 * upgradeCost);
                        moneyManager.updateMoneyLabel();
                        break;
                    default : break;
                }



                break;
            case 2:
                list.Show();
                GetNode<Panel>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes").Hide();
                moneyManager.addMoney(6 * upgradeCost);
                moneyManager.updateMoneyLabel();
                break;
            case 3:
                list.Show();
                GetNode<Panel>("MarginContainer2/PanelContainer/VSplitContainer/MissionContainer/TaskDes").Hide();
                moneyManager.addMoney(8 * upgradeCost);
                moneyManager.updateMoneyLabel();
                break;




            default: break;
        }
    }

    //Weapon Section
    private void _on_gun_1_button_pressed()
    {
        toggleActiviveWeapon(1);
    }

    private void _on_gun_2_button_pressed()
    {
        toggleActiviveWeapon(2);
    }

    private void _on_gun_3_button_pressed()
    {
        toggleActiviveWeapon(3);
    }

    private void _on_gun_4_button_pressed()
    {
        toggleActiviveWeapon(4);
    }
  
    //Swaps the current active weapon into a railgun
    private void _on_railgun_button_pressed()
    {
        
        bool check = checkforControl();
        if (check == true)
        {
            return;
        }
        else
        {
            if(moneyManager.getMoney() >= swapCost)
            {
                Rpc(nameof(swapGun), weaponCount, "Railgun");
                toggleActiviveWeapon(weaponCount);
            }
            
        }
      



    }
    //Swaps the current active weapon into a laser gun
    private void _on_laser_button_pressed()
    {
        bool check = checkforControl();
        if (check == true)
        {
            return;
        }
        else
        {
            if(moneyManager.getMoney() >= swapCost)
            {
                Rpc(nameof(swapGun), weaponCount, "LaserBeam");
                toggleActiviveWeapon(weaponCount);
            }
           
        }
    }
    
    private void _on_missle_button_pressed()
    {
        bool check = checkforControl();
        if (check == true)
        {
            return;
        }
        else
        {
            if(moneyManager.getMoney() >= swapCost)
            {
                Rpc(nameof(swapGun), weaponCount, "Missle");
                toggleActiviveWeapon(weaponCount);
            }
            
        }
    }

    private bool checkforControl()
    {
        Weapon checker = (Weapon)ActiveWeapon;
        
        
        if (checker.getifControlled() == true)
        {
                return true;
        }
        return false;
       
    }
   
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void swapGun(int wepCount,String WeaponType)
    {
        //Spend money
        moneyManager.spendMoney(swapCost);
        moneyManager.updateMoneyLabel();

        bool islookingatWeaponChange = false;
        int weaponint = 0;
        //Get the active weapon we need
        Node2D activeWeapon = null;
        switch (wepCount)
        {
            case 1:
                activeWeapon = Weapon1;
                break;
            case 2:
                activeWeapon = Weapon2;
                break;
            case 3:
                activeWeapon = Weapon3;
                break;
            case 4:
                activeWeapon = Weapon4;
                break;

        }
        //If swapping from one weapon to another refund the cost of the swap, upgrades are lost however
        if (activeWeapon.GetType().ToString() != "WeaponBase")
        {
            moneyManager.addMoney(swapCost);
            moneyManager.updateMoneyLabel();
        }
        //If the players ui is set to the current weapon type then update the ui
        if (wepCount == weaponCount)
        {
            islookingatWeaponChange = true;
        }
        //Switch to a certain weapon
        //If it alreay is the type then replace with nothing
        
        switch (WeaponType)
        {
            case "Railgun":
                if (activeWeapon.GetType().ToString() == "LaserTuret")
                {
                    activeWeapon = changeToNoWeapon(activeWeapon);
                }
                else
                {
                    
                    activeWeapon.Free();
                    activeWeapon = ResourceLoader.Load<PackedScene>("res://Scenes/Laser-Turet.tscn").Instantiate<LaserTuret>();
                    shapeship.AddChild(activeWeapon);
                    weaponint = 1;
                }
                break;
            case "LaserBeam":
                if (activeWeapon.GetType().ToString() == "LaserBeamTuret")
                {
                    activeWeapon = changeToNoWeapon(activeWeapon);
                }
                else
                {

                    activeWeapon.Free();
                    activeWeapon = ResourceLoader.Load<PackedScene>("res://Scenes/LaserBeam-Turet.tscn").Instantiate<LaserBeamTuret>();
                    shapeship.AddChild(activeWeapon);
                    weaponint = 2;
                }
                break;
            case "Missle":
                if (activeWeapon.GetType().ToString() == "MissleTuret")
                {
                    activeWeapon = changeToNoWeapon(activeWeapon);
                }
                else
                {

                    activeWeapon.Free();
                    activeWeapon = ResourceLoader.Load<PackedScene>("res://Scenes/Missle-Turet.tscn").Instantiate<MissleTuret>();
                    shapeship.AddChild(activeWeapon);
                    weaponint = 3;
                }
                break;
        }
      
        //Set name and posistion
        //Rotation as well for the bottem guns
        switch (wepCount)
        {
            case 1:
                activeWeapon.Name = "Weapon1";
                Weapon1 = activeWeapon;
                activeWeapon.Position = new Vector2(-138, -198);
                Upgrades["Weapon1"] = new int[] { weaponint,1,1,1};
                break;
            case 2:
                activeWeapon.Name = "Weapon2";
                Weapon2 = activeWeapon;
                activeWeapon.Position = new Vector2(127, -198);
                Upgrades["Weapon2"] = new int[] { weaponint, 1, 1, 1 };
                break;
            case 3:
                activeWeapon.Name = "Weapon3";
                Weapon3 = activeWeapon;
                activeWeapon.Position = new Vector2(-140, 169);
                activeWeapon.RotationDegrees = 90;
                Upgrades["Weapon3"] = new int[] { weaponint, 1, 1, 1 };
                break;
            case 4:
                activeWeapon.Name = "Weapon4";
                Weapon4 = activeWeapon;
                activeWeapon.Position = new Vector2(139, 169);
                activeWeapon.RotationDegrees = 90;
                Upgrades["Weapon4"] = new int[] { weaponint, 1, 1, 1 };
                break;
        }
        //Switches the Ui so it displays the correct weapon type
        if(islookingatWeaponChange == true) 
        {
            Label label = GetNode<Label>("MarginContainer2/PanelContainer/VSplitContainer/UpgradeContainer/HSplitContainer/Control Container/VBoxContainer/TypeLabel");
            switch (activeWeapon.GetType().Name)
            {
                case "LaserTuret":
                    label.Text = "Railgun";
                    break;
                case "LaserBeamTuret":
                    label.Text = "Laser";
                    break;
                case "MissleTuret":
                    label.Text = "Missle Launcher";
                    break;
                default:
                    label.Text = "No Weapon";
                    break;
            }
            ActiveWeapon = activeWeapon;
            UpdateUpgradeUi();
        }
        
        //GD.Print(activeWeapon.Name);
    }
    //Replace gun with weapon base, so i dont have to copy it 3 times
    //Yeah somtimes i can be efficent lol
    private Node2D changeToNoWeapon(Node2D activeWeapon)
    {
        //NOTE 
            //Keep a very Very close eye on this
            //Issue with que free is it takes too long and the multiplayer freakes out
            //Free does it fast enough but this could cause issues
            //If it does throw in a check for if it exists
            //For now leave as is
        activeWeapon.Free();
        activeWeapon = ResourceLoader.Load<PackedScene>("res://Scenes/WeaponBase.tscn").Instantiate<WeaponBase>();
        shapeship.AddChild(activeWeapon);
        //Swapping to no weapon will refund the money
        moneyManager.addMoney(swapCost);
        moneyManager.updateMoneyLabel();
        return activeWeapon;
    }

    private void _on_strength_button_pressed()
    {
        Rpc(nameof(UpdateStats), 1, weaponCount);
    }

    private void _on_fire_speed_button_pressed()
    {
        Rpc(nameof(UpdateStats), 2, weaponCount);
    }

    private void _on_shots_button_pressed()
    {
        Rpc(nameof(UpdateStats), 3, weaponCount);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdateStats(int stattoUpdrage,int rpcweaponcount)
    {
        if(ActiveWeapon.GetType().Name == "WeaponBase" || moneyManager.getMoney() < upgradeCost)
        {
            return;
        }
        //Upgrade the correct weapon
        switch (rpcweaponcount)
        {

            case 1:
                if (Upgrades["Weapon1"][stattoUpdrage] < 5 )
                {
                    if(Weapon1.GetType().Name == "LaserBeamTuret")
                    {
                        if(stattoUpdrage != 3)
                        {
                            Upgrades["Weapon1"][stattoUpdrage] = Upgrades["Weapon1"][stattoUpdrage] + 1;
                            UpdateWeaponInternal((Weapon)Weapon1, stattoUpdrage);
                            moneyManager.spendMoney(upgradeCost);
                            moneyManager.updateMoneyLabel();
                        }
                    }
                    else
                    {
                        Upgrades["Weapon1"][stattoUpdrage] = Upgrades["Weapon1"][stattoUpdrage] + 1;
                        UpdateWeaponInternal((Weapon)Weapon1, stattoUpdrage);
                        moneyManager.spendMoney(upgradeCost);
                        moneyManager.updateMoneyLabel();
                    }
                    
                }

                break;
            case 2:
                if (Upgrades["Weapon2"][stattoUpdrage] < 5 )
                {
                    if (Weapon2.GetType().Name == "LaserBeamTuret")
                    {
                        if (stattoUpdrage != 3)
                        {
                            Upgrades["Weapon2"][stattoUpdrage] = Upgrades["Weapon2"][stattoUpdrage] + 1;
                            UpdateWeaponInternal((Weapon)Weapon2, stattoUpdrage);
                            moneyManager.spendMoney(upgradeCost);
                            moneyManager.updateMoneyLabel();
                        }
                    }
                    else
                    {
                        Upgrades["Weapon2"][stattoUpdrage] = Upgrades["Weapon2"][stattoUpdrage] + 1;
                        UpdateWeaponInternal((Weapon)Weapon2, stattoUpdrage);
                        moneyManager.spendMoney(upgradeCost);
                        moneyManager.updateMoneyLabel();
                    }
                }
                break;
            case 3:
                if (Upgrades["Weapon3"][stattoUpdrage] < 5)
                {

                    if (Weapon3.GetType().Name == "LaserBeamTuret")
                    {
                        if (stattoUpdrage != 3)
                        {
                            Upgrades["Weapon3"][stattoUpdrage] = Upgrades["Weapon3"][stattoUpdrage] + 1;
                            UpdateWeaponInternal((Weapon)Weapon3, stattoUpdrage);
                            moneyManager.spendMoney(upgradeCost);
                            moneyManager.updateMoneyLabel();
                        }
                    }
                    else
                    {
                        Upgrades["Weapon3"][stattoUpdrage] = Upgrades["Weapon3"][stattoUpdrage] + 1;
                        UpdateWeaponInternal((Weapon)Weapon3, stattoUpdrage);
                        moneyManager.spendMoney(upgradeCost);
                        moneyManager.updateMoneyLabel();
                    }
                }
                break;
            case 4:
                if (Upgrades["Weapon4"][stattoUpdrage] < 5)
                {
                   
                    if (Weapon4.GetType().Name == "LaserBeamTuret")
                    {
                        if (stattoUpdrage != 3)
                        {
                            Upgrades["Weapon4"][stattoUpdrage] = Upgrades["Weapon4"][stattoUpdrage] + 1;
                            UpdateWeaponInternal((Weapon)Weapon4, stattoUpdrage);
                            moneyManager.spendMoney(upgradeCost);
                            moneyManager.updateMoneyLabel();
                        }
                    }
                    else
                    {
                        Upgrades["Weapon4"][stattoUpdrage] = Upgrades["Weapon4"][stattoUpdrage] + 1;
                        UpdateWeaponInternal((Weapon)Weapon4, stattoUpdrage);
                        moneyManager.spendMoney(upgradeCost);
                        moneyManager.updateMoneyLabel();
                    }
                }
                break;
        }
        UpdateUpgradeUi();
        
    }

    private void UpdateUpgradeUi()
    {
        Button strengthbutton = GetNode<Button>("MarginContainer2/PanelContainer/VSplitContainer/UpgradeContainer/HSplitContainer/Control Container/VBoxContainer/StrengthButton");
        strengthbutton.Text = "Strength: " + Upgrades[ActiveWeapon.Name][1] + "/5";
        Button speedthbutton = GetNode<Button>("MarginContainer2/PanelContainer/VSplitContainer/UpgradeContainer/HSplitContainer/Control Container/VBoxContainer/FireSpeedButton");
        speedthbutton.Text = "Shot Speed: " + Upgrades[ActiveWeapon.Name][2] + "/5";
        Button shotsthbutton = GetNode<Button>("MarginContainer2/PanelContainer/VSplitContainer/UpgradeContainer/HSplitContainer/Control Container/VBoxContainer/ShotsButton");
        shotsthbutton.Text = "Shots: " + Upgrades[ActiveWeapon.Name][3] + "/5";
        if(ActiveWeapon.GetType().Name == "LaserBeamTuret")
        {
            shotsthbutton.Text = "Shots : 1/1";
        }
    }

    private void UpdateWeaponInternal(Weapon weapon,int stattoUpgrade)
    {
        switch (stattoUpgrade)
        {
            case 1:
                weapon.setDamage((weapon.getDamage()+1));
                break;
            case 2:
                weapon.setshotspeed((weapon.getshotspeed()+1));
                break;
            case 3:
                weapon.setshots((weapon.getshots()+1));
                break;
            
        }
    }

    private void _on_repair_ship_pressed()
    {
        Rpc(nameof(RpcRepairShip));
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcRepairShip()
    {
        Spaceship ship = GetNode<Spaceship>("/root/World/Shapeship");
        if (ship.getHealth() < 90)
        {
            if (moneyManager.getMoney() >= 200)
            {
                moneyManager.spendMoney(200);
                moneyManager.updateMoneyLabel();
                ship.repairShip(10);
            }
        }
    }




}
