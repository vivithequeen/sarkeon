using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class Sand : TileMapLayer
{
	// structs
	enum NB_type
	{
		PLASMA,
		GAS,
		LIQUID,
		FALLING,
		SOLID,
	}
	
	class NB_particle
	{
		public NB_particle (
			NB_type p_type,
			bool p_solid,
			List<Vector2I> p_checking_pos
		)
		{
			type = p_type;
			solid = p_solid;
			checking_pos = p_checking_pos;
		}
		public bool empty = false;
		public NB_type type;
		public bool solid;
		public Vector2I particle_position;
		public Vector2I color;
		public List<Vector2I> checking_pos;
		public NB_particle pos(Vector2I p_position)
		{
			particle_position = p_position;
			return this;
		}
		public NB_particle clone()
		{
			NB_particle temp = new NB_particle(type, solid, checking_pos);
			temp.empty = empty;
			return temp;
		}
	}
	class NB_chunk
	{
		public NB_chunk(Vector2I p_position, Vector2I p_chunk_size)
		{
			chunk_size = p_chunk_size;
			chunk_position = p_position;
			cell_particle_offset = p_position * chunk_size;
			particles = new Dictionary<string, NB_particle>{};
			list_visual_update = new List<NB_particle>{};
			list_phisics_update = new List<NB_particle>{};
			
			temp_air =  new NB_particle(NB_type.GAS, false, new List<Vector2I> {});
			temp_air.empty = true;
			for (int Y = 0; Y < chunk_size.Y; Y++)
			{
				for (int X = 0; X < chunk_size.X; X++)
				{
					particles[fakeVecToString(X,Y)] = temp_air;
				}
			}

		}
		private NB_particle temp_air;
		public Vector2I chunk_position;
		public Vector2I cell_particle_offset;
		public Vector2I chunk_size;
		public Dictionary<string, NB_particle> particles;
		public List<NB_particle> list_visual_update;
		public List<NB_particle> list_phisics_update;
		public void particleAdd(NB_particle p_particle)
		{
			// GD.Print(particles[vecToString(p_particle.particle_position - cell_particle_offset)], p_particle);
			particles[vecToString(p_particle.particle_position - cell_particle_offset)] = p_particle;
			updateParticlenAdd(p_particle);
		}
		public void particleRemove(Vector2I p_particle_position)
		{
			NB_particle new_air = temp_air.clone().pos(p_particle_position);
			particles[vecToString(p_particle_position - cell_particle_offset)] = new_air;
			updateParticlenAdd(new_air);
		}
		private void updateParticlenAdd(NB_particle p_particle)
		{
			//TODO optimise memory of this (aka cutdown on doubles)
			list_phisics_update.Add(p_particle);
			list_visual_update.Add(p_particle);
		}
		public List<NB_particle> getUpdatedParticles()
		{
			//TODO add script that checks if there were no updates in chunks then it disables the chunk
			return list_phisics_update;
		}
		public List<NB_particle> getUpdatedParticles_visual()
		{
			//gives out variable, and expects the script to clear it
			return list_visual_update;
		}
		public bool getParticleSafe(Vector2I p_particle_position)
		{
			return 
				p_particle_position.X < cell_particle_offset.X + chunk_size.X &&
				p_particle_position.X > cell_particle_offset.X &&
				p_particle_position.Y < cell_particle_offset.Y + chunk_size.Y &&
				p_particle_position.Y > cell_particle_offset.Y;
		}
		public NB_particle getParticle(Vector2I p_particle_position)
		{
			return particles[vecToString(p_particle_position - cell_particle_offset)];
		}
		public string fakeVecToString(int X, int Y) => X + "," + Y;
		public string vecToString(Vector2I p_vec) => p_vec.X + "," + p_vec.Y;
	}
	//Variables
	Dictionary<String, NB_particle> particle_list;
	Vector2I chunk_size = new Vector2I(10,10);
	Dictionary<String, NB_chunk> chunks;
	List<Vector2I> chunks_update_list;
	//Init sands
	public Sand()
	{
		//init dicts
		particle_list = new Dictionary<String, NB_particle>{};
		chunks = new Dictionary<String, NB_chunk>{};
		//init list
		chunks_update_list = new List<Vector2I>{};
		//init all sand data
		particle_list.Add("Stone", new NB_particle(
			NB_type.SOLID,
			true,
			[]
		));
		particle_list.Add("Sand", new NB_particle(
			NB_type.FALLING,
			true,
			[
				new Vector2I(0,1),
				// new Vector2I(1,1),
				// new Vector2I(-1,1),
			]
		));
	}
	public override void _Ready()
	{
		// chunks.Add(fakeVecToString(0,0), new NB_chunk(new Vector2I(0,0), chunk_size));
		// chunks_update_list.Add(new Vector2I(0,0));
		//TODO add so you can add multiple particles aka particlesAdd
		// particleCellPlace(createParticle(new Vector2I(15,2), "Sand"));
		particleCellPlace(createParticle(new Vector2I(10,2), "Sand"));
		// particleCellPlace(createParticle(new Vector2I(4,2), "Sand"));
		// particleCellPlace(createParticle(new Vector2I(4,3), "Sand"));
		// particleCellPlace(createParticle(new Vector2I(5,3), "Sand"));
		// particleCellPlace(createParticle(new Vector2I(4,4), "Sand"));
		// particleCellPlace(createParticle(new Vector2I(4,5), "Sand"));
		particleCellPlace(createParticle(new Vector2I(1,10), "Stone"));
		particleCellPlace(createParticle(new Vector2I(2,10), "Stone"));
		particleCellPlace(createParticle(new Vector2I(3,10), "Stone"));
		particleCellPlace(createParticle(new Vector2I(4,10), "Stone"));
		particleCellPlace(createParticle(new Vector2I(5,10), "Stone"));
		particleCellPlace(createParticle(new Vector2I(6,10), "Stone"));
		particleCellPlace(createParticle(new Vector2I(7,10), "Stone"));
		particleCellPlace(createParticle(new Vector2I(8,10), "Stone"));
		// particleCellPlace(createParticle(new Vector2I(5,0), "Sand"));
		// particleCellPlace(createParticle(new Vector2I(10,0), "Sand"));
		// particleCellPlace(createParticle(new Vector2I(20,0), "Sand"));
		visualiser();
	}
	double timer = 0;
	public override void _Process(double delta)
	{
		timer += delta;
		if (timer > 0.1)
		{
			timer = 0;
			simulationStep();
			visualiser();
		}
	}
	public void simulationStep()
	{
		// GD.Print("1-", chunks_update_list.Count);
		for (int chunk_iter = 0; chunk_iter < chunks_update_list.Count; chunk_iter++)
		{
			NB_chunk temp_chunk = chunks[vecToString(chunks_update_list[chunk_iter])];

			List<NB_particle> particle_list = temp_chunk.getUpdatedParticles();
			int static_count = particle_list.Count;
			// GD.Print("--",chunk_iter,"-2-", static_count);
			// foreach (NB_particle item in particle_list)
			// {
			// 	GD.Print("--",chunk_iter,"-2-", static_count,"-3-", item.particle_position);
			// }
			for (int particle_iter = 0; particle_iter < Math.Min(static_count, 2)  ; particle_iter++)
			{
				GD.Print(static_count);
				NB_particle particle = particle_list[particle_iter];
				// GD.Print("4-", particle.checking_pos.Count);
				for (int check_offset_iter = 0; check_offset_iter < particle.checking_pos.Count; check_offset_iter++)
				{
					Vector2I check_offset = particle.checking_pos[check_offset_iter];
					if (check_pixel(check_offset, particle, temp_chunk)) {
						break;
					}
				}
			}
			particle_list.RemoveRange(0, static_count);
		}
	}
	private bool check_pixel(Vector2I check_offset, NB_particle p_particle, NB_chunk p_current_chunk)
	{
		Vector2I check_position = p_particle.particle_position + check_offset;
		NB_particle returned_particle;
		NB_chunk returned_chunk = p_current_chunk;
		if (p_current_chunk.getParticleSafe(check_position))
		{	
			returned_particle = p_current_chunk.getParticle(check_position);
		} else
		{
			returned_chunk = vecToChunk(check_position);
			returned_particle = returned_chunk.getParticle(check_position);
		}
		// GD.Print(p_particle.type," ", returned_particle.type, check_position);
		if (p_particle.type > returned_particle.type)
		{
			if (returned_particle.empty)
			{
				// use p_current_chunk for moving first particle
				p_current_chunk.particleRemove(p_particle.particle_position);
				p_particle.particle_position = check_position;
				// GD.Print(returned_chunk);
				returned_chunk.particleAdd(p_particle);
			} 
			// else
			// {
				
			// 	p_current_chunk.particleAdd(returned_particle);
			// 	returned_particle.particle_position = p_particle.particle_position;
			// 	returned_chunk.particleAdd(p_particle);
			// 	p_particle.particle_position = check_position;
			// }
			return true;
		}
		return false;
	} 
	private NB_chunk vecToChunk(Vector2I p_position)
	{
		Vector2I position = new Vector2I((int)Math.Floor((double)p_position.X/(double)chunk_size.X), (int)Math.Floor((double)p_position.Y/(double)chunk_size.Y));
		string key = vecToString(position);
		chunks_update_list.Add(position);
		if (chunks.ContainsKey(key))
		{
			return chunks[key];
		} else
		{
			chunks.Add(key, new NB_chunk(position, chunk_size));
			return chunks[key];	
		}
	}
	public void visualiser()
	{
		foreach (Vector2I chunk_update_coord in chunks_update_list)
		{
			List<NB_particle> particle_update_list = chunks[vecToString(chunk_update_coord)].getUpdatedParticles_visual();
			foreach (NB_particle particle in particle_update_list)
			{
				//TODO make so type affects the color drawn
				SetCell(particle.particle_position, 0, particle.color);
			}
			particle_update_list.Clear();
		}
	}
	private NB_particle createParticle(Vector2I p_positio, string type)
	{
		NB_particle return_particle = particle_list[type].clone().pos(p_positio);
		//TODO coloring script goes here
		return_particle.color = new Vector2I(14,0);
		return return_particle;
	}
	private void particleCellPlace(NB_particle p_particle)
	{
		NB_chunk detected_chunk = vecToChunk(p_particle.particle_position);
		detected_chunk.particleAdd(p_particle);
	}
	//Stringies ;PP
    public string fakeVecToString(int X, int Y) => X + "," + Y;
    public string vecToString(Vector2I p_vec) => p_vec.X + "," + p_vec.Y;
}