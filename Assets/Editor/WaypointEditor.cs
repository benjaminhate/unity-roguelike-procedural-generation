using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (Waypoint))]
public class WaypointEditor : Editor {

	void OnSceneGUI(){
		Waypoint w = (Waypoint)target;
		Handles.color = Color.cyan;
		Handles.DrawWireArc (w.transform.position, Vector3.forward, Vector3.right, 360, .25f);
	}

}
