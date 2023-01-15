using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsefulVariableClass : MonoBehaviour
{
	public float Width
	{
		get
		{
			return Camera.main.orthographicSize * Camera.main.aspect * 2;
		}
	}

	public float Height
	{
		get
		{
			return Camera.main.orthographicSize * 2;
		}
	}

	public float MouseX
	{
		get
		{
			return Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
		}
	}

	public float MouseY
	{
		get
		{
			return Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
		}
	}
}
