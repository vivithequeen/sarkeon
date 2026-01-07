using Godot;
using System;
using System.IO.Pipes;

public partial class BaseLeg : Skeleton2D
{
	RayCast2D WallChecker;

	int WallCheckerRotationIndex = -45;

	Vector2 GrabbedBodyLocation;
	bool HasGrabbedBody = false;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		WallChecker = GetNode<RayCast2D>("WallChecker");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GetNode<Node2D>("test_target").GlobalPosition = GetGlobalMousePosition();
		GetNode<Node2D>("test_target").GlobalPosition = GrabbedBodyLocation;

		WallCheckerLogic();

	}
	public void WallCheckerLogic()
	{
		WallChecker.Rotation = Mathf.DegToRad(WallCheckerRotationIndex);
		if(WallChecker.IsColliding() && !HasGrabbedBody)
		{
			GrabbedBodyLocation = WallChecker.GetCollisionPoint();
			HasGrabbedBody = true;
		}

		WallCheckerRotationIndex+=15;
		if(WallCheckerRotationIndex >= 45)
		{
			WallCheckerRotationIndex = -45;
		}
	}
}
