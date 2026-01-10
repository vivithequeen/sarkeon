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
			vel_start = 0;
			velocity = p_velocity;
			type = p_type;
		}
		public int vel_start;
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
	Vector2I particle_sim_size = new Vector2I(200,500);
	int particle_sim_abs_size;
	NB_cell[] particles;
	bool[] particles_locked_cell;
	List<int> particles_buffer_updated_index;
	RandomNumberGenerator random_color;
	bool[] particles_selection_row;
	NB_cell NB_CELL_VOID = new NB_cell(Vector2.Zero, NB_cell_types.VOID);
	int color_var = 0;
	float fall_mul = 1f;
	public override void _Ready()
	{
		//Sets values
		particle_sim_abs_size = particle_sim_size.X * particle_sim_size.Y;
		particles_buffer_updated_index = new List<int>();
		particles_selection_row = new bool[particle_sim_size.Y];
		random_color = new RandomNumberGenerator();
		random_color.Randomize();
		
		particles = new NB_cell[particle_sim_abs_size];
		particles_locked_cell = new bool[particle_sim_abs_size];
		// inits both arrays
		for (int i = 0; i < particle_sim_abs_size; i++)
		{
			NB_cell temp_cell = new NB_cell(
			new Vector2(0,0),
			NB_cell_types.AIR);
			
			particles[i] = (NB_cell)temp_cell.Clone();
			particles_locked_cell[i] = false;
		}
		//Sets all particles
		RandomNumberGenerator random_sand = new RandomNumberGenerator();
		random_sand.Randomize();
		for (int y = 0; y < particle_sim_size.Y; y++)
		{
			for (int x = 0; x < particle_sim_size.X; x++)
			{
				if (random_sand.RandiRange(0, 1000) == 0)
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
		foreach (var index_of_cell in particles_buffer_updated_index)
		{
			particles_locked_cell[index_of_cell] = false;
			particlesUpdateVisual(index_of_cell, 1);
		}
		
		particles_buffer_updated_index.Clear();
	}
	private void particlesUpdateVisual(int p_index, int p_layer)
	{
		Vector2I position = idToPosition(p_index);
		NB_cell particle = particles[p_index];
		switch (particle.type)
		{
			case NB_cell_types.SAND:
				color_var += random_color.RandiRange(-1,1);
				color_var = Math.Clamp(color_var, 0, 4);
				SetCell(position, p_layer, new Vector2I(14,color_var));
				break;
			case NB_cell_types.STONE:
				color_var += random_color.RandiRange(-2,2);
				color_var = Math.Clamp(color_var, 0, 5);
				SetCell(position, p_layer, new Vector2I(color_var + 9,15));
				break;
			case NB_cell_types.AIR:
				SetCell(position, -1);
				break;
		}
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
			if (particles_locked_cell[index]) {continue;}
			switch (particle.type)
			{
				case NB_cell_types.SAND:
					//check bottom
					int exchange_cell = index + vecToIndex(0,1);
					NB_cell end_cell = particlesCheck(exchange_cell);
					if (end_cell.type == NB_cell_types.AIR)
					{
						particle.velocity += new Vector2(0,1) * fall_mul;
					}
					else
					{
						exchange_cell = index + vecToIndex(odd_update?1:-1,1);
						end_cell = particlesCheck(exchange_cell);
						if (end_cell.type == NB_cell_types.AIR)
						{
							particle.velocity += new Vector2(odd_update?1:-1,0) * fall_mul;
						}
						else
						{
							exchange_cell = index + vecToIndex(odd_update?-1:1,1);
							end_cell = particlesCheck(exchange_cell);
							if (end_cell.type == NB_cell_types.AIR)
							{
								particle.velocity += new Vector2(odd_update?-1:1,0) * fall_mul;
							}
						}
					}

					exchange_cell = index+vecToIndex((float)Math.Floor(particle.velocity.X), (float)Math.Floor(particle.velocity.Y));
					end_cell = particlesCheck(exchange_cell);
					if (end_cell.type == NB_cell_types.AIR)
					{
						particlesSwap(index, exchange_cell);
						particleLockNUpdateDoubl(index, exchange_cell);
						break;
					} else
					{

						// particle.velocity.X;
						// particle.velocity.Y;
						int naive_raycast_size = (int)Math.Ceiling(particle.velocity.Length());
						Vector2 naive_raycast_normalized = particle.velocity.Normalized();

						for (int step = 1; step < naive_raycast_size; step++)
						{
							// exchange_cell = index+vecToIndex(step * naive_raycast_normalized.X, step * naive_raycast_normalized.Y);
							exchange_cell = index+vecToIndex((float)Math.Floor(step * naive_raycast_normalized.X), (float)Math.Floor(step * naive_raycast_normalized.Y));
							
							NB_cell result_cell = particlesCheck(exchange_cell);
							if (result_cell.type != NB_cell_types.AIR)
							{
								step -= 1;
								// exchange_cell = index+vecToIndex(step * naive_raycast_normalized.X, step * naive_raycast_normalized.Y);
								exchange_cell = index+vecToIndex((float)Math.Floor(step * naive_raycast_normalized.X), (float)Math.Floor(step * naive_raycast_normalized.Y));
								particle.velocity = Vector2.Zero;
								particlesSwap(index, exchange_cell);
								particleLockNUpdateDoubl(index, exchange_cell);
								break;
							}
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
	private int vecToIndex(float p_x, float p_y)
	{
		return (int)Math.Ceiling(p_x) +particle_sim_size.X * (int)Math.Ceiling(p_y);
	}
	private NB_cell particlesCheck(int p_index)
	{
		if (p_index >= particle_sim_abs_size){return NB_CELL_VOID;}
		if (particles_locked_cell[p_index]) {return NB_CELL_VOID;}
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
		particles_locked_cell[p_index_1] = true;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	double timer_update = 0;
	double timer_update_max = 0.01;
	bool odd_update = false;
	double tx = 0;
	public override void _Process(double delta)
	{
		// timer_update += delta;
		// tx += delta;
		// if (timer_update > timer_update_max)
		// {
		// odd_update = !odd_update;
		// 	NB_cell temp_cell = new NB_cell(
		// 	new Vector2(0,0),
		// 	NB_cell_types.SAND);
		// 	// for (int x = 0; x < 3; x++)
		// 	// {
		// 	// 	for (int y = 0; y < 3; y++)
		// 	// 	{
		// 	// 		particles[70100 + vecToIndex(x*10,y*10) + (int)(Math.Sin(tx)*50)] = temp_cell;
		// 	// 	}
		// 	// }
		// 	particles[70100] = temp_cell;
		// 	updateParticleMovings();
		// 	updateParticleVisuals();
		// 	timer_update = 0;
		// }
		if (Input.IsActionJustPressed("ui_up") || Input.IsActionPressed("ui_down"))
		{
			odd_update = !odd_update;
			NB_cell temp_cell = new NB_cell(
			new Vector2(0,0),
			NB_cell_types.SAND);
			
			if (Input.IsActionPressed("ui_right"))
			{
				for (int x = 0; x < 3; x++)
				{
					for (int y = 0; y < 3; y++)
					{
						particles[70100 + vecToIndex(x*10,y*10) + (int)(Math.Sin(tx)*50)] = temp_cell;
					}
				}
			}
			particles[75100] = temp_cell;
			updateParticleMovings();
			updateParticleVisuals();
			timer_update = 0;	
		}
		if (Input.IsActionJustPressed("ui_left"))
		{
			odd_update = !odd_update;
		}
	}
}
