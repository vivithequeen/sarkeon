using Godot;
using System;
using Godot.Collections;
public partial class BasePhysicsCreature : RigidBody2D
{
	[Export] public Node2D GoalPosition;
	[Export] public Array<BasePhysicsBodySegment> bodySegments;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		StabilizeForce();
		GravityScale = 0.0f;
		return;
		ApplyCentralForce(Vector2.Up * Mass * (float)ProjectSettings.GetSetting("physics/2d/default_gravity"));
		ApplyCentralForce((GoalPosition.GlobalPosition - GlobalPosition).Normalized() * 10000);
		LookAt(GoalPosition.GlobalPosition);
		foreach(RigidBody2D b in FindChildren("*", "RigidBody2D", true, false))
		{
			b.ApplyCentralForce(Vector2.Up * Mass * (float)ProjectSettings.GetSetting("physics/2d/default_gravity"));

		}
	}

	public void StabilizeForce(float Strength = 50.0f)
	{
		ApplyCentralForce(-LinearVelocity * Strength);
		ApplyTorque(-AngularVelocity * Strength);
	}
}
