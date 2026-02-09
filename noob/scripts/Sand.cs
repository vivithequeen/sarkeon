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
			List<Vector2I> p_checking_pos,
			string p_particle_name,
			int p_strong = 0
		)
		{
			type = p_type;
			solid = p_solid;
			checking_pos = p_checking_pos;
			particle_name = p_particle_name;
			strong = p_strong;
		}
		public bool flip = false;
		public bool empty = false;
		public NB_type type;
		public bool solid;
		public Vector2I particle_position;
		public Vector2I color;
		public List<Vector2I> checking_pos;
		public int particle_update_cycle;
		public string particle_name;
		public int strong;
		public NB_particle pos(Vector2I p_position)
		{
			particle_position = p_position;
			return this;
		}
		public List<Vector2I> getPositions()
		{
			return checking_pos;
		}
		public NB_particle clone()
		{
			NB_particle temp = new NB_particle(type, solid, checking_pos, particle_name, strong);
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
			
			for (int Y = 0; Y < chunk_size.Y; Y++)
			{
				for (int X = 0; X < chunk_size.X; X++)
				{
					Vector2I temp_offset = new Vector2I(X, Y);
					NB_particle temp_air =  new NB_particle(NB_type.GAS, false, new List<Vector2I> {}, "Air");
					temp_air.empty = true;
					temp_air.pos(cell_particle_offset + temp_offset);
					particles[vecToString(temp_offset)] = temp_air;
				}
			}

		}
		public Vector2I chunk_position;
		public Vector2I cell_particle_offset;
		public Vector2I chunk_size;
		public Dictionary<string, NB_particle> particles;
		public void particleAdd(NB_particle p_particle)
		{
			// GD.Print(particles[vecToString(p_particle.particle_position - cell_particle_offset)], p_particle);
			particles[vecToString(p_particle.particle_position - cell_particle_offset)] = p_particle;
		}
		public void particleRemove(Vector2I p_particle_position)
		{
			NB_particle temp_air =  new NB_particle(NB_type.GAS, false, new List<Vector2I> {}, "Air");
			temp_air.empty = true;
			temp_air.pos(p_particle_position);
			particles[vecToString(p_particle_position - cell_particle_offset)] = temp_air;
		}
		public List<NB_particle> getUpdatedParticles()
		{
			return [.. particles.Values.Reverse()];
		}
		public bool getParticleSafe(Vector2I p_particle_position)
		{
			return 
				p_particle_position.X < cell_particle_offset.X + chunk_size.X &&
				p_particle_position.X > cell_particle_offset.X &&
				p_particle_position.Y < cell_particle_offset.Y + chunk_size.Y  &&
				p_particle_position.Y > cell_particle_offset.Y;
		}
		public NB_particle getParticle(Vector2I p_particle_position)
		{
			return particles[vecToString(p_particle_position - cell_particle_offset)];
		}
		public string pop_pixel(Vector2I p_particle_position)
		{
			string temp = particles[vecToString(p_particle_position - cell_particle_offset)].particle_name;
            NB_particle temp_air = new NB_particle(NB_type.GAS, false, new List<Vector2I> { }, "Air")
            {
                empty = true
            };
            temp_air.pos(p_particle_position);
			particles[vecToString(p_particle_position - cell_particle_offset)] = temp_air;
			return temp;
		}
		public string pop_pixel_str(Vector2I p_particle_position, int hit_strength)
		{
			NB_particle temp_p = particles[vecToString(p_particle_position - cell_particle_offset)];
			GD.Print(hit_strength);
			if (temp_p.strong > hit_strength) {return "";}
			string temp = particles[vecToString(p_particle_position - cell_particle_offset)].particle_name;
            NB_particle temp_air = new NB_particle(NB_type.GAS, false, new List<Vector2I> { }, "Air")
            {
                empty = true
            };
            temp_air.pos(p_particle_position);
			particles[vecToString(p_particle_position - cell_particle_offset)] = temp_air;
			return temp;
		}
		public string fakeVecToString(int X, int Y) => X + "," + Y;
		public string vecToString(Vector2I p_vec) => p_vec.X + "," + p_vec.Y;
	}
	RandomNumberGenerator random_color;
	int color_variation = 0;
	//Variables
	Dictionary<String, NB_particle> particle_list;
	Dictionary<String, NB_chunk> chunks;
	List<Vector2I> chunks_update_list;
	int particle_update_cycle = 0;
	bool flip_direction = false;
	int cell_update_cycle = 0;
	[Export]
	public Vector2I chunk_size = new Vector2I(32,32);
	[Export]
    public float refresh_delay { get; set; } = 0.01f;
	[Export]
    public int refresh_steps { get; set; } = 1;
	[Export]
    public int loaded_chunks { get; set; } = 6*5*4+5+6+100;
	[Export]
	public Vector2I load_pos { get; set; } = new Vector2I(0,0);
	[Export]
	public Vector2I load_size { get; set; } = new Vector2I(1,1);
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
			[],
			"Stone",
			p_strong: 1000
		));
		particle_list.Add("Sand", new NB_particle(
			NB_type.FALLING,
			true,
			[
				new Vector2I(0,2),
				new Vector2I(1,1),
				new Vector2I(2,1),
				new Vector2I(0,1),
			],
			"Sand",
			p_strong: 100
		));
		particle_list.Add("Water", new NB_particle(
			NB_type.LIQUID,
			false,
			[
				new Vector2I(0,2),
				new Vector2I(2,1),
				new Vector2I(1,1),
				new Vector2I(2,0),
				new Vector2I(1,0),
				new Vector2I(0,1),
			],
			"Water",
			p_strong: 50
		));
		//TODO make it global
		// Init random numbers
		random_color = new RandomNumberGenerator();
		random_color.Seed = 100;
	}
	public override void _Ready()
	{
		initDebugMonitor();
		//TODO add so you can add multiple particles aka particlesAdd
		foreach (Vector2I coords in GetUsedCells())
        {
			switch (GetCellAtlasCoords(coords))
			{
				case Vector2I(0,2):
					particleCellPlace(createParticle(coords, "Water"));
					break;
				case Vector2I(14,1):
					particleCellPlace(createParticle(coords, "Sand"));
					break;
				case Vector2I(11,15):
					particleCellPlace(createParticle(coords, "Stone"));
					break;
				default:
					break;
			}
        }
		visualiser();
	}
	List<MeshInstance2D> debug_chunks = [];
	// very unoptimized debug code o.O
	public void drawChunks()
	{
		foreach (MeshInstance2D to_delete in debug_chunks)
		{
			to_delete.QueueFree();
		}
		debug_chunks.Clear();
		foreach (NB_chunk chunk_iter in chunks.Values)
		{
			MeshInstance2D squre_outline = new MeshInstance2D();
			QuadMesh temp = new QuadMesh();
			temp.Size = chunk_size;
			squre_outline.Modulate = new Color(0.5f + random_color.RandfRange(-0.2f,0.2f), 0, 0);
			squre_outline.Mesh = temp;
			squre_outline.GlobalPosition = chunk_iter.cell_particle_offset + chunk_size/2 + new Vector2(0,-0.25f);
			squre_outline.ZIndex = -1;
			AddChild(squre_outline);
			debug_chunks.Add(squre_outline);
		}
		foreach (Vector2I chunk_iter in chunks_update_list)
		{
			MeshInstance2D squre_outline = new MeshInstance2D();
			QuadMesh temp = new QuadMesh();
			temp.Size = chunk_size;
			squre_outline.Modulate = new Color(0, 0.5f + random_color.RandfRange(-0.2f,0.2f), 0);
			squre_outline.Mesh = temp;
			squre_outline.GlobalPosition = chunks[vecToString(chunk_iter)].cell_particle_offset + chunk_size/2 + new Vector2(0,-0.25f);
			squre_outline.ZIndex = -1;
			AddChild(squre_outline);
			debug_chunks.Add(squre_outline);
		}
	}
	double timer = 0;
	public override void _Process(double delta)
	{
		timer += delta;
		if (timer > refresh_delay)
		{
			timer = 0;
			for (int n = 0; n < refresh_steps; n++)
			{
				flip_direction = !flip_direction;
				particle_update_cycle = particle_update_cycle + 1 % 1000000;
				for (int i = 0; i < 4; i++)
				{
					cell_update_cycle = (cell_update_cycle + 1) % 4;
					simulationStep();
				}
			}
			visualiser();
			// drawChunks();
		}
	}
	public void simulationStep()
	{
		//!TODO just like replace all update chunk system with just simulating chunks that are around player..
		//Aka add so it adds que to it and if chunk is out then it gets deloaded, unles it deloads faster by itself
		// GD.Print("New update");
		chunks_update_list.RemoveRange(0, Math.Max(chunks_update_list.Count() - loaded_chunks, 0));
		foreach (Vector2I chunk in chunks_update_list.Distinct())
		{
			int temp_check = chunk.X % 2 + chunk.Y % 2 * 2;
			temp_check = temp_check < 0 ? 4 + temp_check: temp_check;
			// GD.Print(temp_check, cell_update_cycle);
			if (temp_check != cell_update_cycle)
			{
				continue;
			}
			NB_chunk temp_chunk = chunks[vecToString(chunk)];

			List<NB_particle> temp_particles = temp_chunk.getUpdatedParticles();
			if (temp_particles.Count() == 0) {continue;}
			foreach (NB_particle particle in temp_particles)
			{
				if (particle.particle_update_cycle == particle_update_cycle)
				{
					continue;
				}
				Vector2I multiplier = flip_direction? new Vector2I(-1,1): new Vector2I(1,1);
				for (int check_offset_iter = 0; check_offset_iter < particle.checking_pos.Count; check_offset_iter++)
				{
					Vector2I check_offset = particle.checking_pos[check_offset_iter];
					if (check_pixel(check_offset * multiplier, particle, temp_chunk)) {
						break;
					} else if (check_pixel(check_offset * (multiplier * new Vector2I(-1,1)), particle, temp_chunk)){
						particle.flip = !particle.flip;
						break;
					}
				}
			}
		}
	}
	private bool check_pixel(Vector2I check_offset, NB_particle p_particle, NB_chunk p_current_chunk)
	{
		Vector2I check_position = p_particle.particle_position + check_offset;
		NB_particle returned_particle;
		NB_chunk returned_chunk = p_current_chunk;
		//TODO optimize this part aka make so it is calculated localy and math not bunch of if else statments and maybe cache the math
		if (p_current_chunk.getParticleSafe(check_position))
		{	
			returned_particle = p_current_chunk.getParticle(check_position);
		} else
		{
			returned_chunk = vecToChunk(check_position);
			returned_particle = returned_chunk.getParticle(check_position);
			// GD.Print(returned_particle.type);
			// return false;
		}
		if (p_particle.type > returned_particle.type)
		{
			//TODO optimize this
			if (returned_particle.empty)
			{
				// use p_current_chunk for moving first particle
				p_current_chunk.particleRemove(p_particle.particle_position);
				p_particle.particle_position = check_position;
				p_particle.particle_update_cycle = particle_update_cycle;

				returned_chunk.particleAdd(p_particle);
				// GD.Print(returned_chunk.chunk_position, p_current_chunk.chunk_position);
			} 	
			else
			{
				returned_particle.particle_update_cycle = particle_update_cycle;
				p_particle.particle_update_cycle = particle_update_cycle;

				returned_particle.particle_position = p_particle.particle_position;
				p_current_chunk.particleAdd(returned_particle);
				p_particle.particle_position = check_position;
				returned_chunk.particleAdd(p_particle);
			}
			return true;
		}
		return false;
	} 
	private NB_chunk vecToChunk(Vector2I p_position)
	{
		//TODO optimize this code
		Vector2I position = new Vector2I((int)Math.Floor((double)p_position.X/(double)chunk_size.X), (int)Math.Floor((double)p_position.Y/(double)chunk_size.Y));
		string key = vecToString(position);
		// if (chunks_update_list)
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
			//!TODO rewrite
			NB_chunk temp_chunk = chunks[vecToString(chunk_update_coord)];
			List<NB_particle> temp_particles = temp_chunk.getUpdatedParticles();
			foreach (NB_particle particle in temp_particles)
			{
				//TODO make so type affects the color drawn
				if (particle.empty)
				{
					SetCell(particle.particle_position, -1);
				} else
				{
					SetCell(particle.particle_position, particle.solid? 0: 1, particle.color);
				}
			}
		}
	}
	private NB_particle createParticle(Vector2I p_positio, string type)
	{
		NB_particle return_particle = particle_list[type].clone().pos(p_positio);
		//TODO coloring script goes here
		return_particle.flip = flip_direction;
		color_variation += random_color.RandiRange(-1,1);
		switch (type)
		{
			case "Sand":
				color_variation = Math.Clamp(color_variation, 0, 4);
				return_particle.color = new Vector2I(14,color_variation);
				break;
			case "Stone":
				color_variation = Math.Clamp(color_variation, 0, 3);
				return_particle.color = new Vector2I(9 + color_variation,15);
				break;
			case "Water":
				color_variation = Math.Clamp(color_variation, 0, 3);
				return_particle.color = new Vector2I(2,8 + color_variation);
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
	public void newPos(Vector2I p_newPosition)
	{
		// GD.Print("new pos");
		load_pos = p_newPosition;
		//!TODO insert the loading script here to load chunks
		for (int y = -load_size.Y; y < load_size.Y; y++)
		{
			for (int x = -load_size.X; x < load_size.X; x++)
			{	
				Vector2I temp_vec = new Vector2I(x,y) + load_pos;
				if (!chunks_update_list.Contains(temp_vec))
				{
					string key = vecToString(temp_vec);
					if (!chunks.ContainsKey(key)){
						chunks.Add(key, new NB_chunk(temp_vec, chunk_size));
					}
					// GD.Print("A");
					chunks_update_list.Add(temp_vec);
					continue;
				}
			}
		}
	}
	//Stringies ;PP
	public string fakeVecToString(int X, int Y) => X + "," + Y;
	public string vecToString(Vector2I p_vec) => p_vec.X + "," + p_vec.Y;
	//* For monitoring
	public void initDebugMonitor()
	{
		Performance.AddCustomMonitor("NB_sand/particles", Callable.From(DebugMonitor_GetParticles));
		Performance.AddCustomMonitor("NB_sand/chunks", Callable.From(DebugMonitor_GetChunks));
		Performance.AddCustomMonitor("NB_sand/update_chunks", Callable.From(DebugMonitor_GetUpdateChunks));
		Performance.AddCustomMonitor("NB_sand/unique_update_chunks", Callable.From(DebugMonitor_GetUniqueUpdateChunks));
	}
	public int DebugMonitor_GetParticles()
	{
		return particle_list.Count;
	}
	public int DebugMonitor_GetChunks()
	{
		return chunks.Count;
	}
	public int DebugMonitor_GetUpdateChunks()
	{
		return chunks_update_list.Count;
	}
	public int DebugMonitor_GetUniqueUpdateChunks()
	{
		return chunks_update_list.Distinct().Count();
	}
	//! Public for player
	public Dictionary<string, int> digSquare(Vector2I p_global_position, int size, int hit_strength)
	{
		Dictionary<string, int> return_val = new Dictionary<string, int>{};
		for (int x =  - size; x < size; x++)
		{
			for (int y = - size; y < size; y++)
			{
				Vector2I position_offset =  new Vector2I(x,y) + ( p_global_position - (Vector2I)GlobalPosition) / 4 ;
				Vector2I position = new Vector2I((int)Math.Floor(position_offset.X / (double)chunk_size.X), (int)Math.Floor(position_offset.Y / (double)chunk_size.Y));
				string key = vecToString(position);
				if (chunks.ContainsKey(key))
				{
					string temp = chunks[key].pop_pixel_str(position_offset, hit_strength);
					if (temp != "")
					{
						if (return_val.ContainsKey(temp))
						{
							return_val[temp] += 1;
						} else
						{
							return_val[temp] = 1;
						}
					}
				}
			}
		}
		return return_val;
	}
}