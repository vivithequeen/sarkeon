using Godot;
using System;
using Godot.Collections;
using ImGuiNET;
using System.Linq;


[GlobalClass]
public partial class BaseCritter : Node
{
	// Called when the node enters the scene tree for the first time.
	[Export] public Node2D GoalPositionBase;	
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
		
		float[] weightedSightValues = new float[(180/5) + 1];

		foreach(Dictionary<string, Variant> OneDegreeInformation in SightInformation)
		{
			float distance = ((Node2D)GetParent()).GlobalPosition.DistanceTo((Vector2)OneDegreeInformation["collectionPoint"]);
			weightedSightValues[(int)((float)OneDegreeInformation["rotation"] / 5)] =  distance / 700.0f; //TODO: magic number replace at some point with sight range value :pf: 
			
		}


		int mostFavorableRotation = GetIndexOfLowestValue(weightedSightValues);


		GoalPositionBase.Rotation = Mathf.DegToRad(mostFavorableRotation);
	}


	public static int GetIndexOfLowestValue(float[] arr)
{
    if (arr == null || arr.Length == 0)
    {
        throw new ArgumentException("Array cannot be null or empty.", nameof(arr));
    }

    float minValue = arr[0];
    int minIndex = 0;

    for (int i = 1; i < arr.Length; i++)
    {
        if (arr[i] < minValue)
        {
            minValue = arr[i];
            minIndex = i;
        }
    }

    return minIndex;
}

	public override void _Process(double delta)
	{
		ImGui.Begin($"{GetParent().Name}'s Critter");
		ImGui.End();
	}
}
