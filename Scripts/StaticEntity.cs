using Godot;
using System;

public abstract partial class StaticEntity : StaticBody2D
{
    public abstract void takeDamage(int damage);
}
