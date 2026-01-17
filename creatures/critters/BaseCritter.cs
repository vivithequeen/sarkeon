using Godot;
using System;
using Godot.Collections;
using ImGuiNET;


[GlobalClass]
public partial class BaseCritter : Node
{
	// Called when the node enters the scene tree for the first time.
	public Vector2 GoalPosition;
	public enum CritterGoals {
		Hunting,
		DesperateHunting,
		Tired,
	}

	public Dictionary<string, float> priorities = new Dictionary<string, float>()
	{
		{"hunger" , 0.7f},
	};
	
	/* ok ok this is how the brain should work:


	if it does NOT see anything it wants, no food, no nothin it should continue going in the direction that has the least opsticales
	it should go staright to anything it wants, startng with the most priority (have a taste machanic or smth tasteyer things it wants more)
	*/

	public void UpdateCritterSenses(Array<Dictionary<string, Variant>> SightInformation)
	{
		//should move in the direction of 5 degress that has the least amount of stuff if there is nothing else it wants
		
		float[] weightedSightValues = new float[180/5];

		foreach(Dictionary<string, Variant> OneDegreeInformation in SightInformation)
		{
			float distance = ((Vector2)OneDegreeInformation["globalPosition"]).DistanceTo((Vector2)OneDegreeInformation["collectionPoint"]);
		}
	}
	public override void _Process(double delta)
	{
		ImGui.Begin($"{GetParent().Name}'s Critter");
		ImGui.End();
	}
}
