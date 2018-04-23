using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading; 

public class PathFinding : MonoBehaviour
{
	#region Consts
	private const float MOVECOST = 1;
	private const float MOVECOST_DIAGONAL = 1.4f;
	private const int MAP_LIMIT = 50;
	#endregion

	#region Editor Values
	public bool CutCorners = true;
	public int PathFindingDelay = 10;
	public bool DisplayOuterNodes = true;
	public bool DisplayInnerNodes = true;
	
	#endregion
	
	#region Components
	private MapEditor _mapEditor;
	private TileHandler _tileHandler;
	private UIHandler _uiHandler;
	private PlayerMove _playerMove;
	#endregion

	#region Node Array
	private ArrayList _outerNodes;
	private ArrayList _innerNodes;
	#endregion

	#region Timer
	private float _timeStarted = 0;
	private float _timeTaken = 0;
	private bool _timerRunning = false;
	#endregion

	#region Threading
	private Thread _pathFindingThread;
	private bool _threadRunning = false;
	private bool _killThread = false;
	#endregion
	private Vector3 _playerPos, _goalPos;
	
	void Start()
	{
		_mapEditor = GetComponent<MapEditor>();
		_tileHandler = GetComponent<TileHandler>();
		_uiHandler = GetComponent<UIHandler>();
		_playerMove = _mapEditor.Player.GetComponent<PlayerMove>();
	}

	void Update()
	{
		// When space is pressed calculate new path
		if(Input.GetKeyDown(KeyCode.Space))
		{
			_playerPos = _mapEditor.Player.transform.position;
			_goalPos = _mapEditor.Goal.transform.position;
			
			if(_threadRunning)
			{
				_killThread = true;
			}
			_pathFindingThread = new Thread(CalculatePath);
			_pathFindingThread.Start();

			_uiHandler.UpdateStatus("Calculating, lastTime: " + _timeTaken + "ms");
			_timeStarted = Time.time * 1000;
			_timerRunning = true;
		}

		if(!_threadRunning && _timerRunning)
		{
			_timeTaken = (Time.time * 1000) - _timeStarted;
			_uiHandler.UpdateStatus("Done: " + _timeTaken + "ms");
			_timerRunning = false;
		}
	}

	#region Path Finding Specific Methods
	/// <summary>
	/// Calcuates path from player to goal
	/// Returns the path as an array of Nodes
	/// </summary>
	/// <returns>Node[]</returns>
	private void CalculatePath()
	{	
		_threadRunning = true;

		_outerNodes = new ArrayList();
		_innerNodes = new ArrayList();
		
		int currentNodeIndex;
		Vector2 goalTile = _tileHandler.ClosestTile(_goalPos);

		// Add the starting node
		Vector2 playerTile = _tileHandler.ClosestTile(_playerPos);
		Node playerNode = new Node(playerTile, null, 0, CalculateEuclidian(playerTile));
		_outerNodes.Add(playerNode);
		bool _pathFound = false;
		while(_outerNodes.Count > 0)
		{
			if(_killThread)
			{
				_playerMove.MovePath = null;
				_playerMove.ShouldMove = false;
				_killThread = false;
				return;
			}

			Thread.Sleep(PathFindingDelay);
			
			currentNodeIndex = FindBestScoringNode(_outerNodes);
			_innerNodes.Add(_outerNodes[currentNodeIndex]);
			_outerNodes.RemoveAt(currentNodeIndex);

			Node currentNode = _innerNodes[_innerNodes.Count - 1] as Node;
			
			if(currentNode.Tile == goalTile)
			{
				_pathFound = true;
				break;
			}

			Node[] currentNodeNeighbours = FetchNeighbours(currentNode);
			foreach(Node neighbour in currentNodeNeighbours)
			{
				if(!IsAlreadyOuterNode(neighbour))
				{ _outerNodes.Add(neighbour); continue;}

				Node outerNode = FetchOuterNodeAtTile(neighbour.Tile);
				float newCost = currentNode.MoveCost + CalculateEuclidian(currentNode.Tile, outerNode.Tile);
				if(outerNode.MoveCost <= (newCost))
				{ continue; }

				outerNode.Parent = currentNode;
				outerNode.MoveCost = newCost;
			}
		}

		if(_pathFound)
		{
			_playerMove.MovePath = TraverseToFindPath();
			_playerMove.ShouldMove = true;
		}
		_threadRunning = false;
	}

	/// <summary>
	/// Traveres the inner nodes backwards to find the best path to the goal
	/// </summary>
	/// <returns></returns>
	private Node[] TraverseToFindPath()
	{
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
	/// Returns node in outerNodes from given tile
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	private Node FetchOuterNodeAtTile(Vector2 tile)
	{
		foreach(Node n in _outerNodes)
		{
			if(n.Tile == tile)
			{ return n; }
		}

		return null; 
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
		
		if(IsTileValid(neighbourTile) && CutCorners)
		{
			// Check if sides are valid so the player cant squeese through
			if(IsTileValid(new Vector2(node.Tile.x - 1, node.Tile.y)) || IsTileValid(new Vector2(node.Tile.x, node.Tile.y + 1)))
			{
				neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST_DIAGONAL, CalculateEuclidian(neighbourTile));
				neighbourList.Add(neighbourNode);
			}
		}

		// [ 0, 1 ]
		neighbourTile = node.Tile;	
		neighbourTile.y += 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST, CalculateEuclidian(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ 1, 1 ]
		neighbourTile = node.Tile;	
		neighbourTile.x += 1;
		neighbourTile.y += 1;
		
		if(IsTileValid(neighbourTile) && CutCorners)
		{
			if(IsTileValid(new Vector2(node.Tile.x, node.Tile.y + 1)) || IsTileValid(new Vector2(node.Tile.x + 1, node.Tile.y)))
			{
				neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST_DIAGONAL, CalculateEuclidian(neighbourTile));
				neighbourList.Add(neighbourNode);
			}
		}

		// [ -1, 0 ]
		neighbourTile = node.Tile;	
		neighbourTile.x -= 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST, CalculateEuclidian(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ 1, 0 ]
		neighbourTile = node.Tile;	
		neighbourTile.x += 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST, CalculateEuclidian(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ -1, -1 ]
		neighbourTile = node.Tile;	
		neighbourTile.x -= 1;
		neighbourTile.y -= 1;
		
		if(IsTileValid(neighbourTile) && CutCorners)
		{
			if(IsTileValid(new Vector2(node.Tile.x - 1, node.Tile.y)) || IsTileValid(new Vector2(node.Tile.x, node.Tile.y - 1)))
			{
				neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST_DIAGONAL, CalculateEuclidian(neighbourTile));
				neighbourList.Add(neighbourNode);
			}
		}

		// [ 0, -1 ]
		neighbourTile = node.Tile;	
		neighbourTile.y -= 1;
		
		if(IsTileValid(neighbourTile))
		{
			neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST, CalculateEuclidian(neighbourTile));
			neighbourList.Add(neighbourNode);
		}

		// [ 1, -1 ]
		neighbourTile = node.Tile;	
		neighbourTile.x += 1;
		neighbourTile.y -= 1;
		
		if(IsTileValid(neighbourTile) && CutCorners)
		{
			if(IsTileValid(new Vector2(node.Tile.x, node.Tile.y - 1)) || IsTileValid(new Vector2(node.Tile.x + 1, node.Tile.y)))
			{
				neighbourNode = new Node(neighbourTile, node, node.MoveCost + MOVECOST_DIAGONAL, CalculateEuclidian(neighbourTile));
				neighbourList.Add(neighbourNode);
			}
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
		// Map bounderies
		if(tile.x >= MAP_LIMIT || tile.x <= -MAP_LIMIT || tile.y >= MAP_LIMIT || tile.y <= -MAP_LIMIT )
		{ return false; }

		// is node an obstacle?
		if(_tileHandler.IsTileOccupied(tile))
		{ return false; }

		// Is node in innerNodelist?
		foreach(Node n in _innerNodes)
		{
			if(tile == n.Tile)
			{ return false; }
		}

		return true;
	}

	/// <summary>
	/// Finds the index of the node with the best total score
	/// TODO: Switch to a priority queue
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
		return Mathf.Abs(tile.x - _goalPos.x) + Mathf.Abs(tile.y - _goalPos.z);
	}

	/// <summary>
	/// Calculates the eucdlian distance from a tile to goal
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	private float CalculateEuclidian(Vector2 tile)
	{
		return Mathf.Sqrt(Mathf.Pow((tile.x - _goalPos.x), 2f) + Mathf.Pow((tile.y - _goalPos.z), 2f));
	}

	/// <summary>
	/// Calculates the eucdlian distance from a tile to another tile
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	private float CalculateEuclidian(Vector2 tileStart, Vector2 tileEnd)
	{
		return Mathf.Sqrt(Mathf.Pow((tileStart.x - tileEnd.x), 2f) + Mathf.Pow((tileStart.y - tileEnd.y), 2f));
	}
	#endregion

	/// <summary>
	/// Visualizating the outer and inner nodes
	/// </summary>
	void OnDrawGizmos() 
	{
		if(_outerNodes  != null && DisplayOuterNodes)
		{
			Gizmos.color = Color.yellow;
			try
			{
				Node[] tempArray = _outerNodes.ToArray(typeof(Node)) as Node[];
				foreach(Node n in tempArray)
				{
					if(n != null)
					{Gizmos.DrawCube(new Vector3(n.Tile.x, 0, n.Tile.y), new Vector3(.5f, .1f, .5f));}
				}
			}catch(System.ArgumentException e)
			{
				Debug.Log("Gizmo display array copy failed");
			}
		}
        
		if(_outerNodes  != null && DisplayInnerNodes)
		{
			Gizmos.color = Color.magenta;
			try
			{
				Node[] tempArray = _innerNodes.ToArray(typeof(Node)) as Node[];
				foreach(Node n in tempArray)
				{
					if(n != null)
					{Gizmos.DrawCube(new Vector3(n.Tile.x, 0, n.Tile.y),new Vector3(.5f, .1f, .5f));}
				}
			}catch(System.ArgumentException e)
			{
				Debug.Log("Gizmo display array copy failed");
			}
		}
    }
}
