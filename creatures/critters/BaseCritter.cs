using Godot;
using System;
using Godot.Collections;
using ImGuiNET;
using System.Linq;
using System.Diagnostics;


[GlobalClass]
public partial class BaseCritter : Node
{
	// Called when the node enters the scene tree for the first time.
	public enum CritterGoals
	{
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

	Array<Label> DebugWeightLabels = new();

	public Vector2 UpdateCritterSenses(Array<Dictionary<string, Variant>> SightInformation)
	{
		//should move in the direction of 5 degress that has the least amount of stuff if there is nothing else it wants

		float[] weightedSightValues = new float[(180 / 5) + 1];

		foreach (Dictionary<string, Variant> OneDegreeInformation in SightInformation)
		{
			int index = (int)((float)OneDegreeInformation["rotation"] / 5);
			float distance = ((Node2D)GetParent()).GlobalPosition.DistanceTo((Vector2)OneDegreeInformation["collectionPoint"]);
			float weight = distance / 700.0f;
			weightedSightValues[index] = distance / 700.0f; //TODO: magic number replace at some point with sight range value :pf: 
			UpdateDebugWeight(index, weight, (float)OneDegreeInformation["rotation"]);
		}


		int mostFavorableRotation = GetIndexOfLowestValue(weightedSightValues);


		Vector2 direction = new(100, 0);
		direction = direction.Rotated(Mathf.DegToRad(mostFavorableRotation * 5) + ((Node2D)GetParent()).GlobalRotation);
		direction += ((Node2D)GetParent()).GlobalPosition;
		return direction;
	}

	public void UpdateDebugWeight(int index, float value, float rotation)
	{
		DebugWeightLabels[index].Text = value.ToString();
		Vector2 Pos = new(20,0);
		Pos = Pos.Rotated(Mathf.DegToRad(rotation));
		DebugWeightLabels[index].GlobalPosition = Pos /*+ ((Node2D)GetParent()).GlobalPosition*/;
			
	}

	public void UpdateDebug

	public void InitDebugWeights()
	{
		for(int i = 0; i < 37; i++)
		{
			Label label = new Label();
			DebugWeightLabels.Add(label);
			GetParent().CallDeferred(Node.MethodName.AddChild, label);
		}
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
    public override void _EnterTree()
    {
        InitDebugWeights();
    }

	public override void _Process(double delta)
	{

		ImGui.Begin($"{GetParent().Name}'s Critter");
		ImGui.End();
	}
}
