using Godot;
using System;

[GlobalClass]
public partial class CreatureBase : Resource
{
	[Export]
	public int CreatureSegments {get; set;} = 10;
	[Export]
	public float ColorIndex {get; set;} = 0.0f;
	[Export]
	public Texture ColorPallete {get; set;}

}
