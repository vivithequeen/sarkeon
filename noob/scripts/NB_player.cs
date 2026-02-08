using Godot;
using System;

public partial class NB_player : RigidBody2D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	Sand sand;
	public Vector2I prev_position = Vector2I.Zero;
	[Export]
	public Vector2 gravity_vector = Vector2.Down;
	[Export]
	public RayCast2D floor_raycast;
	[Export]
	public Node2D player_root;
	[Export]
	public float standup_distance = 20f;
	[Export]
	public float standup_strenght = 500f;
	[Export]
	public float crouch_distance = 20f;
	[Export]
	public float jump_distance = 100f;
	[Export]
	public float movement_speed = 100f;
	private bool movement_crouching = false;
	private bool movement_jumping = false;
	private float controll_multiplier = 1f;
	private Vector2 added_force;
	[Export]
	public Vector2 movement_clamp = Vector2.One;
	public override void _Ready()
	{
		sandInit();
	}

	public override void _PhysicsProcess(double delta)
	{
		added_force = Vector2.Zero;
		if (floor_raycast.GetCollisionPoint().DistanceTo(GlobalPosition) < 30)
		{
			controll_multiplier = 1f;
		} else if (floor_raycast.GetCollisionPoint().DistanceTo(GlobalPosition) < 40)
		{
			controll_multiplier = 0.5f;
		} else
		{
			controll_multiplier = 0f;
		}
		GravityScale = 1 - controll_multiplier;
		LinearDamp = controll_multiplier * 10;
		rootUpdate();
		playerInput((float) delta);
		playerEnviroment((float)delta);
		sandUpdate();
		LinearVelocity += added_force.Clamp(-movement_clamp, movement_clamp);
	}
	private void rootUpdate()
	{
		player_root.GlobalPosition = GlobalPosition;
	}
	private void playerEnviroment(float delta)
	{
		if (floor_raycast.IsColliding())
		{
			Vector2 perfect_position;
			if (movement_jumping)
			{
				perfect_position = (floor_raycast.GetCollisionPoint() + Vector2.Up * jump_distance - GlobalPosition) * delta * standup_strenght;
			} else
			{
				perfect_position = (floor_raycast.GetCollisionPoint() + Vector2.Up * 
					(movement_crouching ? crouch_distance : standup_distance)
					- GlobalPosition) * delta * standup_strenght;
			}
			added_force += perfect_position * controll_multiplier;
		}
	} 	
	private void playerInput(float delta)
	{
		Vector2 input = Input.GetVector("left", "right", "up", "down");
		added_force += new Vector2(input.X, 0) * delta * movement_speed * controll_multiplier;
		movement_crouching = input.Y > 0;
		movement_jumping = input.Y < 0;
	}
	private void sandInit()
	{
		Vector2I next_position = (Vector2I)((GlobalPosition - sand.GlobalPosition) / (sand.chunk_size * sand.Scale));
		prev_position = next_position;
		sand.load_pos = next_position;
		sand.newPos(next_position);
	}
	private void sandUpdate()
	{
		if (sand != null)
		{
			Vector2I next_position = (Vector2I)((GlobalPosition - sand.GlobalPosition) / (sand.chunk_size * sand.Scale));
			if (prev_position != next_position)
			{
				prev_position = next_position;
				sand.load_pos = next_position;
				sand.newPos(next_position);
			}
		}
	}
}
