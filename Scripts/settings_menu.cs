using Godot;
using System;

public partial class settings_menu : PanelContainer
{
    private void _on_go_back_button_pressed()
    {
        //Toggle visablity of the settings menu for both places it appears
        var node = GetParent();
        if(node.GetType().Name == "GameMenu")
        {
            GameMenu menu = (GameMenu)node;
            PanelContainer main = menu.GetNode<PanelContainer>("Main Menu");
            main.Show();
        }
        else if (node.GetType().Name == "MainMenu")
        {
            MainMenu menu = (MainMenu)node;
            PanelContainer main = menu.GetNode<PanelContainer>("MainMenu");
            main.Show();
        }
        Hide();
    }
}
