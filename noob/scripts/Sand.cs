using Godot;
using System;
using System.Collections.Generic;

public partial class Sand : TileMapLayer
{
	// structs
	enum NB_type
	{
		PLASMA,
		GAS,
		LIQUID,
		SOLID,
	}
	
	class NB_particle
	{
		public NB_particle (
			NB_type p_type,
			bool p_solid
		)
		{
			type = p_type;
			solid = p_solid;
		}
		public NB_type type;
		public bool solid;
		public Vector2I position;
		public NB_particle pos(Vector2I p_position)
		{
			position = p_position;
			return this;
		}
	}
	class NB_chunk
	{
		public NB_chunk(Vector2I p_position)
		{
			position = p_position;
			particles = new List<NB_particle>{};
			update_list = new List<NB_particle>{};
		}
		public Vector2I position;
		public List<NB_particle> particles;
		public List<NB_particle> update_list;
		public void particleAdd(NB_particle p_particle)
		{
			particles.Add(p_particle);
			update_list.Add(p_particle);
		}
		public List<NB_particle> getUpdatedParticles()
		{
			return update_list;
		}
		public String fakeVecToString(int X, int Y)
		{
			return X + "," + Y;
		}
		public String vecToString(Vector2I p_vec)
		{
			return p_vec.X + "," + p_vec.Y;
		}
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
			true
		));
	}
	//Ready
	public override void _Ready()
	{
		chunks.Add(fakeVecToString(0,0), new NB_chunk(new Vector2I(0,0)));
		chunks_update_list.Add(new Vector2I(0,0));
		//TODO add so you can add multiple particles aka particlesAdd
		chunks[fakeVecToString(0,0)].particleAdd(
			particle_list["Sand"].pos(new Vector2I(0,0))
		);
		visualiser();
	}

	public override void _Process(double delta)
	{
		
	}
	public void simulationStep()
	{
		
	}
	public void visualiser()
	{
		foreach (Vector2I chunk_update_coord in chunks_update_list)
		{
			foreach (NB_particle particle in chunks[vecToString(chunk_update_coord)].getUpdatedParticles())
			{
				SetCell(particle.position, 0, new Vector2I(14,0));
			}
		}
	}
	public String fakeVecToString(int X, int Y)
	{
		return X + "," + Y;
	}
	public String vecToString(Vector2I p_vec)
	{
		return p_vec.X + "," + p_vec.Y;
	}
}
