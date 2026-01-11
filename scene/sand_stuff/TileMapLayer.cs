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
		public NB_cell(Vector2 p_velocity, NB_cell_type p_type, NB_cell_property_type p_property_type)
		{
			vel_start = 0;
			velocity = p_velocity;
			type = p_type;
			property_type = p_property_type;
		}
		public int vel_start;
		public Vector2 velocity;
		public NB_cell_type type;
		public NB_cell_property_type property_type;
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
	bool[] particles_selection_simulation;
	bool[] particles_selection_visual;
	NB_cell NB_CELL_VOID = new NB_cell(Vector2.Zero,NB_cell_type.PLASMA, NB_cell_property_type.VOID);
	int color_var = 0;
	float fall_mul = 1f;
	public override void _Ready()
	{
		//Sets values
		particle_sim_abs_size = particle_sim_size.X * particle_sim_size.Y;
		particles_buffer_updated_index = new List<int>();
		particles_selection_simulation = new bool[particle_sim_size.Y];
		particles_selection_visual = new bool[particle_sim_size.Y];
		random_color = new RandomNumberGenerator();
		random_color.Randomize();
		
		particles = new NB_cell[particle_sim_abs_size];
		particles_locked_cell = new bool[particle_sim_abs_size];
		// inits both arrays
		for (int i = 0; i < particle_sim_abs_size; i++)
		{
			NB_cell temp_cell = new NB_cell(
			new Vector2(0,0),
			NB_cell_type.GAS,
			NB_cell_property_type.AIR);
			
			particles[i] = (NB_cell)temp_cell.Clone();
			particles_locked_cell[i] = false;
		}

		//Sets base stone
		for (int x = 0; x < particle_sim_size.X; x++)
		{
			NB_cell temp_cell = new NB_cell(
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
		for (int i = 0; i < particle_sim_abs_size; i++)
		{
			particles_locked_cell[i] = false;
			// particles_locked_cell[i + vecToIndex(0,-1)] = false;
			particlesUpdateVisual(i, 1);
		}
		//TODO OPTIMIZE :pray:
		// for (int y = 0; y < particle_sim_size.Y; y++)
		// {
		// 	if (!particles_selection_visual[y])
		// 	{
		// 		continue;
		// 	}
		// 	particles_selection_visual[y] = false;
		// 	for (int x = 0; x < particle_sim_size.X; x++)
		// 	{
		// 		var index = vecToIndex(x, y);
		// 		particles_locked_cell[index] = false;

		// 		// particles_locked_cell[index + vecToIndex(0,-1)] = false;
		// 		particlesUpdateVisual(index, 1);
		// 	}
		// }
		particles_buffer_updated_index.Clear();
	}
	private void particlesUpdateVisual(int p_index, int p_layer)
	{
		Vector2I position = idToPosition(p_index);
		NB_cell particle = particles[p_index];
		switch (particle.property_type)
		{
			case NB_cell_property_type.SAND:
				color_var += random_color.RandiRange(-1,1);
				color_var = Math.Clamp(color_var, 0, 4);
				SetCell(position, p_layer, new Vector2I(14,color_var));
				break;
			case NB_cell_property_type.STONE:
				color_var += random_color.RandiRange(-2,2);
				color_var = Math.Clamp(color_var, 0, 5);
				SetCell(position, p_layer, new Vector2I(color_var + 9,15));
				break;
			case NB_cell_property_type.AIR:
				SetCell(position, -1);
				break;
			case NB_cell_property_type.WATER:
				color_var += random_color.RandiRange(-1,1);
				color_var = Math.Clamp(color_var, 0, 2);
				SetCell(position, p_layer, new Vector2I(10,color_var));
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
					if(particleCheckAddVelocity(index, particle, 0,1)){
					if(particleCheckAddVelocity(index, particle, odd_update?1:-1, 1))
					{particleCheckAddVelocity(index, particle, odd_update?-1:1, 1);}}

					particleCollisionDetection(index, particle);
					break;
				case NB_cell_property_type.STONE:
					break;
				case NB_cell_property_type.AIR:
					break;
				case NB_cell_property_type.WATER:
					if(particleCheckAddVelocity(index, particle, 0,1)){
					if(particleCheckAddVelocity(index, particle, odd_update?1:-1, 1)){
					if(particleCheckAddVelocity(index, particle, odd_update?-1:1, 1)){
					if(particleCheckAddVelocity(index, particle, odd_update?1:-1, 0)){
					particleCheckAddVelocity(index, particle, odd_update?-1:1, 0);}}}}

					particleCollisionDetection(index, particle);
					break;
			}
	}
	private bool particleCheckAddVelocity(int index, NB_cell particle, int p_x, int p_y)
	{
		int exchange_cell = index + vecToIndex(p_x,p_y);
		NB_cell end_cell = particlesCheck(exchange_cell);
		if (end_cell.type == NB_cell_type.GAS)
		{
			particle.velocity += new Vector2(p_x, p_y) * fall_mul;
			return false;
		}
		return true;
	}
	private void particleCollisionDetection(int index, NB_cell particle)
	{
		int exchange_cell = index+vecToIndex((float)Math.Floor(particle.velocity.X), (float)Math.Floor(particle.velocity.Y));
		NB_cell end_cell = particlesCheck(exchange_cell);
		if (end_cell.property_type == NB_cell_property_type.AIR || end_cell.property_type == NB_cell_property_type.WATER)
		{
			particlesSwap(index, exchange_cell);
			particleLockNUpdateDoubl(index, exchange_cell);
			return;
		} else
		{
			particleCollisionRaycast(index, particle);
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
				if (result_cell.property_type != NB_cell_property_type.AIR && result_cell.property_type != NB_cell_property_type.WATER)
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
	int sand_left = 500;
	public override void _Process(double delta)
	{
		timer_update += delta;
		tx += delta;
		if (timer_update > timer_update_max)
		{
		odd_update = !odd_update;
			NB_cell temp_cell = new NB_cell(
			new Vector2(0,0),
			NB_cell_type.FALLING,
			NB_cell_property_type.SAND);
			NB_cell temp_water = new NB_cell(
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
