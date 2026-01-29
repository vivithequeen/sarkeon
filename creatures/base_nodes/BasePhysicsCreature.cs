using Godot;
using System;

public partial class BasePhysicsCreature : RigidBody2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
public override void _IntegrateForces(PhysicsDirectBodyState2D state)	{
		ApplyCentralForce(Vector2.Up * 2*Mass * (float)ProjectSettings.GetSetting("physics/2d/default_gravity"));

	}
}
