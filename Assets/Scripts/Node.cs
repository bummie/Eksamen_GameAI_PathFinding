using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Node 
{
	public Vector2 Tile { get; private set; }
	public Node Parent { get; private set; }
	public float MoveCost { get; private set; }
	public float HeuristicCost { get; private set; }
	public float TotalScore { get; private set; }

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
		return Tile.ToString() + ": " + "Parent: " + Parent.Tile.ToString() + " MoveCost: " + MoveCost + " HeuristicCost: " + HeuristicCost + " TotalCost: " + TotalScore;
	}
}
