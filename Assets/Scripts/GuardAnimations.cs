using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAnimations : MonoBehaviour {

    public Animator animator;

    private State current_state;
    private GuardController controller;

	// Use this for initialization
	void Start () {
        controller = GetComponent<GuardController>();
        animator.SetBool("isIdle",true);
        current_state = controller.GetState();
        SetAnimation(current_state);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (current_state != controller.GetState())
        {
            current_state = controller.GetState();
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
