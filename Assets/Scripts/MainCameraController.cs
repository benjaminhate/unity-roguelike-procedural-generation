using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class MainCameraController : MonoBehaviour {

	public GameObject target;

	void Start(){
		Setup ();
	}

	public void Setup() {
		if (target == null) {
			target = GameObject.FindGameObjectWithTag ("Player");
		}
		if (target != null) {
			FieldOfView fov = target.GetComponentInChildren<FieldOfView> ();
			if (fov) {
				float size = fov.viewRadius;
				GetComponent<Camera> ().orthographicSize = size;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (target == null) {
			Setup ();
		} else {
			transform.position = new Vector3(
				target.transform.position.x,
				target.transform.position.y,
				transform.position.z);
		}
	}
}
