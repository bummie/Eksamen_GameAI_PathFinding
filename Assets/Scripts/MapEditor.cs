using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditor : MonoBehaviour
{

	private TileHandler _tileHandler;

	public GameObject Obstacle;
	public GameObject ObstacleParent;

	private Plane _mapPlane;
	private Ray _screenToWorldRay;
	private float _distance;
	private Vector2 _lastEditedTile = Vector2.zero;
	void Start () 
	{
		_mapPlane = new Plane(Vector3.up, Vector3.zero);
		_tileHandler = GetComponent<TileHandler>();
	}
	
	void Update () 
	{
		//Left mouse button clicked
        if (Input.GetMouseButton(0))
		{
			EditObstacle();
		}
	}

	/// <summary>
	/// Adds or removes obstacle from tile where mouse clicked
	/// </summary>
	private void EditObstacle()
	{
		Vector2 tile = MousePositionToTile();
		
		if(tile == _lastEditedTile)
		{
			return;
		}

		if(_tileHandler.IsTileOccupied(tile))
		{
			_tileHandler.RemoveTile(tile);
		}else
		{	
			_tileHandler.AddTile(tile, CreateObstacle(tile));
		}

		_lastEditedTile = tile;
	}

	/// <summary>
	/// Returns the closest tile from mouse position
	/// </summary>
	/// <returns></returns>
	private Vector2 MousePositionToTile()
	{
		_screenToWorldRay = Camera.main.ScreenPointToRay(Input.mousePosition);

		if(_mapPlane.Raycast(_screenToWorldRay, out _distance))
		{
			Vector3 position = _screenToWorldRay.GetPoint(_distance);
			return _tileHandler.ClosestTile(position);
		}
		return Vector2.zero; //TODO: Throw exception
	}

	/// <summary>
	/// Creates an obstacle gameobject
	/// </summary>
	/// <returns></returns>
	private GameObject CreateObstacle(Vector2 tile)
	{
		GameObject obstacle = Instantiate(Obstacle, new Vector3(tile.x, -.5f, tile.y), Quaternion.identity);
		obstacle.transform.SetParent(ObstacleParent.transform);
		obstacle.name = "Obstacle";
		return obstacle;
	}
}
