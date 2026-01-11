using Godot;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
public partial class HeadBase : Node2D
{

	System.Collections.Generic.List<BodySegmentBase> BodySegments = new List<BodySegmentBase>();

	int AmountOfSegments = 10;

	PackedScene BodySegmentBase = GD.Load<PackedScene>("res://creatures/base_nodes/BodySegmentBase.tscn");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Vector2 CurrentPosition = Position;

		for(int i = 0; i < AmountOfSegments; i++)
		{
			BodySegmentBase NewSegment = BodySegmentBase.Instantiate<BodySegmentBase>();
			AddChild(NewSegment);

			BodySegments.Add(NewSegment);
			
			NewSegment.InitSegment(CurrentPosition);
			CurrentPosition.X-=32;
		}
		GD.Print(BodySegments.Count);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition = GetGlobalMousePosition();
		for(int i = 0; i < AmountOfSegments-1; i++)
		{
			if(i == 0)
			{
				BodySegments[i].FollowTarget(this.Position, delta);
			}
			else
			{
				BodySegments[i].FollowTarget(BodySegments[i-1].Position, delta);
			}
		}
	}
}
