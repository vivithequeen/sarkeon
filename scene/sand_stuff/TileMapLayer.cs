using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class TileMapLayer : Godot.TileMapLayer
{
	enum NB_cell_property_type
	{
		SAND,
		STONE,
		AIR,
		VOID,
		WATER
	}
	enum NB_cell_type
	{
		PLASMA,
		GAS,
		LIQUID,
		FALLING,
		SOLID
	}
	private class NB_cell
	{
		public NB_cell(Vector2 p_velocity, NB_cell_type p_type, NB_cell_property_type p_property_type, bool p_fall)
		{
			vel_start = 0;
			velocity = p_velocity;
			type = p_type;
			property_type = p_property_type;
			color = new Vector2I(0,0);
			fall_direction = p_fall;
		}
		public int vel_start;
		public Vector2 velocity;
		public NB_cell_type type;
		public NB_cell_property_type property_type;
		public Vector2I color;
		public bool fall_direction;
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		public void colorSet(Vector2I p_color)
		{
			color = p_color;
		}
		public void velocity_flip()
		{
			fall_direction = !fall_direction;
		}
	}
	//TODO make so it only rerenders updated pixels for optimization
	// List<int> moving_id;
		// Use this for initing ;-;
		// moving_id = new List<int>();
	Vector2I particle_sim_size = new Vector2I(500,500);
	int particle_sim_abs_size;
	NB_cell[] particles;
	bool[] particles_locked_cell;
	//Used to update drawing and cells
	List<int> particles_buffer_updated_index;
	RandomNumberGenerator random_color;
	//Used for physics
	List<Rect2I> particles_rough_rect;
	NB_cell NB_CELL_VOID;
	int color_var = 0;
	float fall_mul = 0.1f;
	float terminal_vel = 100;
	public override void _Ready()
	{
		//Sets values
		particle_sim_abs_size = particle_sim_size.X * particle_sim_size.Y;
		particles_buffer_updated_index = new List<int>();
		particles_rough_rect = new List<Rect2I>();
		particles_rough_rect.Add(new Rect2I(0,250, particle_sim_size.X, particle_sim_size.Y - 250));
		random_color = new RandomNumberGenerator();
		random_color.Randomize();
		NB_CELL_VOID = particleCreate(Vector2.Zero,NB_cell_type.PLASMA, NB_cell_property_type.VOID);
		
		particles = new NB_cell[particle_sim_abs_size];
		particles_locked_cell = new bool[particle_sim_abs_size];
		// inits both arrays
		for (int i = 0; i < particle_sim_abs_size; i++)
		{
			NB_cell temp_cell = particleCreate(
			new Vector2(0,0),
			NB_cell_type.GAS,
			NB_cell_property_type.AIR);
			
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
				if (random_sand.RandiRange(0, 40) == 0)
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
			NB_cell temp_cell = particleCreate(
			new Vector2(0,0),
			NB_cell_type.SOLID,
			NB_cell_property_type.STONE);
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
			particlesUpdateVisual(index_of_cell, 0);
		}
		
		particles_buffer_updated_index.Clear();
	}
	private void particlesUpdateVisual(int p_index, int p_layer)
	{
		Vector2I position = idToPosition(p_index);
		NB_cell particle = particles[p_index];
		// returns if the cell is air XD
		if (particle.property_type == NB_cell_property_type.AIR) 
		{
			SetCell(position, -1);
			return;
		}
		SetCell(position, p_layer, particle.color);
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
			updateParticle(particle, index);
		}
	}
	private void updateParticle(NB_cell particle, int index)
	{
		switch (particle.property_type)
			{
				case NB_cell_property_type.SAND:
					//check bottom
					//TODO ADD le funny NB_cell_type.LIQUID so it filters it all out ;PP
					if(particleCheckAddVelocity(index, particle, 0, 1, 10))
					{
						if(particleCheckAddVelocity(index, particle, particle.fall_direction?-1:1, 1, 0.2f))
						{
							particle.velocity_flip();
						}
					}

					particleCollisionDetection(index, particle);
					break;
				case NB_cell_property_type.STONE:
					break;
				case NB_cell_property_type.AIR:
					break;
				case NB_cell_property_type.WATER:
					if (particleCheckAddVelocity(index, particle, 0, 1, 10))
					{
						if(particleCheckAddVelocity(index, particle, particle.fall_direction?-1:1, 0, 10f))
						{
							particle.velocity_flip();
						}
					}
					// particleCheckAddVelocity(index, particle, odd_update?-1:1, 1);
					// particleCheckAddVelocity(index, particle, odd_update?1:-1, 0);
					// particleCheckAddVelocity(index, particle, particle.fall_direction?-1:1, 0);

					particleCollisionDetection(index, particle);
					break;
			}
	}
	private bool particleCheckAddVelocity(int index, NB_cell particle, int p_x, int p_y, float p_mul)
	{
		int exchange_cell = index + vecToIndex(p_x,p_y);
		NB_cell end_cell = particlesCheck(exchange_cell);
		if (end_cell.property_type == NB_cell_property_type.VOID || end_cell.type == NB_cell_type.SOLID || end_cell.type == NB_cell_type.FALLING)
		{
			return true;
		}
		// particlesSwap(index, exchange_cell);
		// particleLockNUpdateDoubl(index, exchange_cell);
		particle.velocity = (particle.velocity + new Vector2(p_x, p_y) * fall_mul * p_mul).Clamp(-terminal_vel, terminal_vel);
		return false;
	}
	private void particleCollisionDetection(int index, NB_cell particle)
	{
		int exchange_cell = index+vecToIndex((float)Math.Floor(particle.velocity.X), (float)Math.Floor(particle.velocity.Y));
		if (index == exchange_cell) {return;}
		NB_cell end_cell = particlesCheck(exchange_cell);
		if (end_cell.property_type == NB_cell_property_type.VOID || end_cell.type == NB_cell_type.SOLID || end_cell.type == NB_cell_type.FALLING)
		{
			particleCollisionRaycast(index, particle);
			return;
		} else
		{
			particlesSwap(index, exchange_cell);
			particleLockNUpdateDoubl(index, exchange_cell);
			return;
		}
	}
	private void particleCollisionRaycast(int index, NB_cell particle)
	{
		int naive_raycast_size = (int)Math.Ceiling(particle.velocity.Length());
		Vector2 naive_raycast_normalized = particle.velocity.Normalized();

		for (int step = 1; step < naive_raycast_size; step++)
		{
			// exchange_cell = index+vecToIndex(step * naive_raycast_normalized.X, step * naive_raycast_normalized.Y);
			int exchange_cell = index+vecToIndex((float)Math.Floor(step * naive_raycast_normalized.X), (float)Math.Floor(step * naive_raycast_normalized.Y));
			
			NB_cell result_cell = particlesCheck(exchange_cell);
			if (result_cell.property_type == NB_cell_property_type.VOID || result_cell.property_type == NB_cell_property_type.STONE)
			{
				step -= 1;
				// exchange_cell = index+vecToIndex(step * naive_raycast_normalized.X, step * naive_raycast_normalized.Y);
				exchange_cell = index+vecToIndex((float)Math.Floor(step * naive_raycast_normalized.X), (float)Math.Floor(step * naive_raycast_normalized.Y));
				particle.velocity = Vector2.Zero;
				particlesSwap(index, exchange_cell);
				particleLockNUpdateDoubl(index, exchange_cell);
				return;
			}
			// NB_cell result_cell = particlesCheck(exchange_cell);
			// if (result_cell.type == NB_cell_type.SOLID || result_cell.type == NB_cell_type.FALLING || result_cell.property_type == NB_cell_property_type.VOID)
			// {
			// 	// step -= 1;
			// 	// exchange_cell = index+vecToIndex(step * naive_raycast_normalized.X, step * naive_raycast_normalized.Y);
			// 	exchange_cell = index+vecToIndex((float)Math.Floor(step * naive_raycast_normalized.X), (float)Math.Floor(step * naive_raycast_normalized.Y));
			// 	particle.velocity = Vector2.Zero;
			// 	particlesSwap(index, exchange_cell);
			// 	particleLockNUpdateDoubl(index, exchange_cell);
			// 	break;
			// }
		}

		// particlesSwap(index, index+vecToIndex(particle.velocity.X, particle.velocity.Y));
		// particleLockNUpdateDoubl(index, index+vecToIndex(particle.velocity.X, particle.velocity.Y));
	}
	private int vecToIndex(float p_x, float p_y)
	{
		return (int)Math.Ceiling(p_x) +particle_sim_size.X * (int)Math.Ceiling(p_y);
	}
	private NB_cell particlesCheck(int p_index)
	{
		if (p_index < 0){return NB_CELL_VOID;}
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
	private NB_cell particleCreate(Vector2 p_velocity, NB_cell_type p_type, NB_cell_property_type p_property_type)
	{
		NB_cell temp_particle = new NB_cell(p_velocity, p_type, p_property_type, odd_update);
		particleColorSet(temp_particle);
		return temp_particle;
	}
	private void particleColorSet(NB_cell particle)
	{
		switch (particle.property_type)
		{
			case NB_cell_property_type.SAND:
				color_var += random_color.RandiRange(-1,1);
				color_var = Math.Clamp(color_var, 0, 4);
				particle.colorSet(new Vector2I(14,color_var));
				break;
			case NB_cell_property_type.STONE:
				color_var += random_color.RandiRange(-2,2);
				color_var = Math.Clamp(color_var, 0, 5);
				particle.colorSet(new Vector2I(color_var + 9,15));
				break;
			case NB_cell_property_type.AIR:
				particle.colorSet(new Vector2I(0,0));
				break;
			case NB_cell_property_type.WATER:
				color_var += random_color.RandiRange(-1,1);
				color_var = Math.Clamp(color_var, 0, 2);
				particle.colorSet(new Vector2I(10,color_var));
				break;
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	double timer_update = 0;
	double timer_update_max = 0.05;
	bool odd_update = false;
	double tx = 0;
	int sand_left = 100;
	public override void _Process(double delta)
	{
		timer_update += delta;
		if (timer_update > timer_update_max)
		{
			tx += 0.1;
			odd_update = !odd_update;
			NB_cell temp_cell = particleCreate(
			new Vector2(0,0),
			NB_cell_type.FALLING,
			NB_cell_property_type.SAND);
			NB_cell temp_water = particleCreate(
			new Vector2(0,0),
			NB_cell_type.LIQUID,
			NB_cell_property_type.WATER);
			if (sand_left > 0)
			{
				sand_left -= 1;
				for (int x = 0; x < 10; x++)
				{
					for (int y = 0; y < 10; y++)
					{
						particles[70000 + vecToIndex(x,y) + (int)(Math.Sin(tx*2)*25)] = temp_cell;
						particles_buffer_updated_index.Add(70000 + vecToIndex(x,y) + (int)(Math.Sin(tx*2)*25));
					}
				}
				for (int x = 0; x < 10; x++)
				{
					for (int y = 0; y < 10; y++)
					{
						particles[70100 + vecToIndex(x,y) + (int)(Math.Sin(tx*2)*25)] = temp_water;
						particles_buffer_updated_index.Add(70100 + vecToIndex(x,y) + (int)(Math.Sin(tx*2)*25));
					}
				}
			}
			updateParticleMovings();
			updateParticleVisuals();
			timer_update = 0;
		}
	}
}
