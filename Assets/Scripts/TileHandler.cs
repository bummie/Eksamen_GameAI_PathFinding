using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHandler : MonoBehaviour 
{

	public Hashtable OccupiedTiles { get; private set; }
	void Start () 
	{
		OccupiedTiles = new Hashtable();
	}

	/// <summary>
	/// Returnst true if the given tile is already occupied by another tile
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	public bool IsTileOccupied(Vector2 tile)
	{
		return OccupiedTiles.ContainsKey(tile) ? true : false;
	}

	/// <summary>
	/// Returns the closest tile to a given posistion
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	public Vector2 ClosestTile(Vector3 position)
	{
		return new Vector2(Mathf.Round(position.x), Mathf.Round(position.z));
	}

	/// <summary>
	/// Adds given tile to OccupiedTiles table
	/// </summary>
	public void AddTile(Vector2 tile, GameObject obstacle)
	{
		if(IsTileOccupied(tile))
		{
			return;
		}

		OccupiedTiles.Add(tile, obstacle);
	}

	/// <summary>
	/// Removes given tile from occupied table
	/// </summary>
	/// <param name="tile"></param>
	public void RemoveTile(Vector2 tile)
	{
		if(IsTileOccupied(tile))
		{
			Destroy(OccupiedTiles[tile] as GameObject);
			OccupiedTiles.Remove(tile);
		}
	}
}
