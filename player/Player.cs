using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float JumpVelocity = -400.0f;

	public float DesiredSpeed = 0;
	public enum PlayerState
	{
		Idle,
		Walking,
		Air,
	}
	//accelleration
	//speed
	//drag

	//Idle
	public int IdleDrag = 300;
	public Vector2 IdleState(double delta, Vector2 velocity)
	{
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		GD.Print(direction);
		if (direction.X != 0)
		{
			CurrentPlayerState = PlayerState.Walking;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, IdleDrag);
		}
		return velocity;
	}
	//Walking
	public float WalkingAcceleration = 30.0f;
	public int WalkingSpeed = 300;
	public float WalkingDrag = 40.0f;
	public Vector2 WalkingState(double delta, Vector2 velocity)
	{
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		if (direction.X != 0)
		{
			DesiredSpeed = direction.X * WalkingSpeed;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,WalkingAcceleration);
		}
		else
		{
			DesiredSpeed= 0;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,WalkingDrag);
			//CurrentPlayerState = PlayerState.Idle;
		}
		GD.Print(DesiredSpeed + " " + velocity.X);
		return velocity;
	}
	//Air
	public float AirAcceleration = 10.0f;
	public int AirSpeed = 100;
	public float AirDrag = 20.0f;
	public Vector2 AirState(double delta, Vector2 velocity)
	{

		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		if (direction != Vector2.Zero)
		{
			DesiredSpeed = direction.X * WalkingSpeed;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,AirAcceleration);
		}
		else
		{
			DesiredSpeed = direction.X * WalkingSpeed;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,AirAcceleration);
		}
		return velocity;
	}

	public PlayerState CurrentPlayerState = PlayerState.Walking;



	

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if(CurrentPlayerState == PlayerState.Air)
		{
			velocity = AirState(delta, velocity);
			
		}
		else if(CurrentPlayerState == PlayerState.Idle)
		{
			velocity = IdleState(delta, velocity);
		}
		else if(CurrentPlayerState == PlayerState.Walking)
		{
			velocity = WalkingState(delta, velocity);
		}

		// Add the gravity.
		if (IsOnFloor())
		{
			CurrentPlayerState = PlayerState.Walking;
		}
		else
		{
			CurrentPlayerState = PlayerState.Air;
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.

		//GD.Print(CurrentPlayerState);
		Velocity = velocity;
		MoveAndSlide();
	}
}
