using Godot;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
public partial class HeadBase : Node2D
{

	System.Collections.Generic.List<BodySegmentBase> BodySegments = new List<BodySegmentBase>();

	int AmountOfSegments = 10;
	Vector2 TargetPosition;
	PackedScene BodySegmentBase = GD.Load<PackedScene>("res://creatures/base_nodes/BodySegmentBase.tscn");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Vector2 CurrentPosition = GlobalPosition;

		for(int i = 0; i < AmountOfSegments; i++)
		{
			BodySegmentBase NewSegment = BodySegmentBase.Instantiate<BodySegmentBase>();
			Node parent = GetParent();
			if (parent != null) {
				parent.CallDeferred(Node.MethodName.AddChild, NewSegment);
			} else {
				GetTree().Root.AddChild(NewSegment);
			}

			BodySegments.Add(NewSegment);
			
			NewSegment.InitSegment(CurrentPosition);
			CurrentPosition.X-=32;
		}
		GD.Print(BodySegments.Count);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		TargetPosition = GetGlobalMousePosition();
		GlobalPosition = GlobalPosition.MoveToward(TargetPosition, 300 * (float)delta);
		LookAt(TargetPosition);
		for(int i = 0; i < AmountOfSegments; i++)
		{
			if(i == 0)
			{
				BodySegments[i].FollowTarget(this.GlobalPosition, delta);
			}
			else
			{
				BodySegments[i].FollowTarget(BodySegments[i-1].GlobalPosition, delta);
			}
		}
	}
}
