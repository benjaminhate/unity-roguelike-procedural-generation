using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StartingState{
	Patrol = 0,
	Sleep = 2,
	Idle = 3
}

public enum State{
	Patrol,
	Chase,
	Sleep,
	Idle,
	Monitor
}

[RequireComponent(typeof (FieldOfView))]
public class GuardController : Controller {

	public StartingState startingState;
	public Waypoint[] waypoints;
	public float idleDuration;

	private FieldOfView fov;
	private int currentWaypointIndex = 0;
	private Transform target;
	private Vector3 monitorPos;

	private State state;
    private static State static_state;

    public static State GetState()
    {
        return static_state;
    }

    // Value used in PlayerController
    public bool EnableAbsorption(){
		return state != State.Chase;
	}

	void Awake(){
		SaveCurrentState ();
	}

	void Start () {
		fov = GetComponent<FieldOfView> ();
		state = (State) startingState;
        static_state = state;
        UpdateController ();
	}
	
	void Update () {
		Move ();
		UpdateState ();
	}

	void Move(){
		if (state == State.Patrol) {
			MoveToWaypoint (waypoints [currentWaypointIndex]);
		}
		if (state == State.Chase) {
			MoveToTarget (target.position);
		}
		if (state == State.Monitor) {
			MoveToMonitor ();
		}
	}

	void UpdateState(){
		if (fov.visibleTargets.Count > 0) {
			Controller fovController = fov.visibleTargets [0].GetComponent<Controller> ();
			if (state != State.Sleep && state != State.Chase && fovController.innerState.type == ControllerType.PLAYER) {
				ChangeState (State.Chase);
			}
		} else {
			if (state == State.Chase) {
				ChangeState (State.Monitor);
			}
		}
	}

	void ChangeState(State s){
		Debug.Log (s);
		if (s == State.Chase) {
			target = fov.visibleTargets [0];
		}
		if (s == State.Monitor) {
			monitorPos = target.position;
		}
		state = s;
        static_state = state;
    }

	void MoveToTarget(Vector3 targetPos){
		Vector3 currentPos = transform.position;

		// Normalize the direction or else the guard will be faster if he is far away
		Vector3 dir = (targetPos - currentPos).normalized;

		transform.position += dir * innerState.characteristics.speed * Time.deltaTime / innerState.characteristics.decceleration;
		// If the guard is moving
		if (dir != Vector3.zero) {
			float angle = Mathf.Atan2 (dir.x, dir.y) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle,Vector3.back);
		}
	}
		
	void MoveToWaypoint(Waypoint waypoint){
		Vector3 currentPos = transform.position;
		Vector3 waypointPos = waypoint.position;

		// Check if the guard is near the waypoint
		if (Vector3.Distance (currentPos, waypointPos) < .1f) {
			NextWaypoint ();
		} else {
			MoveToTarget (waypoint.position);
		}
	}

	// Change waypoint
	void NextWaypoint(){
		currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
	}

	void MoveToMonitor(){
		Vector3 currentPos = transform.position;

		if (Vector3.Distance (currentPos, monitorPos) < .1f) {

		} else {
			MoveToTarget (monitorPos);
		}
	}
}
