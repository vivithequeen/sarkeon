using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float JumpVelocity = -800.0f;

	public float DesiredSpeed = 0;
	public enum PlayerState
	{
		Idle,
		Walking,
		Air,
		Water,
	}
	//accelleration
	//speed
	//drag

	//Idle
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
	[Export]
	Area2D collision;
	int water_deep = 0;
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
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}
		if (!IsOnFloor())
		{
			CurrentPlayerState = PlayerState.Air;
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
		velocity += GetGravity() * (float)delta;
		if (IsOnFloor())
		{
			CurrentPlayerState = PlayerState.Walking;
		}
		return velocity;
	}

	public PlayerState CurrentPlayerState = PlayerState.Walking;

	public Vector2 WaterState(double delta, Vector2 velocity)
	{
		float direction = Input.GetAxis("left", "right");
		if (direction != 0)
		{
			DesiredSpeed = direction * WalkingSpeed * 0.25f;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,WalkingAcceleration);
		}
		else
		{
			DesiredSpeed= 0;
			velocity.X = Mathf.MoveToward(velocity.X, DesiredSpeed,WalkingDrag);
			//CurrentPlayerState = PlayerState.Idle;
		}
		if (Input.IsActionJustPressed("ui_accept"))
		{
			velocity.Y += JumpVelocity / 2.0f;
		}
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta * 0.1f;
		}
		return velocity;
	}

	private Vector2I prev_load_position = Vector2I.Zero;

	public override void _PhysicsProcess(double delta)
	{
		if(true)
		{
			float direction3 = Input.GetAxis("left", "right");
			float direction2 = Input.GetAxis("ui_up", "ui_down");
			Position += new Vector2(direction3, direction2) * (float)delta * 400;
			if (sand != null)
			{
				Vector2I cached = (Vector2I)((GlobalPosition - sand.GlobalPosition) / (sand.chunk_size * sand.Scale));
				if (prev_load_position != cached) {
					prev_load_position = cached;
					sand.newPos(cached);
				}
				// GD.Print(sand.load_pos);
			}
			return;
		}
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
		else if(CurrentPlayerState == PlayerState.Water)
		{
			velocity = WaterState(delta, velocity);
		}

		Velocity = velocity;

		if (sand != null)
		{
			Vector2I cached = (Vector2I)((GlobalPosition - sand.GlobalPosition) / (sand.chunk_size * sand.Scale));
			if (prev_load_position != cached) {
				prev_load_position = cached;
				sand.newPos(cached);
			}
			// GD.Print(sand.load_pos);
		}
		MoveAndSlide();
	}
    public override void _Ready()
    {
        collision.BodyEntered += (body) => waterEntered();
        collision.BodyExited += (body) => waterExited();
    }
	private void waterEntered()
	{
		water_deep ++;
		CurrentPlayerState = PlayerState.Water;
	}
	private void waterExited()
	{
		water_deep --;
		if (water_deep == 0)
		{
			if (IsOnFloor())
			{
				CurrentPlayerState = PlayerState.Walking;
			} else
			{
				CurrentPlayerState = PlayerState.Air;
			}
		}
		
	}
}
