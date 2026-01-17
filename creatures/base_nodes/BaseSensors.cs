using Godot;
using System;
using System.Security.Principal;
using Godot.Collections;
using System.Collections;
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
	public int eyeIndexTotalSteps = 240; // frames to completion

	RayCast2D eye;
	
	public override void _Ready()
	{

		eye = GetNode<RayCast2D>("eye1");
		StartDegree = (-(baseCreatureAttricutes.FieldOfView-180)/2)-90;

		eyeIndexStep = baseCreatureAttricutes.FieldOfView/eyeIndexTotalSteps;

	}

	Array<Dictionary<string, Variant>> SeenObjects = new Array<Dictionary<string, Variant>>();
	public Array<Dictionary<string, Variant>> SearchWithEyes()
	{
		if(eyeIndex == 0)
		{
			SeenObjects.Clear();
		}

		Dictionary<string, Variant> eyeResult = EyeStep();
		if(eyeResult != null)
		{
			SeenObjects.Add(eyeResult);
		}

		if(eyeIndex >= eyeIndexTotalSteps)
		{
			return SeenObjects;
		}
		
		return null;
	}

	public Dictionary<string, Variant> EyeStep()
	{
		if(eyeIndex >= eyeIndexTotalSteps)
		{
			eyeIndex = 0;
		}

		 		
		eye.Rotation = Mathf.DegToRad(eyeIndexStep * eyeIndex);
		eyeIndex+=1;

		if (eye.IsColliding())
		{
			return new Dictionary<string, Variant> {
				{"collider", eye.GetCollider()}, 
				{"rotation", eyeIndexStep * eyeIndex}, 
				{"collectionPoint", eye.GetCollisionPoint()}, 
				{"globalPosition", GlobalPosition},
			};
		}
		return null;

		
		
	}
}
