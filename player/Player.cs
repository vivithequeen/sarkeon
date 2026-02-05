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
	public Vector2I prev_position = Vector2I.Zero;
	public int IdleDrag = 300;
	public Vector2 IdleState(double delta, Vector2 velocity)
	{
		float direction = Input.GetAxis("left", "right");
		if (direction != 0)	
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
	public float WalkingAcceleration = 60.0f;
	public int WalkingSpeed = 300;
	public float WalkingDrag = 60.0f;
	[Export]
	Sand sand;
	public Vector2 WalkingState(double delta, Vector2 velocity)
	{
		float direction = Input.GetAxis("left", "right");
		if (direction != 0)
		{
			DesiredSpeed = direction * WalkingSpeed;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,WalkingAcceleration);
		}
		else
		{
			DesiredSpeed= 0;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,WalkingDrag);
			//CurrentPlayerState = PlayerState.Idle;
		}
		return velocity;
	}
	//Air
	public float AirAcceleration = 40.0f;
	public int AirSpeed = 200;
	public float AirDrag = 20.0f;
	public Vector2 AirState(double delta, Vector2 velocity)
	{

		float direction = Input.GetAxis("left", "right");
		if (direction != 0)
		{
			DesiredSpeed = direction * WalkingSpeed;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,AirAcceleration);
		}
		else
		{
			DesiredSpeed = direction * WalkingSpeed;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,AirDrag);
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
		if (sand != null)
		{
			Vector2I next_position = (Vector2I)((GlobalPosition - sand.GlobalPosition) / (sand.chunk_size * sand.Scale));
			if (prev_position != next_position)
			{
				prev_position = next_position;
				sand.load_pos = next_position;
				sand.newPos(next_position);
			}
			// GD.Print(sand.load_pos);
		}
		MoveAndSlide();
	}
    public override void _Ready()
    {
        Vector2I next_position = (Vector2I)((GlobalPosition - sand.GlobalPosition) / (sand.chunk_size * sand.Scale));
		sand.load_pos = next_position;
		sand.newPos(next_position);
    }
}
