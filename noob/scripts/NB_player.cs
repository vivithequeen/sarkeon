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
	public override void _Ready()
	{
		sandInit();
	}

	public override void _PhysicsProcess(double delta)
	{
		rootUpdate();
		playerInput();
		playerEnviroment((float)delta, 0.5f);
		sandUpdate();
	}
	private void rootUpdate()
	{
		player_root.GlobalPosition = GlobalPosition;
	}
	private void playerEnviroment(float delta, float precentage)
	{
		if (floor_raycast.IsColliding())
		{
			Vector2 perfect_position = (floor_raycast.GetCollisionPoint() + Vector2.Up * standup_distance - GlobalPosition) * delta * standup_strenght;
			LinearVelocity += perfect_position * precentage;
		}
	} 	private void playerInput()
	{
		Vector2 input = Input.GetVector("left", "right", "up", "down");
		LinearVelocity += new Vector2(input.X, 0) * 32;
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
