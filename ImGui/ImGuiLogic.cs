using Godot;
using System;
using ImGuiNET;
using System.Xml;
[GlobalClass]
public partial class ImGuiLogic : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ImGui.BeginMainMenuBar();
		String fps = "FPS:" + Engine.GetFramesPerSecond();
		ImGui.Text(fps);
		ImGui.EndMainMenuBar();
	}
}
