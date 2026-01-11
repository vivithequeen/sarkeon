using Godot;
using System;

public partial class BodySegmentBase : Node2D
{
	public void InitSegment(Vector2 InitPosition)
	{
		GD.Print("I HAVE BEEN BORN");
		GlobalPosition = InitPosition;
	}

	public void FollowTarget(Vector2 TargetPosition, double delta)
	{
		float DistanceToTarget = GlobalPosition.DistanceTo(TargetPosition);
		Vector2 DirectionToTarget = GlobalPosition.DirectionTo(TargetPosition);
		LookAt(TargetPosition);


		if(DistanceToTarget > 64)
		{
			GlobalPosition = GlobalPosition.MoveToward(TargetPosition, 300 * (float)delta);
		}
	}
}
