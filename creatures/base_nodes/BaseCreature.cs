using Godot;
using System;
using Godot.Collections;
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
		if (baseSensors != null)
		{
			Array<Dictionary<string, Variant>> SeenObjects = baseSensors.SearchWithEyes();
			if (SeenObjects != null)
			{
				GD.Print(SeenObjects);
			}
		}
	}
}
