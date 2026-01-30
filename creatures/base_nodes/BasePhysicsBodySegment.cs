using Godot;
using Godot.Collections;
using System;

public partial class BasePhysicsBodySegment : RigidBody2D
{
	[Export] public Array<BaseLeg> Legs;
	RigidBody2D ParentRigidBody;

    public override void _Ready()
    {
        ParentRigidBody = GetParent<RigidBody2D>();
    }




	public int GetAmountOfLegsOnGround()
	{
		int n = 0;
		foreach(BaseLeg leg in Legs)
		{
			if(leg.CurrentState == BaseLeg.BaseLegState.Grab)
			{
				n++;
			}
		}
		return n;
	}
	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		int amountOfLegsOnGround = GetAmountOfLegsOnGround();

		if(amountOfLegsOnGround < 0)
		{
			return;
		}

		ApplyCentralForce(Vector2.Up * Mass * (float)ProjectSettings.GetSetting("physics/2d/default_gravity"));

	}
}
