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
	public Vector2 TargetPos = new Vector2(0, 0);

	public Line2D navLine = new();
	public override void _Ready()
	{
		GetParent().CallDeferred(Node.MethodName.AddChild, navLine);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		navLine.ClearPoints();

		Array<Vector2I> Path = navigationSystem.GetPath(GlobalPosition / 4, GetGlobalMousePosition() / 4);
		if (Path.Count > 1)
		{
			if (Path[1].X * Path[1].Y > 0)
			{
				TargetPos = Path[1] * 4;

				foreach (Vector2I pos in Path)
				{
					navLine.AddPoint(pos * 4);
				}
			} // TODO: update
		}

	}
}
