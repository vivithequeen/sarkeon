using Godot;
using Godot.Collections;
using System;
using System.ComponentModel;

public partial class BasePhysicsBodySegment : RigidBody2D
{
	[Export] public Array<BaseLeg> Legs;

	[Export] public BasePhysicsHead Head;
	[Export] public RayCast2D GroundChecker;






	public int GetAmountOfLegsOnGround()
	{
		int n = 0;
		foreach (BaseLeg leg in Legs)
		{
			if (leg.CurrentState == BaseLeg.BaseLegState.Grab)
			{
				n++;
			}
		}
		return n;
	}
	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{


		int amountOfLegsOnGround = GetAmountOfLegsOnGround();

		if (amountOfLegsOnGround == 0)
		{
			GravityScale = 1.0f;
			return;
		}



		GravityScale = 0.0f;
		StabilizeForce();

		GroundChecker.LookAt(GlobalPosition + new Vector2(1, 0));
		if (GroundChecker.IsColliding())
		{
			ApplyCentralForce(Vector2.Up * Mass * (float)ProjectSettings.GetSetting("physics/2d/default_gravity"));
			if (Head != null)
			{
				Head.ApplyCentralForce(Vector2.Up * Mass * (float)ProjectSettings.GetSetting("physics/2d/default_gravity"));

			}
		}
		// 
	}


	public void StabilizeForce(float Strength = 50.0f)
	{
		ApplyCentralForce(-LinearVelocity * Strength);
		ApplyTorque(-AngularVelocity * Strength);
	}

}
