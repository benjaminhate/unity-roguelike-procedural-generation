using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class MainCameraController : MonoBehaviour {

	public GameObject target;

	void Start(){
		FieldOfView fov = target.GetComponentInChildren<FieldOfView> ();
		if (fov) {
			float size = fov.viewRadius;
			GetComponent<Camera> ().orthographicSize = size;
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(
			target.transform.position.x,
			target.transform.position.y,
			transform.position.z);
	}
}
