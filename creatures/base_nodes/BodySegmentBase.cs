using Godot;
using System;

public partial class BodySegmentBase : Node2D
{
	public void InitSegment(Vector2 InitPosition)
	{
		GD.Print("I HAVE BEEN BORN");
		Position = InitPosition;
	}

	public void FollowTarget(Vector2 TargetPos, double delta)
	{
		float DistanceToTarget = Position.DistanceTo(TargetPos);
		Vector2 DirectionToTarget = Position.DirectionTo(TargetPos);


		if(DistanceToTarget > 64)
		{
			Position = Position.MoveToward(TargetPos, 300 * (float)delta);
		}
	}
}
