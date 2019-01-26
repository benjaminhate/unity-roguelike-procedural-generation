using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StartingState{
	Patrol = 0,
	Sleep = 1,
	Idle = 2
}

public enum State{
	Patrol,
	Sleep,
	Idle,
	Chase,
	Monitor
}

public class GuardController : Controller {

	public StartingState startingState;
	public Waypoint[] waypoints;
	public float idleDuration;

	private FieldOfView fov;
	private int currentWaypointIndex = 0;
	private Transform target;
	private Vector3 monitorPos;
	private bool idleFromMonitor;

	private float idleStart;
	private bool idleStill;
	private float initAngle;

	private AnimationController anim;

	private State state;
	private bool isMoving;

    public State GetState()
    {
        return state;
    }

    // Value used in PlayerController
    public bool EnableAbsorption(){
		return state != State.Chase;
	}

	void Awake(){
		SaveCurrentState ();
	}

	void Start () {
		anim = GetComponent<AnimationController> ();
		fov = GetComponentInChildren<FieldOfView> ();
		ChangeState ((State) startingState);
        UpdateController ();
		idleStill = idleDuration <= 0;
	}
	
	void Update () {
		Move ();
		UpdateState ();
		UpdateAnim ();
		if (state == State.Chase && Vector2.Distance (target.position,transform.position) < 3f) {
			SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
		}
	}

	void Move(){
		isMoving = false;
		if (state == State.Patrol) {
			MoveToWaypoint (waypoints [currentWaypointIndex]);
		}
		if (state == State.Chase) {
			MoveToTarget (target.position);
		}
		if (state == State.Monitor) {
			MoveToMonitor ();
		}
		if (state == State.Idle) {
			if (idleStill) {
				WaitIdle ();
			} else {
				MoveAroundIdle ();
			}
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
		if (s == State.Idle) {
			idleStart = Time.time;
			initAngle = transform.eulerAngles.z;
		}
		state = s;
    }

	void MoveToTarget(Vector3 targetPos){
		Vector3 currentPos = transform.position;

		// Normalize the direction or else the guard will be faster if he is far away
		Vector3 dir = (targetPos - currentPos).normalized;

		transform.position += dir * innerState.characteristics.speed * Time.deltaTime / innerState.characteristics.decceleration;
		// If the guard is moving
		isMoving = dir != Vector3.zero;
		if (isMoving) {
			float angle = Mathf.Atan2 (dir.x, dir.y) * Mathf.Rad2Deg;
			fov.transform.rotation = Quaternion.AngleAxis(angle,Vector3.back);
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
			idleFromMonitor = true;
			ChangeState (State.Idle);
		} else {
			MoveToTarget (monitorPos);
		}
	}

	void MoveAroundIdle(){
		float percent = (Time.time - idleStart) / idleDuration;

		if (percent > 1f) {
			if (idleFromMonitor) {
				ChangeState (State.Patrol);
			} else {
				idleStill = true;
				idleStart = Time.time;
			}
		} else {
			float angle = Mathf.Rad2Deg * Mathf.PI * 2.0f * percent + initAngle;
			fov.transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);
		}
	}

	void WaitIdle(){
		if (Time.time - idleStart > idleDuration) {
			idleStart = Time.time;
			idleStill = false;
		}
	}

	void UpdateAnim(){
		anim.animSpeed = innerState.characteristics.speed / 20f;
		anim.rotController = fov.transform;
		anim.isMoving = isMoving;
		anim.UpdateAnimator ();
	}
}
