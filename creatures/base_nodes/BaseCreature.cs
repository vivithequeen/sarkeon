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
	public Vector2 TargetPos = new Vector2(4, 4);


	float UpdatePathTimer = 0;

	public Line2D navLine = new();
	public override void _Ready()
	{
		GetParent().CallDeferred(Node.MethodName.AddChild, navLine);
		Array<Vector2I> Path = navigationSystem.GetPath(GlobalPosition / 4, new Vector2I(100,100));
		GD.Print(Path);
		baseCritter.UpdateCritterNavigation(Path);
		

	}

	// Called every frame. 'delta' is the elapsed time since the previo	us frame.
	public override void _Process(double delta)
	{
		UpdatePathTimer += (float)delta;

		if (UpdatePathTimer > 0.5)
		{
			UpdatePathTimer = 0;
			Array<Vector2I> Path = navigationSystem.GetPath(GlobalPosition / 4, GetGlobalMousePosition() / 4);

			GD.Print(GlobalPosition, GetGlobalMousePosition());
			if (Path.Count != 0)
			{
				baseCritter.UpdateCritterNavigation(Path);
			}
		}

		//every 3rd frame
		baseCritter.GetNextCritterNavigationPointIndex(GlobalPosition / 4);

		TargetPos = baseCritter.GetCurrentCritterNavigationPoint(0) * 4;
		if(TargetPos.X == -4)
		{
			TargetPos = GlobalPosition;
		}
		//GD.Print(PathIndex);
		//end every 3rd frame


	}
}
