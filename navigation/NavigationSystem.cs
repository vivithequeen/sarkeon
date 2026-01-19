using Godot;
using System;
using Godot.Collections;
public partial class NavigationSystem : Node
{
	
	[Export] Godot.TileMapLayer Sand;
	[Export] Godot.TileMapLayer Path;

	AStarGrid2D astar = new();
	public override void _Ready()
	{
		InitAstarGrid();
		
	}
    public override void _Process(double delta)
    {
		Vector2 FinalPos = GetParent<Node2D>().GetLocalMousePosition();
        ShowPath(FinalPos);

    }

	public void InitAstarGrid()
	{
		astar.Region = new(0,0,1000,1000);
		astar.CellSize = new(1,1);
		astar.Update();
		foreach(Vector2I Cell in Sand.GetUsedCells())
		{
			astar.SetPointSolid(Cell, true);
		}
	}
	
	public void ShowPath(Vector2 FinalPos)
	{
		Array<Vector2I> PathTaken = astar.GetIdPath(new Vector2I(30,30), (Vector2I)FinalPos);
		Path.Clear();
		foreach(Vector2I Cell in PathTaken)
		{
			Path.SetCell(Cell, 0, Vector2I.Zero);
		}
	}

}
