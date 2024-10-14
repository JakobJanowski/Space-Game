using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public partial class PlayerManager : Node2D
{

	
	private string yourplayer = null;
	//Note realy wished i knew about the godot version of the datatypes before 
	private Godot.Collections.Dictionary<long,string> playerList = new Godot.Collections.Dictionary<long,string>();
	private bool isinMenu = false;
	//Time for sillyness
	
	
	public void addToPlayerList(long player)
	{
		playerList.Add(player,player.ToString());
	}

	public void removeFromPlayerList(long player) 
	{  
		playerList.Remove(player);
	}	

	public void setYourPlayer(String player) 
	{
		yourplayer = player;
	}

	public void updatePlayerName(long player,string newName)
	{
		if (playerList.ContainsKey(player))
		{
			playerList[player] = new string(newName);
		}
	}
  

	

	

	
	//Returns the player name. As this is called once localy it will always return the name
	//Of the player you are controlling. A replacement for getmultiplayer authority as it doesnt work
	public String getYourPlayer() 
	{
		return yourplayer;
	}

	public Godot.Collections.Dictionary<long,string> getPlayerList()
	{
		return playerList;
	}

	public void updatePlayerlist(Godot.Collections.Dictionary<long, string> _playerList)
	{
		playerList = _playerList;
		

	}

	public void toggleMenuStatus(bool status)
	{
		if(status)
		{
			isinMenu = true;
		}
		else
		{
			isinMenu= false;
		}
	}

	public bool getMenuStatus()
	{
		return isinMenu;
	}

}
