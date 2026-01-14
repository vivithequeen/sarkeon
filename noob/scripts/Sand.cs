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
			bool p_solid,
			List<Vector2I> p_checking_pos
		)
		{
			type = p_type;
			solid = p_solid;
			checking_pos = p_checking_pos;
		}
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
					if (){break}
				}
			}
		}
	}
	private bool check_pixel(Vector2I check_offset, NB_particle p_particle)
	{
		return false
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
		NB_particle return_particle = particle_list["Sand"].pos(new Vector2I(0,0));
		return_particle.color = new Vector2I(14,0);
		return return_particle;
	}
	//Stringies ;PP
    public string fakeVecToString(int X, int Y) => X + "," + Y;
    public string vecToString(Vector2I p_vec) => p_vec.X + "," + p_vec.Y;
}
