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
	}
	Dictionary<String, NB_particle> particle_list;
	//Variables
	Vector2I chunk_size = new Vector2I(100,100);
	Dictionary<Vector2I, Dictionary<Vector2I, NB_particle>> chunks;
	//Init sands
	public Sand()
	{
		//init dicts
		particle_list = new Dictionary<String, NB_particle>{};
		chunks = new Dictionary<Vector2I, Dictionary<Vector2I, NB_particle>>{};
		//init all sand data
		particle_list.Add("Sand", new NB_particle(
			NB_type.SOLID,
			true
		));
	}
	//Ready
	public override void _Ready()
	{
		chunks.Add(new Vector2I(0,0), new Dictionary<Vector2I, NB_particle>{});
		GD.Print(chunks);
	}

	public override void _Process(double delta)
	{
	}
}
