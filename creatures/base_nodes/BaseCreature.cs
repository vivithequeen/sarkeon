using Godot;
using System;
using Godot.Collections.Array;
public partial class BaseCreature : Node2D
{
	// Called when the node enters the scene tree for the first time.
	

	[Export] public BaseSensors baseSensors;

	
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//Array<Array<Variant>> SeenObjects =baseSensors.EyeStep()
		//if ()
		//{
		//	
		//}
	}
}
