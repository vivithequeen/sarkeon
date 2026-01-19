using Godot;
using System;
using System.IO.Pipes;

public partial class BaseLeg : Skeleton2D
{
	RayCast2D WallChecker;
	Node2D Target;
	Node2D ResetPosition;
	[Export]
	int WallCheckerRotationIndex = 45;
	[Export]
	int StartWallCheckerRotation = 45;

	bool FirstStep = false;
	Vector2 GrabbedBodyLocation;
	//Node2D GrabbedBody;

	bool isTweening = false;

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


		Target = GetNode<Node2D>("Target");
		

		ResetPosition = GetNode<Node2D>("ResetPosition");

		
		WallChecker = GetNode<RayCast2D>("WallChecker");
		

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		WallCheckerLogic(delta);
	}
	public void WallCheckerLogic(double delta)
	{

		if (WallChecker.IsColliding() && CurrentBaseLegState == BaseLegState.Search)
		{
			GrabbedBodyLocation = WallChecker.GetCollisionPoint();

			//var collider = WallChecker.GetCollider();
			//GrabbedBody = collider as Node2D;

			CurrentBaseLegState = BaseLegState.Grab;
			if (!FirstStep)
			{
				FirstStep = true;
			}
			
		}
		if (!WallChecker.IsColliding() && CurrentBaseLegState == BaseLegState.Grab)
		{
			CurrentBaseLegState = BaseLegState.Reset;
			isTweening = false;
		}

		if (CurrentBaseLegState == BaseLegState.Grab)
		{
			Tween tween = GetTree().CreateTween();

			tween.TweenProperty(Target, "global_position", GrabbedBodyLocation, 0.1f);
			WallChecker.GlobalRotation = (GrabbedBodyLocation - GlobalPosition).Angle();
			//GrabbedBodyLocation = WallChecker.GetCollisionPoint();
		}

		if(CurrentBaseLegState == BaseLegState.Search)
		{
			WallChecker.Rotation = Mathf.DegToRad( FirstStep ? WallCheckerRotationIndex : StartWallCheckerRotation);
		}

		if (CurrentBaseLegState == BaseLegState.Reset)
		{
			Tween tween = GetTree().CreateTween();
			WallChecker.Rotation = Mathf.DegToRad( FirstStep ? WallCheckerRotationIndex : StartWallCheckerRotation);
			tween.TweenProperty(Target, "global_position", ResetPosition.GlobalPosition, 0.1f);
			tween.TweenCallback(Callable.From(() => CurrentBaseLegState = BaseLegState.Search));

		}

	}

	public void Reset()
	{
		CurrentBaseLegState = BaseLegState.Reset;
		WallChecker.Rotation = Mathf.DegToRad(WallCheckerRotationIndex);
		if (WallChecker.IsColliding())
		{
			GrabbedBodyLocation = WallChecker.GetCollisionPoint();
		}
	}


}
