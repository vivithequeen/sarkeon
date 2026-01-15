using Godot;
using System;
using System.Security.Principal;

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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EyeStep();
	}

	public void EyeStep()
	{
		if(eyeIndex >= eyeIndexTotalSteps)
		{
			eyeIndex = 0;
		}
		GD.Print(eye.Rotation);
		eye.Rotation = Mathf.DegToRad(eyeIndexStep * eyeIndex) + headBase.Rotation;
		eyeIndex+=1;

	}
}
