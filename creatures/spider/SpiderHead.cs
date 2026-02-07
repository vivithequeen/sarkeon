using Godot;
using System;

public partial class SpiderHead : HeadBase
{

	

	public override void _Ready()
	{
		tailBase = GD.Load<PackedScene>("res://creatures/spider/SpiderTail.tscn");
		InitBodyAndColors();
	}
}
