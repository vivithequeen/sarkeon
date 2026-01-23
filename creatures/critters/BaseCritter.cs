using Godot;
using System;
using Godot.Collections;
using ImGuiNET;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;


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


	private Array<Vector2I> _currentCritterNavigationPath;

	/* ok ok this is how the brain should work:


	if it does NOT see anything it wants, no food, no nothin it should continue going in the direction that has the least opsticales
	it should go staright to anything it wants, startng with the most priority (have a taste machanic or smth tasteyer things it wants more)
	*/


	public Node2D GetBestTargetObject(Array<Node2D> SeenObjects)
	{
		Node2D bestObject = SeenObjects[0];
		float bestObjectWeight = (float)SeenObjects[0].Get("weight");

		foreach (Node2D obj in SeenObjects)
		{
			if ((float)obj.Get("weight") > bestObjectWeight)
			{
				bestObject = obj;
				bestObjectWeight = (float)obj.Get("weight");
			}
		}
		return bestObject;
	}

	public void UpdateCritterNavigation(Array<Vector2I> NavPositions)
	{
		_currentCritterNavigationPath = NavPositions;
	}

	public void GetNextCritterNavigationPointIndex(Vector2 position)
	{
		if(_currentCritterNavigationPath.Count == 0)
		{
			return;
		}
		if (position.DistanceTo(_currentCritterNavigationPath[0]) < 1)
		{
			_currentCritterNavigationPath.RemoveAt(0);
		}

	}
	public Vector2I GetCurrentCritterNavigationPoint(int index)
	{
		if(_currentCritterNavigationPath.Count == 0)
		{
			return new Vector2I(-1,-1);
		}
		return _currentCritterNavigationPath[index];
	}


	public override void _Ready()
	{

	}

	public override void _Process(double delta)
	{

		ImGui.Begin($"{GetParent().Name}'s Critter");
		ImGui.End();
	}
}
