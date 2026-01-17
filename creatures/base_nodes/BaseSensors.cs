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
	public int eyeIndexTotalSteps = 20; // frames to completion

	RayCast2D eye;
	
	public override void _Ready()
	{

		eye = GetNode<RayCast2D>("eye1");
		StartDegree = (-(baseCreatureAttricutes.FieldOfView-180)/2)-90;

		eyeIndexStep = baseCreatureAttricutes.FieldOfView/eyeIndexTotalSteps;

	}

	Array<Array<Variant>> SeenObjects;
	public Array<Array<Variant>> SearchWithEyes()
	{
		if(eyeIndex == 0)
		{
			SeenObjects.Clear();
		}
		
		
		SeenObjects.Add(EyeStep());

		if(eyeIndex >= eyeIndexTotalSteps)
		{
			return SeenObjects;
		}
		
		return null;
	}

	public Array<Variant> EyeStep()
	{
		if(eyeIndex >= eyeIndexTotalSteps)
		{
			eyeIndex = 0;
		}

		 		
		eye.Rotation = Mathf.DegToRad(eyeIndexStep * eyeIndex);
		eyeIndex+=1;

		if (eye.IsColliding())
		{
			return new Array<Variant>{eye.GetCollider(),eyeIndex*eyeIndex};
		}
		return null;

		
		
	}
}
