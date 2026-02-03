using Godot;
using System;
using Godot.Collections;
public partial class BasePhysicsCreature : Node2D
{

	[Export] public NavigationSystem navigationSystem;
	public Vector2 TargetPos = new Vector2(4, 4);

	float UpdatePathTimer = 0;
	[Export] public BasePhysicsHead basePhysicsHead;
	[Export] public BaseCritter baseCritter;

	public Line2D navLine = new();
	public override void _Ready()
	{
		GetParent().CallDeferred(Node.MethodName.AddChild, navLine);
		navLine.DefaultColor = new Color("00ffff6b");
		navLine.JointMode = Line2D.LineJointMode.Round;
		Array<Vector2I> Path = navigationSystem.GetPath(GlobalPosition / 4, new Vector2I(100, 100));


		baseCritter.SetCurrentCritterNavigationPath(Path);


	}

	public override void _Process(double delta)
	{
		UpdatePathTimer += (float)delta;

		if (UpdatePathTimer > 0.5)
		{
			navLine.ClearPoints();
			UpdatePathTimer = 0;
			Array<Vector2I> Path = navigationSystem.GetPath(basePhysicsHead.GlobalPosition / 4, GetGlobalMousePosition() / 4);

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
		if (baseCritter.UpdateCritterNavigationPath(basePhysicsHead.GlobalPosition / 4))
		{
			if (navLine.GetPointCount() > 0)
			{
				navLine.RemovePoint(0);
			}
		}

		TargetPos = baseCritter.GetCurrentCritterNavigationPoint(0) * 4;
		if (TargetPos.X == -4)
		{
			TargetPos = basePhysicsHead.GlobalPosition;
		}
		//GD.Print(PathIndex);
		//end every 3rd frame

		// Array<Node2D> seenObjects = baseSensors.SearchWithEyes();
		// if (seenObjects != null)
		// {
		// 	//GD.Print(seenObjects);
		// }


	}
}
