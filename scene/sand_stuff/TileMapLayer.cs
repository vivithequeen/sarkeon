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
		AIR,
		VOID
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
		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
	//TODO make so it only rerenders updated pixels for optimization
	// List<int> moving_id;
		// Use this for initing ;-;
		// moving_id = new List<int>();
	Vector2I particle_sim_size = new Vector2I(500,500);
	int particle_sim_abs_size;
	NB_cell[] particles;
	bool[] particles_buffer;
	List<int> particles_buffer_updated_index;
	List<int> particles_marked_solid;
	RandomNumberGenerator random_color;
	NB_cell NB_CELL_VOID = new NB_cell(Vector2.Zero, NB_cell_types.VOID);
	public override void _Ready()
	{
		//Sets values
		particle_sim_abs_size = particle_sim_size.X * particle_sim_size.Y;
		particles_buffer_updated_index = new List<int>();
		particles_marked_solid = new List<int>();
		random_color = new RandomNumberGenerator();
		random_color.Randomize();
		
		particles = new NB_cell[particle_sim_abs_size];
		particles_buffer = new bool[particle_sim_abs_size];
		// inits both arrays
		for (int i = 0; i < particle_sim_abs_size; i++)
		{
			NB_cell temp_cell = new NB_cell(
			new Vector2(0,0),
			NB_cell_types.AIR);
			
			particles[i] = (NB_cell)temp_cell.Clone();
			particles_buffer[i] = false;
		}
		//Sets all particles
		RandomNumberGenerator random_sand = new RandomNumberGenerator();
		random_sand.Randomize();
		for (int y = 0; y < particle_sim_size.Y; y++)
		{
			for (int x = 0; x < particle_sim_size.X; x++)
			{
				if (random_sand.RandiRange(0, 10) == 0)
				{
					NB_cell temp_cell = new NB_cell(
					new Vector2(0,0),
					NB_cell_types.SAND);
					particles[x+y*particle_sim_size.X] = temp_cell;
					particles_buffer_updated_index.Add(x+y*particle_sim_size.X);
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
			particles_buffer_updated_index.Add(x+(particle_sim_size.Y-1)*particle_sim_size.X);
		}

		//Updates visuals
		updateParticleVisuals();
	}

	public void updateParticleVisuals()
	{
		int color_var = 0;
		foreach (var index_of_cell in particles_buffer_updated_index)
		{
			Vector2I position = idToPosition(index_of_cell);
			NB_cell particle = particles[index_of_cell];
			particles_buffer[index_of_cell] = false;
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
		// foreach (var index_of_cell in particles_marked_solid)
		// {
		// 	Vector2I position = idToPosition(index_of_cell);
		// 	NB_cell particle = particles[index_of_cell];
		// 	particles_buffer[index_of_cell] = false;
		// 	switch (particle.type)
		// 	{
		// 		case NB_cell_types.SAND:
		// 			color_var += random_color.RandiRange(-2,2);
		// 			color_var = Math.Clamp(color_var, 0, 4);
		// 			SetCell(position, 1, new Vector2I(14,color_var));
		// 			break;
		// 		case NB_cell_types.STONE:
		// 			color_var += random_color.RandiRange(-2,2);
		// 			color_var = Math.Clamp(color_var, 0, 5);
		// 			SetCell(position, 1, new Vector2I(color_var + 9,15));
		// 			break;
		// 		case NB_cell_types.AIR:
		// 			SetCell(position, -1);
		// 			break;
		// 	}
		// }
		particles_buffer_updated_index.Clear();
	}
	private void particlesUpdateVisual(int p_layer)
	{
		
	}
	private Vector2I idToPosition(int id)
	{
		return new Vector2I(
			id % particle_sim_size.X,
			id / particle_sim_size.X
		);
	}
	//Used to eliminate race condition
	public void updateParticleMovings()
	{
		for (int i = 0; i < particle_sim_abs_size; i++)
		{
			int index = i;
			NB_cell particle = particles[index];
			if (particles_buffer[index]) {continue;}
			switch (particle.type)
			{
				case NB_cell_types.SAND:
					//check bottom
					int exchange_cell = index+particle_sim_size.X;
					NB_cell end_cell = particlesCheck(exchange_cell);
					if (end_cell.type == NB_cell_types.AIR)
					{
						particlesSwap(index, exchange_cell);
						particleLockNUpdateDoubl(index, exchange_cell);
						break;
					} 
					exchange_cell = index+particle_sim_size.X + (odd_update? 1: -1);
					end_cell = particlesCheck(exchange_cell);
					if (end_cell.type == NB_cell_types.AIR)
					{
						particlesSwap(index, exchange_cell);
						particleLockNUpdateDoubl(index, exchange_cell);
						break;
					} 
					exchange_cell = index+particle_sim_size.X + (odd_update? -1: 1);
					end_cell = particlesCheck(exchange_cell);
					if (end_cell.type == NB_cell_types.AIR)
					{
						particlesSwap(index, exchange_cell);
						particleLockNUpdateDoubl(index, exchange_cell);
						break;
					} 
					particles_marked_solid.Add(index);
					break;
				case NB_cell_types.STONE:
					break;
				case NB_cell_types.AIR:
					break;
			}
		}
	}
	private NB_cell particlesCheck(int p_index)
	{
		if (p_index >= particle_sim_abs_size){return NB_CELL_VOID;}
		if (particles_buffer[p_index]) {return NB_CELL_VOID;}
		return particles[p_index];
	}
	private void particlesSwap(int p_index_1, int p_index_2)
	{
		NB_cell cell_temp = particles[p_index_1];
		particles[p_index_1] = particles[p_index_2];
		particles[p_index_2] = cell_temp;
	}
	private void particleLockNUpdateDoubl(int p_index_1, int p_index_2)
	{
		particleLockNUpdate(p_index_1);
		particleLockNUpdate(p_index_2);
	}
	private void particleLockNUpdate(int p_index_1)
	{
		particles_buffer_updated_index.Add(p_index_1);
		particles_buffer[p_index_1] = true;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	double timer_update = 0;
	double timer_update_max = 0.01;
	bool odd_update = false;

	public override void _Process(double delta)
	{
		timer_update += delta;
		if (timer_update > timer_update_max)
		{
			odd_update = !odd_update;
			updateParticleMovings();
			updateParticleVisuals();
			timer_update = 0;
		}
	}
}
