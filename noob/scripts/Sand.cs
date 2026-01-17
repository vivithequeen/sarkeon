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
		public bool empty;
		public NB_type type;
		public bool solid;
		public Vector2I position;
		public Vector2I color;
		public List<Vector2I> checking_pos;
		public NB_particle pos(Vector2I p_position)
		{
			position = p_position;
			return this;
		}
	}
	class NB_chunk
	{
		public NB_chunk(Vector2I p_position, Vector2I chunk_size)
		{
			position = p_position;
			global_position = p_position * chunk_size;
			particles = new Dictionary<string, NB_particle>{};
			update_list = new List<NB_particle>{};
			NB_particle temp_air =  new NB_particle(NB_type.GAS, false, new List<Vector2I> {});
			temp_air.empty = true;
			for (int Y = 0; Y < chunk_size.Y; Y++)
			{
				for (int X = 0; X < chunk_size.X; X++)
				{
					particles[fakeVecToString(X,Y)] = temp_air;
				}
			}
		}
		public Vector2I position;
		public Vector2I global_position;
		public Dictionary<string, NB_particle> particles;
		public List<NB_particle> update_list;
		public void particleAdd(NB_particle p_particle)
		{
			particles[vecToString(p_particle.position)] = p_particle;
			update_list.Add(p_particle);
		}
		public List<NB_particle> getUpdatedParticles()
		{
			return update_list;
		}
		public NB_particle getParticle(Vector2I p_check_position)
		{
			// GD.Print(p_check_position, global_position);
			return particles[vecToString(p_check_position - global_position)];
		}
		public string fakeVecToString(int X, int Y) => X + "," + Y;
		public string vecToString(Vector2I p_vec) => p_vec.X + "," + p_vec.Y;
	}
	//Variables
	Dictionary<String, NB_particle> particle_list;
	Vector2I chunk_size = new Vector2I(100,100);
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
		particle_list.Add("Sand", new NB_particle(
			NB_type.SOLID,
			true,
			[
				new Vector2I(0,1),
				new Vector2I(1,1),
				new Vector2I(-1,1),
			]
		));
	}
	
	//Ready
	public override void _Ready()
	{
		chunks.Add(fakeVecToString(0,0), new NB_chunk(new Vector2I(0,0), chunk_size));
		chunks_update_list.Add(new Vector2I(0,0));
		//TODO add so you can add multiple particles aka particlesAdd
		chunks[fakeVecToString(0,0)].particleAdd(
			createParticle(new Vector2I(0,0), "Sand")
		);
	}
	public override void _Process(double delta)
	{
		simulationStep();
		visualiser();
	}
	public void simulationStep()
	{
		foreach (Vector2I chunk_update_coord in chunks_update_list)
		{
			foreach (NB_particle particle in chunks[vecToString(chunk_update_coord)].getUpdatedParticles())
			{
				foreach (Vector2I check_offset in particle.checking_pos)
				{
					if (check_pixel(check_offset, particle)) {
						break;
					}
				}
			}
		}
	}
	private bool check_pixel(Vector2I check_offset, NB_particle p_particle)
	{
		Vector2I check_position = p_particle.position + check_offset;
		NB_chunk indexed_chunk = vecToChunk(check_position);
		NB_particle returned_particle =  indexed_chunk.getParticle(check_position);
		if (p_particle.type == NB_type.FALLING)
		{
			if (
				returned_particle.type == NB_type.LIQUID ||
				returned_particle.type == NB_type.GAS ||
				returned_particle.type == NB_type.PLASMA
			)
			{
				
			}
		}
		else if (p_particle.type == NB_type.LIQUID)
		{
			if (
				returned_particle.type == NB_type.GAS ||
				returned_particle.type == NB_type.PLASMA
			)
			{
				
			}
		}
		else if (p_particle.type == NB_type.GAS)
		{
			if (returned_particle.type == NB_type.PLASMA)
			{
				
			}
		}
		return false;
	} 
	private NB_chunk vecToChunk(Vector2I p_position)
	{
		Vector2I position = new Vector2I((int)Math.Floor((double)p_position.X/(double)chunk_size.X), (int)Math.Floor((double)p_position.Y/(double)chunk_size.Y));
		string key = vecToString(position);
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
			foreach (NB_particle particle in chunks[vecToString(chunk_update_coord)].getUpdatedParticles())
			{
				SetCell(particle.position, 0, particle.color);
			}
		}
	}
	private NB_particle createParticle(Vector2I p_positio, string type)
	{
		NB_particle return_particle = particle_list[type].pos(p_positio);
		return_particle.color = new Vector2I(14,0);
		return return_particle;
	}
	public void placeParticle()
	{
		
	}
	//Stringies ;PP
    public string fakeVecToString(int X, int Y) => X + "," + Y;
    public string vecToString(Vector2I p_vec) => p_vec.X + "," + p_vec.Y;
}
