using Godot;
using System;
using Godot.Collections;
public partial class BasePhysicsCreature : RigidBody2D
{
	[Export] public Node2D GoalPosition;
	[Export] public Array<BasePhysicsBodySegment> bodySegments;

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		bool IsStabizized = bodySegments[0].GetAmountOfLegsOnGround() > 0;

		if (!IsStabizized)
		{
			GravityScale = 1.0f;
			return;
		}
		GravityScale = 0.0f;
		//StabilizeForce();

		float AngelToGoal = GlobalPosition.AngleTo(GoalPosition.GlobalPosition);
		ApplyTorque(AngelToGoal);

	}

	public void StabilizeForce(float Strength = 50.0f)
	{
		ApplyCentralForce(-LinearVelocity * Strength);
		ApplyTorque(-AngularVelocity * Strength);
	}
	
}
