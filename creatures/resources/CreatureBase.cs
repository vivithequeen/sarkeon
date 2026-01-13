using Godot;
using System;

[GlobalClass]
public partial class CreatureBase : Resource
{
	[Export]
	public int CreatureSegments {get; set;} = 10;


}
