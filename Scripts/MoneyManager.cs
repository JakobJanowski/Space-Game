using Godot;
using System;

public partial class MoneyManager : Node2D
{
    //TEMP should be 100
    private int money = 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void addMoney(int amount)
    {
        money += amount;
    }

    public void spendMoney(int amount)
    {
        money -= amount;
    }

    public int getMoney()
    {
        return money;
    }

    public void updateMoneyLabel()
    {
        RichTextLabel moneyLabel = GetNode<RichTextLabel>("/root/World/Interface/Interface/Panel/MoneyLabel");
        moneyLabel.Text = "Funds: $" + money;
    }

    public void resetMoney()
    {
        money = 100;
    }
}
