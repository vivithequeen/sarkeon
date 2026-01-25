using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
			
			for (int Y = 0; Y < chunk_size.Y; Y++)
			{
				for (int X = 0; X < chunk_size.X; X++)
				{
					temp_air =  new NB_particle(NB_type.GAS, false, new List<Vector2I> {});
					temp_air.empty = true;
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
		public void 	updateParticlenAdd(NB_particle p_particle)
		{
			//TODO optimise memory of this (aka cutdown on doubles)
			list_phisics_update.Add(p_particle);
			list_visual_update.Add(p_particle);
		}
		public List<NB_particle> getUpdatedParticles()
		{
			//TODO add script that checks if there were no updates in chunks then it disables the chunk
			// List<NB_particle> newList = new List<NB_particle>(list_phisics_update);
			// list_phisics_update.Clear();
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
	RandomNumberGenerator random_color;
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
				new Vector2I(1,1),
				new Vector2I(-1,1),
			]
		));
		// Init random numbers
		random_color = new RandomNumberGenerator();
		random_color.Seed = 100;
	}
	public override void _Ready()
	{
		// chunks.Add(fakeVecToString(0,0), new NB_chunk(new Vector2I(0,0), chunk_size));
		// chunks_update_list.Add(new Vector2I(0,0));
		//TODO add so you can add multiple particles aka particlesAdd
		for (float x = -40; x < 80; x += 0.5f)
		{
			int x_offset = (int)Math.Floor((
					Math.Sin(x/10f) +
					Math.Sin(x/5f+5)
				)*5);
			particleCellPlace(createParticle(new Vector2I((int)Math.Floor(x),10 + x_offset), "Stone"));
			particleCellPlace(createParticle(new Vector2I((int)Math.Floor(x),10 + x_offset+1), "Stone"));
			particleCellPlace(createParticle(new Vector2I((int)Math.Floor(x),10 + x_offset+2), "Stone"));
		}
		for (int y = 0; y < 10; y++)
		{
			for (int x = 0; x < 10; x++)
			{
				particleCellPlace(createParticle(new Vector2I(x*2 -10,y*2-30), "Sand"));
			}
		}
		visualiser();
	}
	List<MeshInstance2D> debug_chunks = [];
	public void drawChunks()
	{
		foreach (MeshInstance2D to_delete in debug_chunks)
		{
			to_delete.QueueFree();
		}
		debug_chunks.Clear();
		foreach (NB_chunk chunk in chunks.Values)
		{
			MeshInstance2D squre_outline = new MeshInstance2D();
			QuadMesh temp = new QuadMesh();
			temp.Size = chunk_size;
			squre_outline.Modulate = new Color(0.5f, 0, 0);
			squre_outline.Mesh = temp;
			squre_outline.GlobalPosition = chunk.cell_particle_offset + chunk_size/2  + Vector2.One / 2f;
			squre_outline.ZIndex = -2;
			AddChild(squre_outline);
			debug_chunks.Add(squre_outline);
			// GD.Print(chunk.cell_particle_offset);
		}
		foreach (Vector2I chunk_iter in chunks_update_list)
		{
			MeshInstance2D squre_outline = new MeshInstance2D();
			QuadMesh temp = new QuadMesh();
			temp.Size = chunk_size;
			squre_outline.Modulate = new Color(0, 0.5f, 0);
			squre_outline.Mesh = temp;
			squre_outline.GlobalPosition = chunks[vecToString(chunk_iter)].cell_particle_offset + chunk_size/2 + Vector2.One / 2f;
			squre_outline.ZIndex = -1;
			AddChild(squre_outline);
			debug_chunks.Add(squre_outline);
			// GD.Print(chunk.cell_particle_offset);
		}
	}
	double timer = 0;
	public override void _Process(double delta)
	{
		timer += delta;
		if (timer > 0.1f)
		{
			timer = 0;
			simulationStep();
			visualiser();
			drawChunks();
		}
	}
	public void simulationStep()
	{
		List<Vector2I> chunks_update_list_dub = [.. chunks_update_list];
		chunks_update_list.Clear();
		// GD.Print("1-", chunks_update_list_dub.Count);
		foreach (Vector2I chunk in chunks_update_list_dub.Distinct())
		{
			NB_chunk temp_chunk = chunks[vecToString(chunk)];
			List<NB_particle> particle_list = temp_chunk.getUpdatedParticles();
			int static_count = particle_list.Count;
			// GD.Print(static_count);
			for (int particle_iter = 0; particle_iter < static_count  ; particle_iter++)
			{
				NB_particle particle = particle_list[particle_iter];
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
				
				chunks_update_list.Add(p_current_chunk.chunk_position);
				returned_chunk.particleAdd(p_particle);
				chunks_update_list.Add(returned_chunk.chunk_position);
				
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
		// GD.Print(p_position, key);
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
		foreach (Vector2I chunk_update_coord in chunks_update_list.Distinct())
		{
			List<NB_particle> particle_update_list = chunks[vecToString(chunk_update_coord)].getUpdatedParticles_visual();
			foreach (NB_particle particle in particle_update_list)
			{
				//TODO make so type affects the color drawn
				if (particle.empty)
				{
					SetCell(particle.particle_position, -1);
				} else
				{
					SetCell(particle.particle_position, 0, particle.color);
				}
			}
			particle_update_list.Clear();
		}
	}
	private NB_particle createParticle(Vector2I p_positio, string type)
	{
		NB_particle return_particle = particle_list[type].clone().pos(p_positio);
		//TODO coloring script goes here
		switch (type)
		{
			case "Sand":
				return_particle.color = new Vector2I(14,random_color.RandiRange(0,4));
				break;
			case "Stone":
				return_particle.color = new Vector2I(9 + random_color.RandiRange(0,3),15);
				break;
			default:
				return_particle.color = new Vector2I(0,0);
				break;
		}
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
