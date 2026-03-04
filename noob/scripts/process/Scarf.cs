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

	private Vector2 global_offset = Vector2.Zero;
	private List<Vector2> point_offsets;
	public override void _Ready() 
	{
		global_offset = GlobalPosition / GlobalTransform.Scale;
		point_offsets =
        [
            GetPointPosition(3) / GlobalTransform.Scale,
            GetPointPosition(2) / GlobalTransform.Scale,
            GetPointPosition(1) / GlobalTransform.Scale,
            GetPointPosition(0) / GlobalTransform.Scale,
        ];
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 true_global_scale = GlobalPosition / GlobalTransform.Scale;
		Vector2 curent_position = true_global_scale - start_position;
		for (int scarf_node_iter = 0; scarf_node_iter < point_offsets.Count; scarf_node_iter++)
		{
			Vector2 scarf_node_position = point_offsets[scarf_node_iter];
			scarf_node_position = curent_position + ((scarf_node_position - curent_position).Normalized() * distance);
			curent_position = scarf_node_position;
			SetPointPosition(scarf_node_iter, scarf_node_position - true_global_scale - start_position);
		}
	}
}
