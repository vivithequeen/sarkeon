using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class Scarf : Line2D
{
	[Export]
	public int distance = 0;
	[Export]
	public Vector2 start_position = Vector2.Zero;
	[Export]
	public Vector2 gravity = Vector2.Zero;

	[Export]
	public Vector2 wind = Vector2.Zero;

	private Vector2 global_offset = Vector2.Zero;
	private List<Vector2> point_offsets;
	private Vector2 previous_global_position;
	public override void _Ready() 
	{
		global_offset = GlobalPosition;
		point_offsets =
        [
            GetPointPosition(0),
            GetPointPosition(1),
            GetPointPosition(2),
            GetPointPosition(3),
        ];
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 scarfs_velocity = (GlobalPosition - previous_global_position) / GlobalTransform.Scale;
		previous_global_position = GlobalPosition;

		Vector2 curent_position = global_offset - start_position + start_position;
		for (int scarf_node_iter = point_offsets.Count - 1; scarf_node_iter >= 0; scarf_node_iter--)
		{
			curent_position = (point_offsets[scarf_node_iter] - curent_position).Normalized() * distance + curent_position;
			point_offsets[scarf_node_iter] = curent_position - scarfs_velocity + (gravity + wind) * (float)delta;
			SetPointPosition(scarf_node_iter, curent_position);
		}
		
		GlobalRotation = 0;
	}
}
