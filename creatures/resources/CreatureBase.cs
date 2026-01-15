using Godot;
using System;

[GlobalClass]
public partial class CreatureBase : Node
{
	[Export]
	public int CreatureSegments {get; set;} = 10;
	[Export(PropertyHint.Range, "0.0,1.0,0.05")]
	public float ColorIndex {get; set;} = 0.0f;
	[Export]
	public Texture ColorPallete {get; set;}

}
