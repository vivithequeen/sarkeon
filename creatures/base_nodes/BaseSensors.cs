using Godot;
using System;
using System.Security.Principal;
using Godot.Collections;
public partial class BaseSensors : Node2D
{
	[Export]
	BaseCreatureAttricutes baseCreatureAttricutes;

	[Export]
	HeadBase headBase;
	//sight range base ig -105 to -255 so ig 150
	// Called when the node enters the scene tree for the first time.


	public float StartDegree = 0.0f; // in DEGREES. im not fucking with radiants ill just convert them 


	public int eyeIndex = 0;
	public float eyeIndexStep;
	public int eyeIndexTotalSteps = 30; // frames to completion

	Array<RayCast2D> eyes;

	public override void _Ready()
	{

		eyes.Add(GetNode<RayCast2D>("eye1"));
		eyes.Add(GetNode<RayCast2D>("eye2"));
		//eyes.Add(GetNode<RayCast2D>("eye3"));
		//eyes.Add(GetNode<RayCast2D>("eye4"));
		StartDegree = (-(baseCreatureAttricutes.FieldOfView - 180) / 2) - 90;

		eyeIndexStep = baseCreatureAttricutes.FieldOfView / eyeIndexTotalSteps;

	}
	Array<Node2D> SeenObjects = new Array<Node2D>();
	public Array<Node2D> SearchWithEyes()
	{



		if (eyeIndex == 0)
		{
			SeenObjects.Clear();
		}
		int index = 0;
		foreach (RayCast2D eye in eyes)
		{
			float angle = Mathf.DegToRad()
			Node2D eyeResult;
			if (eyeResult != null)
			{
				SeenObjects.Add(eyeResult);
			}
			index++;
		}


		if (eyeIndex >= eyeIndexTotalSteps)
		{
			return SeenObjects;
		}

		return null;
	}

	public Node2D EyeResult(RayCast2D eye, float angle)
	{
		eye.Rotation = angle;


		if (eye.IsColliding())
		{
			return (Node2D)eye.GetCollider();
		}
		return null;
	}
}
