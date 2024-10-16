using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EnemyManager : Node2D
{
	//On occasion a enemy isnt created fast enough
	//A sife effect of using defered i suppose
	//This is to track the enemies and make sure the enemy exists for everyone

	private List<string> enemyNames = new List<string>();
	private List<Entity> enemies = new List<Entity>();
	private List<List<string>> otherEnemyNames = new List<List<string>>();
	private Timer timer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timer = GetNode<Timer>("Timer");
		
	}

	public void reset()
	{
        enemyNames = new List<string>();
		enemies = new List<Entity>();
		otherEnemyNames = new List<List<string>>();
}

	public void startTimer()
	{
        timer.Start(5);
    }

	public void stopTimer() 
	{ 
		timer.Stop(); 
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void addtoEnemies(Entity enemy)
	{
		enemies.Add(enemy);
	}

	public List<string> getEnemyNames()
	{
		return enemyNames;
	}

	public void addtoEnemyList(string name)
	{
		enemyNames.Add(name);
	}

	public void removefromEnemyList(string name)
	{
		enemyNames.Remove(name);
	}

	private void _on_timer_timeout()
	{
		//Reset other enemy names
		otherEnemyNames = new List<List<string>>();
        //Send out RPC to others
        if (IsMultiplayerAuthority()) 
		{ 
			Rpc(nameof(sendEnemyList));
		}
		timer.Start(5);
	}

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void sendEnemyList()
	{
		string[] names = enemyNames.ToArray();
        Rpc(nameof(receiveEnemyList), names);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void receiveEnemyList(string[] enemynames)
	{
		
		if(IsMultiplayerAuthority() )
		{
            List<string> names = enemynames.ToList<string>();
			otherEnemyNames.Add(names);
			foreach( var list in  otherEnemyNames )
			{
				if (enemyNames.SequenceEqual(list))
				{
					GD.Print("True");
				}
				else
				{
                    GD.Print("False");
					//RPC to everyone to remove bad elements
					var intersect = enemyNames.Intersect(list).ToList();
					foreach(var inter in intersect)
					{
						Rpc(nameof(rpcRemoveName), inter);
					}
                }
			}
        }
	}
	//Removes the instance of enemy from all players
	//Keep an eye on this, its a very violent solution to a rare problem
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void rpcRemoveName(string name)
	{
		if (enemyNames.Contains(name))
		{
			enemyNames.Remove(name);
			foreach(var enemy in enemies)
			{
				if(enemy.Name == name)
				{
					enemy.QueueFree();
				}
			}
		}
	}
}
