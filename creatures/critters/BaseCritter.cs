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
		UpdateDebugWeightLabels();


		float[] weightedSightValues = new float[(180 / 5) + 1];
		// Initialize all directions as "safe" (high weight = far away = no obstacle)
		for(int i = 0; i < weightedSightValues.Length; i++)
		{
			weightedSightValues[i] = float.MaxValue;
		}

		foreach (Dictionary<string, Variant> OneDegreeInformation in SightInformation)
		{
			int index = (int)((float)OneDegreeInformation["rotation"] / 5);
			if(index >= 0 && index < weightedSightValues.Length)
			{
				float distance = ((Node2D)GetParent()).GlobalPosition.DistanceTo((Vector2)OneDegreeInformation["collectionPoint"]);
				float weight = distance / 700.0f;
				weightedSightValues[index] = weight; //TODO: magic number replace at some point with sight range value :pf: 
				UpdateDebugWeightValues(index, weight, (float)OneDegreeInformation["rotation"]);
			}
		}

		// Find direction with HIGHEST weight (furthest from obstacles = safest)
		int mostFavorableIndex = GetIndexOfHighestValue(weightedSightValues);
		float mostFavorableRotation = mostFavorableIndex * 5 + Mathf.RadToDeg(((Node2D)GetParent()).GlobalRotation);
		Vector2 direction = new(200, 0);
		direction = direction.Rotated(Mathf.DegToRad(mostFavorableRotation));
		direction += ((Node2D)GetParent()).GlobalPosition;
		return direction;
	}

	public void UpdateDebugWeightValues(int index, float value, float rotation)
	{
		DebugWeightLabels[index].Text = value.ToString();
	}

	public void UpdateDebugWeightLabels()
	{
		for(int i = 0; i < DebugWeightLabels.Count; i++)
		{
			Vector2 Pos = new(150f,0);
			Pos = Pos.Rotated(Mathf.DegToRad(i * (180.0f / 37)));
			DebugWeightLabels[i].GlobalPosition = Pos + ((Node2D)GetParent()).GlobalPosition;
		}
	}

	public void InitDebugWeights()
	{
		for(int i = 0; i < 37; i++)
		{
			Label label = new Label();
			DebugWeightLabels.Add(label);
			GetParent().CallDeferred(Node.MethodName.AddChild, label);
		}
	}

	public static int GetIndexOfHighestValue(float[] arr)
	{
		if (arr == null || arr.Length == 0)
		{
			throw new ArgumentException("Array cannot be null or empty.", nameof(arr));
		}

		float maxValue = arr[0];
		int maxIndex = 0;

		for (int i = 1; i < arr.Length; i++)
		{
			if (arr[i] > maxValue)
			{
				maxValue = arr[i];
				maxIndex = i;
			}
		}

		return maxIndex;
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
    public override void _Ready()
    {
        InitDebugWeights();
    }

	public override void _Process(double delta)
	{

		ImGui.Begin($"{GetParent().Name}'s Critter");
		ImGui.End();
	}
}
