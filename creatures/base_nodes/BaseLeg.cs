using Godot;
using System;
using System.IO.Pipes;

public partial class BaseLeg : Skeleton2D
{
	RayCast2D WallChecker;
	Node2D Target;
	Node2D ResetPosition;
	int WallCheckerRotationIndex = 45;

	Vector2 GrabbedBodyLocation;
	//Node2D GrabbedBody;


	public enum BaseLegState
	{
		Search,
		Reset,
		Grab,
	}

	BaseLegState CurrentBaseLegState = BaseLegState.Search;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		ResetPosition = GetNode<Node2D>("ResetPosition");
		Target = GetNode<Node2D>("Target");
		WallChecker = GetNode<RayCast2D>("WallChecker");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		



		WallCheckerLogic(delta);

	}
	public void WallCheckerLogic(double delta)
	{
		GD.Print(CurrentBaseLegState);


		if (WallChecker.IsColliding() && CurrentBaseLegState != BaseLegState.Grab)
		{
			GrabbedBodyLocation = WallChecker.GetCollisionPoint();

			//var collider = WallChecker.GetCollider();
			//GrabbedBody = collider as Node2D;

			CurrentBaseLegState = BaseLegState.Grab;
		}
		if (!WallChecker.IsColliding() && CurrentBaseLegState == BaseLegState.Grab)
		{
			CurrentBaseLegState = BaseLegState.Reset;
		}

		if (CurrentBaseLegState == BaseLegState.Grab)
		{
			Tween tween = GetTree().CreateTween();

			tween.TweenProperty(Target, "global_position", GrabbedBodyLocation, delta * 40.0f);
			WallChecker.GlobalRotation = (GrabbedBodyLocation - GlobalPosition).Angle();
			//GrabbedBodyLocation = WallChecker.GetCollisionPoint();
		}

		if(CurrentBaseLegState == BaseLegState.Search)
		{
			WallChecker.Rotation = Mathf.DegToRad(WallCheckerRotationIndex);
		}

		if (CurrentBaseLegState == BaseLegState.Reset)
		{
			Tween tween = GetTree().CreateTween();
			WallChecker.Rotation = Mathf.DegToRad(WallCheckerRotationIndex);
			tween.TweenProperty(Target, "global_position", ResetPosition.GlobalPosition, delta * 40.0f);
			tween.TweenCallback(Callable.From(() => CurrentBaseLegState = BaseLegState.Search));

		}

	}


}
