using Godot;
using System;
using Godot.Collections;
public partial class BasePhysicsCreature : Node2D
{

	[Export] public NavigationSystem navigationSystem;
	[Export] public BaseCreatureAttricutes baseCreatureAttricutes;
	[Export] public BasePhysicsHead basePhysicsHead;
	[Export] public BaseCritter baseCritter;

	public Vector2 TargetPos = new Vector2(4, 4);

	float UpdatePathTimer = 0;


	public Line2D navLine = new();


	ShaderMaterial BaseColorMaterial;
	Shader BaseColorShader = GD.Load<Shader>("res://assets/creature/shaders/Recolor.gdshader");


	public override void _Ready()
	{
		GetParent().CallDeferred(Node.MethodName.AddChild, navLine);
		navLine.DefaultColor = new Color("00ffff6b");
		navLine.JointMode = Line2D.LineJointMode.Round;
		Array<Vector2I> Path = navigationSystem.GetPath(GlobalPosition / 4, new Vector2I(100, 100));


		baseCritter.SetCurrentCritterNavigationPath(Path);
		InitColors();

	}
	public void InitColors()
	{
		BaseColorMaterial = new ShaderMaterial();
		BaseColorMaterial.Shader = BaseColorShader;

		BaseColorMaterial.SetShaderParameter("currentRow", baseCreatureAttricutes.ColorIndex);
		BaseColorMaterial.SetShaderParameter("palette", baseCreatureAttricutes.ColorPallete);






		foreach (Node node in GetChildren())
		{
			if (node is TextureRect)
			{

				node.Set("material", BaseColorMaterial);
			}
			Godot.Collections.Array<Node> textureChildren = node.FindChildren("*", "TextureRect", true, false);
			foreach (Node textureRect in textureChildren)
			{
				if (textureRect is TextureRect)
				{
					textureRect.Set("material", BaseColorMaterial);
				}
			}
		}

	}
	public override void _Process(double delta)
	{
		UpdatePathTimer += (float)delta;

		if (UpdatePathTimer > 0.5)
		{
			navLine.ClearPoints();
			UpdatePathTimer = 0;
			Array<Vector2I> Path = navigationSystem.GetPath(basePhysicsHead.GlobalPosition / 4, GetGlobalMousePosition() / 4);

			GD.Print(GlobalPosition, GetGlobalMousePosition());
			if (Path.Count != 0)
			{
				baseCritter.SetCurrentCritterNavigationPath(Path);
			}


			Array<Vector2I> tempPath = baseCritter.GetCurrentCritterNavigationPath();
			foreach (Vector2 p in tempPath)
			{
				navLine.AddPoint(p * 4);
			}

		}

		//every 3rd frame
		if (baseCritter.UpdateCritterNavigationPath(basePhysicsHead.GlobalPosition / 4))
		{
			if (navLine.GetPointCount() > 0)
			{
				navLine.RemovePoint(0);
			}
		}

		TargetPos = baseCritter.GetCurrentCritterNavigationPoint(0) * 4;
		if (TargetPos.X == -4)
		{
			TargetPos = basePhysicsHead.GlobalPosition;
		}
		//GD.Print(PathIndex);
		//end every 3rd frame

		// Array<Node2D> seenObjects = baseSensors.SearchWithEyes();
		// if (seenObjects != null)
		// {
		// 	//GD.Print(seenObjects);
		// }


	}
}
