using Godot;
using System;

public partial class TailBase : BodySegmentBase
{
	public float FollowDistance = 15;
	public override void FollowTarget(Vector2 TargetPosition, double delta)
	{
		float DistanceToTarget = GlobalPosition.DistanceTo(TargetPosition);
		Vector2 DirectionToTarget = GlobalPosition.DirectionTo(TargetPosition);
		LookAt(TargetPosition);


		if (DistanceToTarget > FollowDistance)
		{
			GlobalPosition = GlobalPosition.MoveToward(TargetPosition, 300 * (float)delta);
		}
	}

}
