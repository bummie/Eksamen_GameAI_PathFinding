using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour 
{
	public Text textStatus, textMode;
	private bool _textChanged = false;
	private string _newTextStatus = "";
	private string _newTextMode = "";

	void Update()
	{
		if(_textChanged)
		{
			textStatus.text = _newTextStatus;
			textMode.text = _newTextMode;

			_textChanged = false;
		}
	}

	/// <summary>
	/// Updates the UI text in status
	/// </summary>
	/// <param name="text"></param>
	public void UpdateStatus(string text)
	{
		_newTextStatus = text;
		_textChanged = true;
	}

	/// <summary>
	/// Updates the UI text in Mode
	/// </summary>
	/// <param name="text"></param>
	public void UpdateMode(string text)
	{
		_newTextMode = text;
		_textChanged = true;
	}
}
