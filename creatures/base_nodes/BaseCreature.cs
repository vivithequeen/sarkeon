using Godot;
using System;
using Godot.Collections;
using System.IO;
public partial class BaseCreature : Node2D
{
	// Called when the node enters the scene tree for the first time.
	

	[Export] public BaseSensors baseSensors;

	[Export] public BaseCritter baseCritter;

	[Export] public NavigationSystem navigationSystem;
	public Vector2 TargetPos = new Vector2(0,0);

	public Line2D navLine = new();
	public override void _Ready()
	{
		GetParent().CallDeferred(Node.MethodName.AddChild, navLine);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		navLine.ClearPoints();
		
		Array<Vector2I> Path = navigationSystem.GetPath(GlobalPosition, GetGlobalMousePosition());
		if(Path.Count != 0)
		{
			TargetPos = Path[3];
		}
		foreach(Vector2I pos in Path)
		{
			navLine.AddPoint(pos);
		}
	}
}
