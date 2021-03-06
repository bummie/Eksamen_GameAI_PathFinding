﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
	private enum EditorMode{ AddTile, RemoveTile, Move };
	private TileHandler _tileHandler;
	private Camera _camera;

	#region Public Editor
	public GameObject Player;
	public GameObject Goal;
	public GameObject Obstacle;
	public GameObject ObstacleParent;
	
	#endregion

	#region private fields

	private EditorMode _mode;
	private Plane _mapPlane;
	private Ray _screenToWorldRay;
	private float _distance;
	private bool _isMoving = false;
	private GameObject _movingObject = null;
	#endregion
	void Start () 
	{
		_mapPlane = new Plane(Vector3.up, Vector3.zero);
		_tileHandler = GetComponent<TileHandler>();
		_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

		_mode = EditorMode.AddTile;
	}
	
	void Update () 
	{
		MapZoom();

		MoveMap();

		HandleModeAction();

		SwapMode();
	}

	/// <summary>
	/// Handles scroll zoom
	/// </summary>
	private void MapZoom()
	{
		float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
		if (mouseScroll > 0f)
		{	
			if(_camera.orthographicSize > 15)
			{
				_camera.orthographicSize--;
			}
			
		}
		else if(mouseScroll < 0f)
		{
			if(_camera.orthographicSize < 60)
			{
				_camera.orthographicSize++;
			}
		}
	}

	/// <summary>
	/// Moves the map by usning the WASD keys
	/// </summary>
	private void MoveMap()
	{
		if(Input.GetKey(KeyCode.W))
		{	
			Vector3 camPos = _camera.transform.position;
			camPos.z++;
			_camera.transform.position = camPos;
		}

		if(Input.GetKey(KeyCode.S))
		{	
			Vector3 camPos = _camera.transform.position;
			camPos.z--;
			_camera.transform.position = camPos;
		}

		if(Input.GetKey(KeyCode.A))
		{	
			Vector3 camPos = _camera.transform.position;
			camPos.x--;
			_camera.transform.position = camPos;
		}

		if(Input.GetKey(KeyCode.D))
		{	
			Vector3 camPos = _camera.transform.position;
			camPos.x++;
			_camera.transform.position = camPos;
		}
	}

	/// <summary>
	/// Adds obstacle from tile where mouse clicked
	/// </summary>
	private void AddObstacle()
	{
		Vector2 tile = MousePositionToTile();
		
		// If tile occupied by obstacle already
		if(_tileHandler.IsTileOccupied(tile))
		{ return; }

		// If space is occupied by player og goal
		if((_tileHandler.ClosestTile(Player.transform.position) == tile) || (_tileHandler.ClosestTile(Goal.transform.position) == tile))
		{ return; }
		
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
		return Vector2.zero; //TODO: Throw exception?
	}

	/// <summary>
	/// Returns the object clicked on
	/// </summary>
	/// <returns></returns>
	private GameObject FetchClickedObject()
	{
		RaycastHit hitObject; 
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
		
		if ( Physics.Raycast (ray, out hitObject, 100.0f))
		{
			return hitObject.transform.gameObject;
		}

		return null;
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

	private void HandleModeAction()
	{
		// Left mouse button down
		if (Input.GetMouseButtonDown(0))
		{	
			if(_mode == EditorMode.Move)
			{
				GameObject clickedObject = FetchClickedObject();
				
				if(clickedObject.tag == "Moveable")
				{
					_isMoving = true;
					_movingObject = clickedObject;
					Player.GetComponent<PlayerMove>().ShouldMove = false;
				}
			}
		}

		//Left mouse button held down
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
					if(_isMoving)
					{	
						Vector2 tilePosition = MousePositionToTile();
						_movingObject.transform.position = new Vector3(tilePosition.x, -.5f, tilePosition.y);
					}
				break;
			}
			
			Player.GetComponent<PlayerMove>().ShouldMove = false;
		}

		// Left mouse button up
		if (Input.GetMouseButtonUp(0))
		{
			_isMoving = false;
			_movingObject = null;
		}
	}

	/// <summary>
	/// Changes the editormode to the next in queue
	/// </summary>
	private void SwapMode()
	{
		if (Input.GetMouseButtonDown(1))
		{
			_mode++;

			if(_mode > EditorMode.Move)
			{
				_mode = EditorMode.AddTile;
			}

			GetComponent<UIHandler>().UpdateMode("Mode: " + _mode.ToString());
		}
	}
}
