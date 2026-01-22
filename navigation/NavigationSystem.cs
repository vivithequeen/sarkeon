using Godot;
using System;
using Godot.Collections;
public partial class NavigationSystem : Node
{
	
	[Export] Godot.TileMapLayer Sand;
	
	AStarGrid2D astar = new();
	public override void _Ready()
	{
		InitAstarGrid();
		
	}
    public override void _Process(double delta)
    {

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
	

	public Array<Vector2I> GetPath(Vector2 StartPos, Vector2 FinalPos)
	{
		return astar.GetIdPath((Vector2I)StartPos, (Vector2I)FinalPos);
	}

}
