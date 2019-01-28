using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Waypoint {
	public Vector2 position;

	public Waypoint(Vector2 pos){
		this.position = pos;
	}
}
