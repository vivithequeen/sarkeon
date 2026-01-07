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
	private class NB_cell
	{
		public NB_cell(Vector2 p_velocity, NB_cell_types p_type)
		{
			velocity = p_velocity;
			type = p_type;
		}
		public Vector2 velocity;
		public NB_cell_types type;
	}
	//TODO make so it only rerenders updated pixels for optimization
	// List<int> moving_id;
		// Use this for initing ;-;
		// moving_id = new List<int>();
	//!X MUST NOT DIVIDE BY 2 SO IT CHECKS IN CHECKER PATTERN
	Vector2I particle_sim_size = new Vector2I(201,100);
	int particle_sim_abs_size;
	NB_cell[] particles;
	NB_cell[] particles_buffer;
	List<int> particles_buffer_updated_index;
	RandomNumberGenerator random_color;
	public override void _Ready()
	{
		//Sets values
		particle_sim_abs_size = particle_sim_size.X * particle_sim_size.Y;
		particles = new NB_cell[particle_sim_abs_size];
		particles_buffer = new NB_cell[particle_sim_abs_size];
		random_color = new RandomNumberGenerator();
		random_color.Randomize();

		//Sets all particles
		RandomNumberGenerator random_sand = new RandomNumberGenerator();
		random_sand.Randomize();
		for (int y = 0; y < particle_sim_size.Y; y++)
		{
			for (int x = 0; x < particle_sim_size.X; x++)
			{
				if (random_sand.RandiRange(0, 5) == 0)
				{
					NB_cell temp_cell = new NB_cell(
					new Vector2(0,0),
					NB_cell_types.SAND);
					particles[x+y*particle_sim_size.X] = temp_cell;
				} else
				{
					NB_cell temp_cell = new NB_cell(
					new Vector2(0,0),
					NB_cell_types.AIR);
					particles[x+y*particle_sim_size.X] = temp_cell;
				}
			}
		}

		//Sets base stone
		for (int x = 0; x < particle_sim_size.X; x++)
		{
			NB_cell temp_cell = new NB_cell(
			new Vector2(0,0),
			NB_cell_types.STONE);
			particles[x+(particle_sim_size.Y-1)*particle_sim_size.X] = temp_cell;
		}


		//Updates visuals
		updateParticleVisuals();
	}

	public void updateParticleVisuals()
	{
		int color_var = 0;
		for (int i = 0; i < particle_sim_abs_size; i++)
		{
			
			Vector2I position = idToPosition(i);
			NB_cell particle = particles[i];
			//TODO make it render for each type instead matching each time ;-;
			switch (particle.type)
			{
				case NB_cell_types.SAND:
					color_var += random_color.RandiRange(-2,2);
					color_var = Math.Clamp(color_var, 0, 4);
					SetCell(position, 0, new Vector2I(14,color_var));
					break;
				case NB_cell_types.STONE:
					color_var += random_color.RandiRange(-2,2);
					color_var = Math.Clamp(color_var, 0, 5);
					SetCell(position, 0, new Vector2I(color_var + 9,15));
					break;
				case NB_cell_types.AIR:
					SetCell(position, -1);
					break;
			}
		}
	}
	private Vector2I idToPosition(int id)
	{
		return new Vector2I(
			id % particle_sim_size.X,
			id / particle_sim_size.X
		);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	double timer_update = 0;
	double timer_update_max = 0.1;
	//Used to eliminate race condition
	public void updateParticleMovings()
	{
		for (int i = 0; i < particle_sim_abs_size; i++)
		{
			int index = i;
			NB_cell particle = particles[index];
			switch (particle.type)
			{
				case NB_cell_types.SAND:
					if (index+particle_sim_size.X >= particle_sim_abs_size){break;}
					if (particles_buffer[index+particle_sim_size.X].type == NB_cell_types.AIR)
					{
						particles_buffer[index] = new NB_cell(
						new Vector2(0,0),
						NB_cell_types.AIR);
						particles_buffer[index+particle_sim_size.X] = particle;
						particles_buffer_updated_index.Add(index);
						particles_buffer_updated_index.Add(index+particle_sim_size.X);
					} else
					{
						if (particles_buffer[index+particle_sim_size.X - 1].type == NB_cell_types.AIR)
						{
							particles_buffer[index] = new NB_cell(
							new Vector2(0,0),
							NB_cell_types.AIR);
							particles_buffer[index+particle_sim_size.X - 1] = particle;
							particles_buffer_updated_index.Add(index);
							particles_buffer_updated_index.Add(index+particle_sim_size.X - 1);
						}
					}
					break;
				case NB_cell_types.STONE:
					break;
				case NB_cell_types.AIR:
					break;
			}
		}
	}

	public override void _Process(double delta)
	{
		timer_update += delta;
		if (timer_update > timer_update_max)
		{
			updateParticleMovings();
			updateParticleVisuals();
			timer_update = 0;
		}
	}
}
