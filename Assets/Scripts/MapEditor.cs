﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
	private enum EditorMode{AddTile, RemoveTile, Move};
	private TileHandler _tileHandler;

	public GameObject Obstacle;
	public GameObject ObstacleParent;

	private EditorMode _mode;
	private Plane _mapPlane;
	private Ray _screenToWorldRay;
	private float _distance;
	private Vector2 _lastEditedTile = Vector2.zero;
	void Start () 
	{
		_mapPlane = new Plane(Vector3.up, Vector3.zero);
		_tileHandler = GetComponent<TileHandler>();

		_mode = EditorMode.AddTile;
	}
	
	void Update () 
	{
		//Left mouse button clicked
        if (Input.GetMouseButton(0))
		{
			switch(_mode)
			{
				case EditorMode.AddTile:
					AddObstacle();
				break;

				case EditorMode.RemoveTile:
					RemoveObstacle();
				break;

				case EditorMode.Move:

				break;
				
			}
		}

		if (Input.GetMouseButtonDown(1))
		{
			SwapMode();
			Debug.Log("Swap");
		}
	}

	/// <summary>
	/// Adds obstacle from tile where mouse clicked
	/// </summary>
	private void AddObstacle()
	{
		Vector2 tile = MousePositionToTile();
		
		if(_tileHandler.IsTileOccupied(tile))
		{
			return;
		}

		_tileHandler.AddTile(tile, CreateObstacle(tile));
	}

	/// <summary>
	/// Removes obstacle from tile where mouse clicked
	/// </summary>
	private void RemoveObstacle()
	{
		Vector2 tile = MousePositionToTile();

		if(_tileHandler.IsTileOccupied(tile))
		{
			_tileHandler.RemoveTile(tile);
		}
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

	/// <summary>
	/// Changes the editormode to the next in queue
	/// </summary>
	private void SwapMode()
	{
		_mode++;

		if(_mode > EditorMode.Move)
		{
			_mode = EditorMode.AddTile;
		}
	}
}
