using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

	public bool isMoving;
    public bool isAGuard;
	public Transform rotController;
	public float animSpeed;
	private Animator anim;
    private GuardController controller;
    private State state;
	public bool isAttackAnimated;
	private bool attackIsPlaying;

    void Start(){
		anim = GetComponent<Animator> ();
		controller = GetComponent<GuardController> ();
		isAGuard = false;
		if (controller != null)
			isAGuard = true;
	}

	public IEnumerator DeadAnimation(float time){
		anim.Play ("Death");
		yield return new WaitForSeconds (time);
		if (isAGuard) {
			GetComponent<GuardController> ().Death ();
		} else {
			GetComponent<PlayerController> ().Death ();
		}
	}

	// Event function for finishing attack animation
	public void FinishAttack(){
		isAttackAnimated = false;
		attackIsPlaying = false;
	}

	public void UpdateAnimator()
	{
		if (anim == null)
			return;
		
		float angle = rotController.rotation.eulerAngles.z;
		float speed = animSpeed;

		if (isAGuard) {
			state = controller.GetState ();
		}

		if (attackIsPlaying)
			return;

		if (angle > 135 && angle < 225) {
			if (isMoving && isAGuard && state == State.Chase) {
				//anim.Play("Run_S");
				anim.Play ("Walk_S");
				speed *= 2;
			} else if (isMoving) {
				anim.Play ("Walk_S");
			} else if (isAGuard && state == State.Sleep) {
				//anim.Play("Sleep_D");
				anim.Play ("Idle_S");
			} else if (isAGuard && state == State.Attack && isAttackAnimated) {
				attackIsPlaying = true;
				anim.Play ("Attack_S");
			} else {
				anim.Play ("Idle_S");
			}
		}
		if (angle > 315 || angle < 45) {
			if (isMoving && isAGuard && state == State.Chase) {
				//anim.Play("Run_Z");
				anim.Play ("Walk_Z");
				speed *= 2;
			} else if (isMoving) {
				anim.Play ("Walk_Z");
			} else if (isAGuard && state == State.Sleep) {
				//anim.Play("Sleep_Z");
				anim.Play ("Idle_Z");
			} else if (isAGuard && state == State.Attack && isAttackAnimated) {
				attackIsPlaying = true;
				anim.Play ("Attack_Z");
			} else {
				anim.Play ("Idle_Z");
			}
		}
		if (angle >= 45 && angle <= 135) {
			if (isMoving && isAGuard && state == State.Chase) {
				//anim.Play("Run_Q");
				anim.Play ("Walk_Q");
				speed *= 2;
			} else if (isMoving) {
				anim.Play ("Walk_Q");
			} else if (isAGuard && state == State.Sleep) {
				//anim.Play("Sleep_Q");
				anim.Play ("Idle_Q");
			} else if (isAGuard && state == State.Attack && isAttackAnimated) {
				attackIsPlaying = true;
				anim.Play ("Attack_Q");
			} else {
				anim.Play ("Idle_Q");
			}
		}
		if (angle >= 225 && angle <= 315) {
			if (isMoving && isAGuard && state == State.Chase) {
				//anim.Play("Run_D");
				anim.Play ("Walk_D");
				speed *= 2;
			} else if (isMoving) {
				anim.Play ("Walk_D");
			} else if (isAGuard && state == State.Sleep) {
				//anim.Play("Sleep_D");
				anim.Play ("Idle_D");
			} else if (isAGuard && state == State.Attack && isAttackAnimated) {
				attackIsPlaying = true;
				anim.Play ("Attack_D");
			} else {
				anim.Play ("Idle_D");
			}
		}

		anim.speed = speed;

	}
}
