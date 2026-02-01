using Godot;
using System;
using Godot.Collections;
public partial class BasePhysicsHead : RigidBody2D
{
	[Export] public Array<BasePhysicsBodySegment> bodySegments;
	[Export] public float AlignmentStrength = 500.0f; 
	[Export] public float DampeningStrength = 50.0f;  
	BasePhysicsCreature ParentRigidBody;

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		bool IsStabizized = bodySegments[0].GetAmountOfLegsOnGround() > 0;

		if (!IsStabizized)
		{
			GravityScale = 1.0f;
			return;
		}
		GravityScale = 0.0f;
		float AngleToGoal = GlobalPosition.AngleTo(ParentRigidBody.GoalPosition.GlobalPosition);
		StabilizeToAngleAndPosition(ParentRigidBody.GoalPosition.GlobalPosition, AngleToGoal);
		ApplyCentralForce(new Vector2(1,0) * 40f);

		
		

	}
	public override void _Ready()
	{
		ParentRigidBody = GetParent<BasePhysicsCreature>();

	}
public void StabilizeToAngleAndPosition(Vector2 targetPosition, float targetAngleRad, float strength = 50.0f)
{

    Vector2 positionDiff = (targetPosition - GlobalPosition).Normalized() * 200;
    
    Vector2 forceSpring = positionDiff * strength;
    

   
    Vector2 forceDamping = -LinearVelocity * (strength * 0.2f); 

    ApplyCentralForce(forceSpring + forceDamping);


    float angleDiff = Mathf.DegToRad(Mathf.LerpAngle(Mathf.RadToDeg(Rotation), Mathf.RadToDeg(targetAngleRad), 1.0f)) - Rotation;
    
    float torqueSpring = angleDiff * AlignmentStrength;
    float torqueDampen = -AngularVelocity * DampeningStrength;

    ApplyTorque(torqueSpring + torqueDampen);
}
}
