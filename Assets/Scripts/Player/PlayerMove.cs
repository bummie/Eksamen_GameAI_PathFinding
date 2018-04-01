using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

	public bool ShouldMove { get; set; }

	private Node[] _movePath;
	public Node[] MovePath
	{
		get
		{
			return _movePath;
		}

		set
		{
			_movePath = value;
			_currentPathIndex = 0;
		} 
	}
	private int _currentPathIndex = 0;

	public float PlayerSpeed = 1f;

	private Vector3 _targetPosition;
	void Start () 
	{
		ShouldMove = false;
		MovePath = null;
	}
	
	void Update ()
	{
		MovePlayer();
	}

	/// <summary>
	/// Moves the player through the given path
	/// </summary>
	private void MovePlayer()
	{
		if(ShouldMove)
		{
			if(_currentPathIndex == MovePath.Length)
			{ ShouldMove = false; return; }

			_targetPosition = new Vector3(MovePath[_currentPathIndex].Tile.x, -.5f, MovePath[_currentPathIndex].Tile.y);

			transform.position = Vector3.MoveTowards(transform.position, _targetPosition, PlayerSpeed * Time.deltaTime);

			if(Vector3.Distance(transform.position, _targetPosition) < .5f)
			{
				_currentPathIndex++;
			}
    	}
	}

	/// <summary>
	/// Draw path
	/// </summary>
	void OnDrawGizmos() 
	{
        if (MovePath != null)
		{
            Gizmos.color = Color.cyan;
			for(int i = 0; i < MovePath.Length-1; i++)
			{
            	Gizmos.DrawLine(new Vector3(MovePath[i].Tile.x, 1f, MovePath[i].Tile.y), new Vector3(MovePath[i+1].Tile.x, 1f, MovePath[i+1].Tile.y));
			}
        }
    }
}