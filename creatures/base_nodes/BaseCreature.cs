using Godot;
using System;
using Godot.Collections;
using System.IO;
using System.Linq;
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
		navLine.DefaultColor = new Color("00ffff6b");
		navLine.JointMode = Line2D.LineJointMode.Round;
		Array<Vector2I> Path = navigationSystem.GetPath(GlobalPosition / 4, new Vector2I(100, 100));
		GD.Print(Path);
		baseCritter.SetCurrentCritterNavigationPath(Path);


	}

	// Called every frame. 'delta' is the elapsed time since the previo	us frame.
	public override void _Process(double delta)
	{
		UpdatePathTimer += (float)delta;

		if (UpdatePathTimer > 0.5)
		{
			navLine.ClearPoints();
			UpdatePathTimer = 0;
			Array<Vector2I> Path = navigationSystem.GetPath(GlobalPosition / 4, GetGlobalMousePosition() / 4);

			GD.Print(GlobalPosition, GetGlobalMousePosition());
			if (Path.Count != 0)
			{
				baseCritter.SetCurrentCritterNavigationPath(Path);
			}


			Array<Vector2I> tempPath = baseCritter.GetCurrentCritterNavigationPath();
			foreach (Vector2 p in tempPath)
			{
				navLine.AddPoint(p * 4);
			}

		}

		//every 3rd frame
		if (baseCritter.UpdateCritterNavigationPath(GlobalPosition / 4))
		{
			if (navLine.GetPointCount() > 0)
			{
				navLine.RemovePoint(0);
			}
		}

		TargetPos = baseCritter.GetCurrentCritterNavigationPoint(0) * 4;
		if (TargetPos.X == -4)
		{
			TargetPos = GlobalPosition;
		}
		//GD.Print(PathIndex);
		//end every 3rd frame


	}
}
