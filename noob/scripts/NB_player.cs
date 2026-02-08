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
	[Export]
	public Skeleton2D skeleton;
	//! LEFT LEG
	[Export]
	public Node2D left_leg_ik_node;
	[Export]
	public Bone2D left_leg_start_ik_bone;
	[Export]
	public Bone2D left_leg_end_ik_bone;
	[Export]
	public RemoteTransform2D left_leg_ik_sticker;
	[Export]
	public RayCast2D left_leg_raycast;
	[Export]
	public RayCast2D left_leg_footing_raycast;
	[Export]
	public RayCast2D left_leg_footing_middle_raycast;
	private float left_leg_timer = 0;
	private float left_leg_ik_lenght = 0;
	private Vector2 left_leg_prev_position = Vector2.Zero;
	private Vector2 left_leg_goal_position = Vector2.Zero;
	private bool left_ik_debounce = true;
	//! RIGHT LEG
	[Export]
	public Godot.Collections.Array<Bone2D> right_leg;
	[Export]
	public RayCast2D right_leg_raycast;
	private float right_leg_timer = 0;
	private Vector2 right_leg_prev_position = Vector2.Zero;
	public override void _Ready()
	{
		sandInit();
		left_leg_ik_lenght = (left_leg_start_ik_bone.GlobalPosition - left_leg_end_ik_bone.GlobalPosition).LengthSquared();
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
		moveLegs((float) delta);
		playerInput((float) delta);
		playerEnviroment((float)delta);
		sandUpdate();
		LinearVelocity += added_force.Clamp(-movement_clamp, movement_clamp);
	}
	private void moveLegs(float delta)
	{
		if (left_leg_raycast.IsColliding())
		{
			if(left_ik_debounce)
			{
				left_ik_debounce = false;
				getLeftPosition();
				left_leg_prev_position += left_leg_goal_position - GlobalPosition - left_leg_prev_position;
				left_leg_ik_sticker.UpdatePosition = false;
			}
			left_leg_prev_position += (left_leg_goal_position - GlobalPosition - left_leg_prev_position) * delta * 20;
			if ((left_leg_goal_position - left_leg_start_ik_bone.GlobalPosition).LengthSquared() > left_leg_ik_lenght || left_leg_timer <= 0)
			{
				getLeftPosition();
			} else {
				left_leg_timer -= delta;
			}
			left_leg_ik_node.GlobalPosition = left_leg_prev_position + GlobalPosition;
		} else
		{
			if(!left_ik_debounce)
			{
				left_ik_debounce = true;
				left_leg_ik_sticker.GlobalPosition = left_leg_ik_node.GlobalPosition;
				left_leg_ik_sticker.UpdatePosition = true;

			}
		}
	}
	private void getLeftPosition()
	{
		if (LinearVelocity.LengthSquared() > 100)
		{
			left_leg_footing_raycast.TargetPosition = LinearVelocity.Normalized() * 30;
			if (left_leg_footing_raycast.IsColliding())
			{
				left_leg_goal_position = left_leg_footing_raycast.GetCollisionPoint();
			} else
			{
				left_leg_footing_middle_raycast.TargetPosition = (left_leg_raycast.TargetPosition + left_leg_footing_raycast.TargetPosition).Normalized() * 30;
				if (left_leg_footing_middle_raycast.IsColliding())
				{
					left_leg_goal_position = left_leg_footing_middle_raycast.GetCollisionPoint();
				} else
				{
					left_leg_goal_position = left_leg_raycast.GetCollisionPoint();
				}
				//! make this a function and make it called from debounce one and add debounce in general
			}
		} else
		{
			left_leg_goal_position = left_leg_raycast.GetCollisionPoint();
		}
		left_leg_timer = 1f;
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
	private bool ik(Bone2D b_1, Bone2D b_2, Bone2D b_3, Vector2 goal)
	{
		float b_1_size = b_2.Position.Length();
		if (b_1_size + b_3.Position.Length() < (goal - b_1.GlobalPosition).Length())
		{
			b_1.LookAt(goal);
			b_1.Rotation -= (float)Math.PI/2f;
			b_2.LookAt(goal);			
			b_2.Rotation -= (float)Math.PI/2f;
			return true;
		} else
		{
			b_2.LookAt(goal);
			b_2.Rotation -= (float)Math.PI/2f;
			Vector2 bone_look = b_3.GlobalPosition - b_2.GlobalPosition;
			Vector2 goal_look = goal - b_3.GlobalPosition;
			float error = b_3.GlobalPosition.DistanceTo(goal) * (bone_look.Dot(goal_look) > 0? 1 : -1);
			b_1.Rotation += error / b_1_size;
			return false;
		}
	}
}