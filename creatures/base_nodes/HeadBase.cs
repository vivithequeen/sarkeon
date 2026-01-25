using Godot;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
public partial class HeadBase : Node2D
{

	System.Collections.Generic.List<BodySegmentBase> BodySegments = new List<BodySegmentBase>();


	[Export] public Node2D GoalPositionBase;
	[Export]

	BaseCreatureAttricutes baseCreatureAttricutes;
	int AmountOfSegments = 0;
	Vector2 TargetPosition;
	PackedScene bodySegmentBase = GD.Load<PackedScene>("res://creatures/base_nodes/BodySegmentBase.tscn");
	PackedScene tailBase = GD.Load<PackedScene>("res://creatures/base_nodes/TailBase.tscn");
	// Called when the node enters the scene tree for the first time.



	ShaderMaterial BaseColorMaterial;
	Shader BaseColorShader = GD.Load<Shader>("res://assets/creature/shaders/Recolor.gdshader");
	public override void _Ready()
	{
		
		BaseColorMaterial = new ShaderMaterial();
		BaseColorMaterial.Shader = BaseColorShader;

		BaseColorMaterial.SetShaderParameter("currentRow", baseCreatureAttricutes.ColorIndex);
		BaseColorMaterial.SetShaderParameter("palette", baseCreatureAttricutes.ColorPallete);





		AmountOfSegments = baseCreatureAttricutes.CreatureSegments;
		Vector2 CurrentPosition = GlobalPosition;
		Node parent = GetParent().GetParent();
		for(int i = 0; i < AmountOfSegments-1; i++)
		{
			BodySegmentBase NewSegment = bodySegmentBase.Instantiate<BodySegmentBase>();
			
			
			parent.CallDeferred(Node.MethodName.AddChild, NewSegment);


			BodySegments.Add(NewSegment);
			
			NewSegment.InitSegment(CurrentPosition);
			CurrentPosition.X-=32;
		}
		
		BodySegmentBase NewTail = tailBase.Instantiate<BodySegmentBase>();
		
		
		parent.CallDeferred(Node.MethodName.AddChild, NewTail);
		BodySegments.Add(NewTail);

		NewTail.InitSegment(CurrentPosition);


		
		foreach(Node node in GetChildren()){
			if(node is TextureRect)
			{
				
				node.Set("material", BaseColorMaterial);
			}
		}
		foreach(Node node in BodySegments){
			Godot.Collections.Array<Node> textureChildren = node.FindChildren("*","TextureRect",true,false);
			foreach(Node textureRect in textureChildren){
				if(textureRect is TextureRect)
				{
					textureRect.Set("material", BaseColorMaterial);
				}
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		TargetPosition = ((BaseCreature)GetParent()).TargetPos;
		
		//TargetPosition = GetGlobalMousePosition();
		GoalPositionBase.GlobalPosition = ((BaseCreature)GetParent()).TargetPos;
		//TODO: CHANGE CHANGE CHANGE CHANGE CHANGE CHANGE CHANGE
		GetParent<Node2D>().GlobalPosition = GetParent<Node2D>().GlobalPosition.MoveToward(TargetPosition, 300 * (float)delta);

		float AngleToTarget = (TargetPosition - GlobalPosition).Normalized().Angle();
		GetParent<Node2D>().GlobalRotation = Mathf.LerpAngle(GetParent<Node2D>().GlobalRotation, AngleToTarget, (float)delta); //TODO: make smooth 
		for(int i = 0; i < AmountOfSegments; i++)
		{
			if(i == 0)
			{
				BodySegments[i].FollowTarget(this.GlobalPosition, delta);
			}
			else
			{
				BodySegments[i].FollowTarget(BodySegments[i-1].GlobalPosition, delta);
			}
		}
	}
}
