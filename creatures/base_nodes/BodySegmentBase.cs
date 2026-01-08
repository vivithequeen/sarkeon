using Godot;
using System;

public partial class BodySegmentBase : Skeleton2D
{
	int Seconds = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalPosition = GetGlobalMousePosition();
		if(Seconds % 2 == 0)
		{
			foreach(BaseLeg leg in GetNode<Node2D>("EvenLegs").GetChildren())
			{
				leg.Reset();
			}
		}
		else
		{
			foreach(BaseLeg leg in GetNode<Node2D>("OddLegs").GetChildren())
			{
				leg.Reset();
			}
		}
	}
	public void _on_timer_timeout()
	{
		Seconds++; 
	}
}
