using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State{
	Patrol,
	Chase
}

public class GuardController : Controller {

	public Waypoint[] waypoints;

	private FieldOfView fov;
	private int currentWaypointIndex = 0;
	private Transform target;

	public State state;

	// Use this for initialization
	void Start () {
		fov = GetComponent<FieldOfView> ();
		state = State.Patrol;
		UpdateController ();
	}
	
	// Update is called once per frame
	void Update () {
		Move ();
		UpdateState ();
	}

	void Move(){
		if (state == State.Patrol) {
			MoveToWaypoint (waypoints [currentWaypointIndex]);
		}
		if (state == State.Chase) {
			MoveToTarget (target);
		}
	}

	void UpdateState(){
		if (fov.visibleTargets.Count > 0) {
			if (state == State.Patrol) {
				ChangeState (State.Chase);
			}
		} else {
			if (state == State.Chase) {
				ChangeState (State.Patrol);
			}
		}
	}

	void ChangeState(State s){
		Debug.Log (s);
		if (s == State.Chase) {
			target = fov.visibleTargets [0];
		}
		state = s;
	}

	void MoveToTarget(Transform target){
		Vector3 currentPos = transform.position;
		Vector3 targetPos = target.position;

		Vector3 dir = (targetPos - currentPos).normalized;
		Debug.Log (dir);
		transform.position += dir * characteristics.speed * Time.deltaTime;
		if (dir != Vector3.zero) {
			float angle = Mathf.Atan2 (dir.x, dir.y) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle,Vector3.back);
		}
	}

	void MoveToWaypoint(Waypoint waypoint){
		Vector3 currentPos = transform.position;
		Vector3 waypointPos = waypoint.transform.position;
		Debug.Log (waypointPos);
		Debug.Log (Vector3.Distance (currentPos, waypointPos));
		if (Vector3.Distance (currentPos, waypointPos) < .1f) {
			NextWaypoint ();
		} else {
			MoveToTarget (waypoint.transform);
		}
	}

	void NextWaypoint(){
		currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
	}
}
