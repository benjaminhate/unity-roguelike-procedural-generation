using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

	public bool isMoving;
	public Transform rotController;
	public float animSpeed;
	private Animator anim;

	void Start(){
		anim = GetComponent<Animator> ();
	}

	public void UpdateAnimator(){
		float angle = rotController.rotation.eulerAngles.z;
		if(angle > 225 && angle <= 345){
			if (isMoving) {
				anim.Play ("Walk_D");
			} else {
				anim.Play ("Idle_D");
			}
		}
		if(angle > 135 && angle <= 225){
			if (isMoving) {
				anim.Play ("Walk_S");
			} else {
				anim.Play ("Idle_S");
			}
		}
		if(angle > 45 && angle <= 135){
			if (isMoving) {
				anim.Play ("Walk_Q");
			} else {
				anim.Play ("Idle_Q");
			}
		}
		if(angle >= 345 || angle <= 45){
			if (isMoving) {
				anim.Play ("Walk_Z");
			} else {
				anim.Play ("Idle_Z");
			}
		}
		anim.speed = animSpeed;
	}
}
