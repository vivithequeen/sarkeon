using Godot;
using System;

public partial class BaseCritter : Node
{
	// Called when the node enters the scene tree for the first time.
	public Vector2 GoalPosition;
	public enum CritterGoals {
		Hunting,
		DesperateHunting,
		Breeding,
		Tired,
	}
	
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
