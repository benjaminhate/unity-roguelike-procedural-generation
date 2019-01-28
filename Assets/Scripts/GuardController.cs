using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	Monitor,
	Attack
}

public class GuardController : Controller {

	public StartingState startingState;
	public List<Waypoint> waypoints;
	public float idleDuration;

	private FieldOfView fov;
	private int currentWaypointIndex = 0;
	private Vector3 targetPos;
	private Vector3 monitorPos;
	private bool idleFromMonitor;

	private float idleStart;
	private bool idleStill;
	private float initAngle;

	private AnimationController anim;

	private State state;
	private bool isMoving;

	private float attackDist = 3.8f;
	private float waypointDist = .1f;

	private bool isDead = false;

    public State GetState()
    {
        return state;
    }

	public void Death(){
		Destroy (gameObject);
	}

	public void DeadAnimation(){
		isDead = true;
		StartCoroutine (anim.DeadAnimation (2f));
	}

    // Value used in PlayerController
    public bool EnableAbsorption(){
		return state != State.Chase && state != State.Attack && !isDead;
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
		if (!isDead) {
			Move ();
			UpdateState ();
			UpdateAnim ();
		}
	}

	void Move(){
		isMoving = false;
		if (state == State.Patrol) {
			MoveToWaypoint (waypoints [currentWaypointIndex]);
		}
		if (state == State.Chase) {
			MoveToChase ();
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
		if (state == State.Attack) {
			WaitForAttack ();
		}
	}

	void UpdateState(){
		if (fov.visibleTargets.Count > 0) {
			Controller fovController = fov.visibleTargets [0].GetComponent<Controller> ();
			State[] states = {State.Sleep, State.Attack, State.Chase};
			if (!StateIsInList (state, states) && fovController.innerState.type == ControllerType.PLAYER) {
				ChangeState (State.Chase);
			}
			if (state == State.Chase) {
				targetPos = fov.visibleTargets [0].position;
			}
		} else {
			if (state == State.Chase) {
				ChangeState (State.Monitor);
			}
			if (state == State.Attack) {
				
			}
		}
	}

	void ChangeState(State s){
		Debug.Log (s);
		if (s == State.Chase) {
			if (fov.visibleTargets.Count > 0)
				targetPos = fov.visibleTargets [0].position;
		}
		if (s == State.Monitor) {
			monitorPos = targetPos;
		}
		if (s == State.Idle) {
			idleStart = Time.time;
			initAngle = transform.eulerAngles.z;
		}
		if (s == State.Attack) {
			anim.isAttackAnimated = true;
		}
		state = s;
    }

	void MoveToTarget(Vector3 targetPos){
		Vector3 currentPos = transform.position;

		// Normalize the direction or else the guard will be faster if he is far away
		Vector3 dir = (targetPos - currentPos).normalized;

		transform.position += dir * innerState.characteristics.speed * Time.deltaTime;
		// If the guard is moving
		isMoving = dir != Vector3.zero;
		if (isMoving) {
			float angle = Mathf.Atan2 (dir.x, dir.y) * Mathf.Rad2Deg;
			fov.transform.rotation = Quaternion.AngleAxis(angle,Vector3.back);
		}
	}

	void MoveToChase(){
		Vector3 currentPos = transform.position;

		if (Vector3.Distance (currentPos, targetPos) < attackDist) {
			ChangeState (State.Attack);
		} else {
			MoveToTarget (targetPos);
		}
	}
		
	void MoveToWaypoint(Waypoint waypoint){
		Vector3 currentPos = transform.position;
		Vector3 waypointPos = waypoint.position;

		// Check if the guard is near the waypoint
		if (Vector3.Distance (currentPos, waypointPos) < waypointDist) {
			NextWaypoint ();
		} else {
			MoveToTarget (waypoint.position);
		}
	}

	// Change waypoint
	void NextWaypoint(){
		currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
	}

	void MoveToMonitor(){
		Vector3 currentPos = transform.position;

		if (Vector3.Distance (currentPos, monitorPos) < waypointDist) {
			idleFromMonitor = true;
			ChangeState (State.Idle);
		} else {
			MoveToTarget (monitorPos);
		}
	}

	void MoveAroundIdle(){
		float percent = (Time.time - idleStart) / innerState.characteristics.speed;

		if (percent > 1f) {
			if (idleFromMonitor) {
				ChangeState ((State)startingState);
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

	void WaitForAttack(){
		Vector3 currentPos = transform.position;
		if (!anim.isAttackAnimated) {
			if (fov.visibleTargets.Count > 0) {
				targetPos = fov.visibleTargets [0].position;
				//Debug.Log (currentPos + ";" + targetPos);
				if (Vector3.Distance (currentPos, targetPos) >= attackDist) {
					ChangeState (State.Chase);
				} else {
					anim.isAttackAnimated = true;
				}
			} else {
				ChangeState (State.Monitor);
			}

		}
	}

	bool StateIsInList(State state, State[] list){
		for (int i = 0; i < list.Length; i++) {
			if (list [i] == state)
				return true;
		}
		return false;
	}

	void UpdateAnim(){
		anim.animSpeed = Mathf.Sqrt (innerState.characteristics.speed) / 10f;
		anim.rotController = fov.transform;
		anim.isMoving = isMoving;
		anim.UpdateAnimator ();
	}
}
