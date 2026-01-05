using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class TileMapLayer : Godot.TileMapLayer
{
	enum NB_cell_types
	{
		SAND,
		STONE,
		AIR
	}
	private struct NB_cell
	{
		public NB_cell(Vector2I p_position, Vector2 p_velocity, NB_cell_types p_type)
		{
			position = p_position;
			velocity = p_velocity;
			type = p_type;
		}
		public Vector2I position{ get; init; }
		public Vector2 velocity{ get; init; }
		public NB_cell_types type{ get; init; }
	}
	Vector2I size_x = new Vector2I(100,100);
	List<int> moving_id;
	NB_cell[] particles;
	RandomNumberGenerator random_color;
	public override void _Ready()
	{
		//Sets values
		moving_id = new List<int>();
		particles = new NB_cell[size_x.X * size_x.Y];
		random_color = new RandomNumberGenerator();
		random_color.Randomize();

		//Sets all particles
		RandomNumberGenerator random_sand = new RandomNumberGenerator();
		random_sand.Randomize();
		for (int y = 0; y < size_x.Y; y++)
		{
			for (int x = 0; x < size_x.X; x++)
			{
				if (random_sand.RandiRange(0, 5) == 0)
				{
					NB_cell temp_cell = new NB_cell(
					new Vector2I(x,y),
					new Vector2(0,0),
					NB_cell_types.SAND);
					particles[x+y*size_x.X] = temp_cell;
					moving_id.Add(x+y*size_x.X);
				} else
				{
					NB_cell temp_cell = new NB_cell(
					new Vector2I(x,y),
					new Vector2(0,0),
					NB_cell_types.AIR);
					particles[x+y*size_x.X] = temp_cell;
					moving_id.Append(x+y*size_x.X);
				}
			}
		}

		//Sets base stone
		for (int x = 0; x < size_x.X; x++)
		{
			NB_cell temp_cell = new NB_cell(
			new Vector2I(x,50),
			new Vector2(0,0),
			NB_cell_types.STONE);
			particles[x+50*size_x.X] = temp_cell;
		}

		//Updates visuals
		updateParticleVisuals();
	}

	public void updateParticleVisuals()
	{
		int color_var = 0;
		foreach (NB_cell particle in particles)
		{
			//TODO make it render for each type instead matching each time ;-;
			switch (particle.type)
			{
				case NB_cell_types.SAND:
					color_var += random_color.RandiRange(-2,2);
					color_var = Math.Clamp(color_var, 0, 4);
					SetCell(particle.position, 0, new Vector2I(14,color_var));
					break;
				case NB_cell_types.STONE:
					color_var += random_color.RandiRange(-2,2);
					color_var = Math.Clamp(color_var, 0, 5);
					SetCell(particle.position, 0, new Vector2I(color_var + 9,15));
					break;
				case NB_cell_types.AIR:
					SetCell(particle.position, -1);
					break;
			}
		}
		
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	float timer_update = 0;
	float timer_update_max = 1;

	public override void _Process(double delta)
	{
		
	}
}
