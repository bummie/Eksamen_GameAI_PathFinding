using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour 
{
	public Text textStatus, textMode;

	/// <summary>
	/// Updates the UI text in status
	/// </summary>
	/// <param name="text"></param>
	public void UpdateStatus(string text)
	{
		textStatus.text = text;
	}

	/// <summary>
	/// Updates the UI text in Mode
	/// </summary>
	/// <param name="text"></param>
	public void UpdateMode(string text)
	{
		textMode.text = text;
	}
}
