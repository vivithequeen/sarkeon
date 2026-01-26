using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public partial class NavigationSystem : Node
{

	[Export] Godot.TileMapLayer Sand;

	AStarGrid2D astar = new();
	public override void _Ready()
	{
		InitAstarGrid(20, 400, 400);

	}
	public override void _Process(double delta)
	{

	}
	static readonly Vector2I[] Directions =
	{
		Vector2I.Up,
		Vector2I.Down,
		Vector2I.Left,
		Vector2I.Right,
		Vector2I.Up + Vector2I.Left,
		Vector2I.Up + Vector2I.Right,
		Vector2I.Down + Vector2I.Left,
		Vector2I.Down + Vector2I.Right,
	};


	public void InitAstarGrid(int n, int x, int y) // this is d1 vibecoded
	{
		astar.Region = new Rect2I(0, 0, x, y);
		astar.CellSize = new Vector2I(1, 1);
		astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.AtLeastOneWalkable;
		astar.Update();

		var visited = new Godot.Collections.Dictionary<Vector2I, int>();
		var queue = new Queue<Vector2I>();

		// 1. Mark solids + seed BFS
		foreach (Vector2I cell in Sand.GetUsedCells())
		{
			astar.SetPointSolid(cell, true);

			foreach (Vector2I dir in Directions)
			{
				Vector2I neighbor = cell + dir;
				if (!astar.IsInBounds(neighbor.X, neighbor.Y) || visited.ContainsKey(neighbor))
					continue;

				visited[neighbor] = 1;
				queue.Enqueue(neighbor);
			}
		}

		while (queue.Count > 0)
		{
			Vector2I cell = queue.Dequeue();
			int dist = visited[cell];

			if (dist > n || astar.IsPointSolid(cell))
				continue;

			float weight = Math.Max(1f, x - (dist - 1) * y);
			astar.SetPointWeightScale(cell, weight);

			if (dist == n)
				continue;

			foreach (Vector2I dir in Directions)
			{
				Vector2I next = cell + dir;
				if (!astar.IsInBounds(next.X, next.Y) || visited.ContainsKey(next))
					continue;

				visited[next] = dist + 1;
				queue.Enqueue(next);
			}
		}
	}



	public Array<Vector2I> GetPath(Vector2 StartPos, Vector2 FinalPos)
	{
		return astar.GetIdPath((Vector2I)StartPos, (Vector2I)FinalPos);
	}

}
