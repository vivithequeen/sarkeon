using Godot;
using System;

public partial class NB_fps : Label
{
	double last_fps = 0;
	public override void _Process(double delta)
	{
		last_fps = (last_fps + delta) / 10;
		Text = Math.Round(0.1/last_fps, 2).ToString();
	}
}
