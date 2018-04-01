using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
	private const float MOVECOST = 1;
	private const float MOVECOST_DIAGONAL = 1.4f;

	private MapEditor _mapEditor;
	private TileHandler _tileHandler;

	private ArrayList _outerNodes;
	private ArrayList _innerNodes;
	
	void Start()
	{
		_mapEditor = GetComponent<MapEditor>();
		_tileHandler = GetComponent<TileHandler>();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			Node[] path = CalculatePath();
		
			_mapEditor.Player.GetComponent<PlayerMove>().MovePath = path;
			_mapEditor.Player.GetComponent<PlayerMove>().ShouldMove = true;
		}
	}

	/// <summary>
	/// Calcuates path from player to goal
	/// Returns the path as an array of Nodes
	/// </summary>
	/// <returns>Node[]</returns>
	public Node[] CalculatePath()
	{
		_outerNodes = new ArrayList();
		_innerNodes = new ArrayList();
		int currentNodeIndex;
		Vector2 goalTile = _tileHandler.ClosestTile(_mapEditor.Goal.transform.position);

		// Add the starting node
		Vector2 playerTile = _tileHandler.ClosestTile(_mapEditor.Player.transform.position);
		Node playerNode = new Node(playerTile, null, 0, CalculateManhatten(playerTile));
		_outerNodes.Add(playerNode);

		while(_outerNodes.Count > 0)
		{
			currentNodeIndex = FindBestScoringNode(_outerNodes);
			_innerNodes.Add(_outerNodes[currentNodeIndex]);
			_outerNodes.RemoveAt(currentNodeIndex);

			Node currentNode = _innerNodes[_innerNodes.Count - 1] as Node;
			
			if(currentNode.Tile == goalTile)
			{
				// TODO: We've found the path we're lookin for boys
				Debug.Log("Found path: " + _innerNodes.Count);
				
				break;
			}

			Node[] currentNodeNeigbours = FetchNeighbours(currentNode);
			foreach(Node neighbour in currentNodeNeigbours)
			{
				if(IsAlreadyOuterNode(neighbour))
				{

				}else
				{
					_outerNodes.Add(neighbour);
				}
			}
		}
		
		//TODO: Trekke ut til egen metode
		List<Node> bestPath  = new List<Node>();

		bool foundStartNode = false;
		Node lastAdded = null;
		while(!foundStartNode)
		{
			if(lastAdded == null)
			{
				lastAdded = _innerNodes[_innerNodes.Count - 1] as Node;
				bestPath.Add(lastAdded);
				continue;
			}

			if(lastAdded.Parent != null)
			{
				lastAdded = lastAdded.Parent;
				bestPath.Add(lastAdded);
			}else
			{
				foundStartNode = true;
			}
		}
		
		bestPath.Reverse();
		return bestPath.ToArray();
	}


	/// <summary>
	/// Returns whether the given Node is
	/// in the OuterNodeArraylist or not
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	private bool IsAlreadyOuterNode(Node node)
	{
		foreach(Node n in _outerNodes)
		{
			if(n.Tile == node.Tile)
			{ return true; }
		}

		return false;
	}
	/// <summary>
	/// Takes a node and finds its neighbouring nodes that are valid
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	private Node[] FetchNeighbours(Node node)
	{
		List<Node> neighbourList = new List<Node>();
		Vector2 neighbourTile;
		Node neighbourNode;

		// [ -1, 1 ]
		neighbourTile = node.Tile;	
		neighbourTile.x -= 1;
		neighbourTile.y += 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST_DIAGONAL, CalculateManhatten(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ 0, 1 ]
		neighbourTile = node.Tile;	
		neighbourTile.y += 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST, CalculateManhatten(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ 1, 1 ]
		neighbourTile = node.Tile;	
		neighbourTile.x += 1;
		neighbourTile.y += 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST_DIAGONAL, CalculateManhatten(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ -1, 0 ]
		neighbourTile = node.Tile;	
		neighbourTile.x -= 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST, CalculateManhatten(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ 1, 0 ]
		neighbourTile = node.Tile;	
		neighbourTile.x += 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST, CalculateManhatten(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ -1, -1 ]
		neighbourTile = node.Tile;	
		neighbourTile.x -= 1;
		neighbourTile.y -= 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST_DIAGONAL, CalculateManhatten(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ 0, -1 ]
		neighbourTile = node.Tile;	
		neighbourTile.y -= 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST, CalculateManhatten(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ 1, -1 ]
		neighbourTile = node.Tile;	
		neighbourTile.x += 1;
		neighbourTile.y -= 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST_DIAGONAL, CalculateManhatten(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		return neighbourList.ToArray();
	}

	/// <summary>
	/// Returns whether or not a node is valid
	/// A node is not valid if already in the open list
	/// or if the node is an obstacle
	/// </summary>
	/// <returns></returns>
	private bool IsTileValid(Vector2 tile)
	{	
		// is node an obstacle?
		if(_tileHandler.IsTileOccupied(tile))
		{ return false; }

		// Is node in outerNodelist?
		foreach(Node n in _innerNodes)
		{
			if(tile == n.Tile)
			{ return false; }
		}

		return true;
	}

	/// <summary>
	/// Finds the index of the node with the best total score
	/// </summary>
	/// <param name="nodeList"></param>
	/// <returns></returns>
	private int FindBestScoringNode(ArrayList nodeList)
	{
		int bestNodeIndex = 0;
		float bestNodeScore = -1;
		for(int i = 0; i < nodeList.Count - 1; i++)
		{				
			Node listNode = nodeList[i] as Node;

			if((bestNodeScore == -1) || listNode.TotalScore < bestNodeScore )
			{
				bestNodeIndex = i;
				bestNodeScore = listNode.TotalScore;
			}
		}
		return bestNodeIndex;
	}

	/// <summary>
	/// Returns the manhatten distance from given tile to goal
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	private float CalculateManhatten(Vector2 tile)
	{
		return Mathf.Abs(tile.x - _mapEditor.Goal.transform.position.x) + Mathf.Abs(tile.y - _mapEditor.Goal.transform.position.z);
	}
}
