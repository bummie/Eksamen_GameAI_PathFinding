using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

	public bool ShouldMove { get; set; }
	public Node[] MovePath { get; set; }
	private int _currentPathIndex = 0;

	public float PlayerSpeed = 1f;

	private Vector3 _targetPosition;
	void Start () 
	{
		ShouldMove = false;
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
			{ ShouldMove = false;}

			_targetPosition = new Vector3(MovePath[_currentPathIndex].Tile.x, -.5f, MovePath[_currentPathIndex].Tile.y);

			transform.position = Vector3.MoveTowards(transform.position, _targetPosition, PlayerSpeed * Time.deltaTime);

			if(Vector3.Distance(transform.position, _targetPosition) < .5f)
			{
				_currentPathIndex++;
			}
    	}
	}
}