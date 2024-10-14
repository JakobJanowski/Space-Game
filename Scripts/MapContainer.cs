using Godot;
using System;
using System.Collections.Generic;

public partial class MapContainer : MarginContainer
{
	//Get thing for spaceship
	private Spaceship playship;
	private TextureRect playerrect;
    private TextureRect box;
    const int defaultX = 1152;
    const int defaultY = 648;
    // Called when the node enters the scene tree for the first time
    // 
    //Renders are objects to render
    private List<Node2D> renders = new List<Node2D>();
    //Rects is a list of things to render
    private List<TextureRect> rects = new List<TextureRect>();

    //Where
    //1 is add
    //2 is remove and add
    //3 is remove
    private int taskStage = 1;
    private Node2D TaskObjective;
    private bool isSettingUp = true;

    public override void _Ready()
	{
        box = GetNode<TextureRect>("Panel/HBoxContainer/TextureRect");
        playship = GetNode<Spaceship>("/root/World/Shapeship");
		playerrect = GetNode<TextureRect>("Panel/HBoxContainer/TextureRect/Player");

        Node2D spanwers = GetNode<Node2D>("/root/World/EnemySpawners");
        var children = spanwers.GetChildren();

       

        TaskHandler handler = GetNode<TaskHandler>("/root/World/Environment/TaskHandler");
        handler.taskProgress += Handler_taskProgress;
        handler.updateMap += Handler_updateMap;
        handler.TaskOver += Handler_TaskOver;

        //Get all enemy spawners
        foreach (var child in children)
        {
            if (child.IsInGroup("EnemySpawn"))
            {
                addtorenderList((Node2D)child);
            }
        }
        //Load on map
        foreach(var ob in renders){

            TextureRect rect = new TextureRect();
            rect.Texture = (Texture2D)ResourceLoader.Load("res://Sprites/EnemySpawner.png");
            box.AddChild(rect);
            rects.Add(rect);
        }
        isSettingUp = false;
    }

    private void Handler_TaskOver()
    {
        taskStage = 3;
        Handler_updateMap(TaskObjective);
    }

    public void removeRender(Node2D render)
    {
        if (renders.Contains(render))
        {
            renders.Remove(render);
        }
    }

    private void Handler_updateMap(Node2D createdobj)
    {
        switch(taskStage)
        {
            case 1:
                addtorenderList(createdobj);
                TaskObjective = createdobj;
                taskStage = 2;
                break;
            case 2:
                removefromrenderList(TaskObjective);
                addtorenderList(createdobj);
                TaskObjective = createdobj;
                taskStage = 3;
                break;
            case 3:
                removefromrenderList((Node2D)createdobj);
                TaskObjective = null;
                taskStage = 1;
                break;
            default:
                break;
        }
    }

    private void Handler_taskProgress(int num, int progress)
    {
        switch (num)
        {
            case 1:
                if(progress == 2)
                {
                    //Just in case
                    taskStage = 3;
                    Handler_updateMap(TaskObjective);
                }
                break;
            default:
                break;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if(box.Visible == true)
        {
            
            playerrect.Position = processPosistion(playship);
            playerrect.Rotation = playship.Rotation;
            playerrect.RotationDegrees = playship.RotationDegrees + 90;
            for(int i = 0; i < rects.Count; i = i + 1)
            {
                rects[i].Position = processPosistion(renders[i]);
                if (rects[i].IsInGroup("Destination") || rects[i].IsInGroup("Crate"))
                {
                    rects[i].Position = new Vector2(rects[i].Position.X - (rects[i].GetRect().Size.X/2), rects[i].Position.Y - (rects[i].GetRect().Size.Y / 2));
                }
            }

        }

    }

    private Vector2 processPosistion(Node2D node)
    {
        //
      


        float scalex = 18000 / box.GetRect().Size.X;
        float scaley = 18000 / box.GetRect().Size.Y;
        //X+10+50

        Vector2 translatedPos = node.GlobalPosition;
        translatedPos = new Vector2(translatedPos.X + 2900, translatedPos.Y + 2700);
        translatedPos = new Vector2((float)(translatedPos.X / scalex), (float)(translatedPos.Y / scaley));

        
        return translatedPos;
        
    }

    public void addtorenderList(Node2D node)
    {
        renders.Add(node);
        if(isSettingUp == false)
        {
            TextureRect rect = new TextureRect();
            rect.Texture = (Texture2D)ResourceLoader.Load("res://Sprites/objectivecircle.png");
            box.AddChild(rect);
            rect.Scale = new Vector2(5, 5);
            rect.AddToGroup("Destination");
            rects.Add(rect);
        }
       
        
    }

    public void removefromrenderList(Node2D node)
    {
        
        //Remove everything
        //Not sure if this will solve the index out of range issue, does at least
        //prevent the crash
        if(renders.Contains(node))
        {
            int index = renders.IndexOf(node);
            TextureRect rect = rects[index];
            rect.QueueFree();
            rects.RemoveAt(index);
            renders.Remove(node);
            //If still there do it again
            if (renders.Contains(node))
            {
                index = renders.IndexOf(node);
                rect = rects[index];
                rect.QueueFree();
                rects.RemoveAt(index);
                renders.Remove(node);
            }
        }
       
        

      
    }
}
