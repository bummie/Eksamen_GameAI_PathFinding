using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Node 
{
	#region Node Properties
	public Vector2 Tile { get; private set; }
	public Node Parent { get; set; }
	private float _moveCost;
	public float MoveCost
	{
		get{ return _moveCost; }
		set
		{
			_moveCost = value;
			TotalScore = MoveCost + HeuristicCost;
		}
	}
	private float _heuristicCost;
	public float HeuristicCost
	{
		get{ return _heuristicCost; }
		set
		{
			_heuristicCost = value;
			TotalScore = MoveCost + HeuristicCost;
		}
	}

	public float TotalScore { get; private set; }
	#endregion
	
	/// <summary>
	/// Node Constructor
	/// </summary>
	/// <param name="tile"></param>
	/// <param name="parent"></param>
	/// <param name="moveCost"></param>
	/// <param name="heuristicCost"></param>
	public Node(Vector2 tile, Node parent, float moveCost, float heuristicCost)
	{
		Tile = tile;
		Parent = parent;
		MoveCost = moveCost;
		HeuristicCost = heuristicCost;
		TotalScore = MoveCost + HeuristicCost;
	}

	/// <summary>
	/// Returns a string with info about the Node
	/// </summary>
	/// <returns></returns>
	public override string ToString()
	{
		string parentTile = Parent == null ? "Null" : Parent.Tile.ToString();
		return Tile + ": " + "Parent: " + parentTile + " MoveCost: " + MoveCost + " HeuristicCost: " + HeuristicCost + " TotalCost: " + TotalScore;
	}
}
