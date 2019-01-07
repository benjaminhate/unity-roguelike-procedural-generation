using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAnimations : MonoBehaviour {

    public Animator animator;

    private State current_state;

	// Use this for initialization
	void Start () {
        animator.SetBool("isIdle",true);
        current_state = GuardController.GetState();
        SetAnimation(current_state);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (current_state != GuardController.GetState())
        {
            current_state = GuardController.GetState();
            SetAnimation(current_state);
        }

    }

    void SetAnimation(State state)
    {
        if (state == State.Idle)
        {
            animator.SetBool("isIdle", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isSleeping", false);
        }
        if (state == State.Patrol)
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalking", true);
            animator.SetBool("isSleeping", false);
        }
        if(state == State.Sleep)
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isSleeping", true);
        }
    }
}
