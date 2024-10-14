using Godot;
using System;

public abstract partial class Entity : CharacterBody2D
{
	
	public abstract void takeDamage(int damage);
}
