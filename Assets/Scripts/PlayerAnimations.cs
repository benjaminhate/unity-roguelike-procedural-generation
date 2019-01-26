using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public bool isMoving;
    public Transform rotController;
    public float animSpeed;

    private Animator anim;
    private State current_state;
    private GuardController guardController;
    private PlayerController playerController;
    private bool facingRight = true;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void UpdateAnimator()
    {
        float angle = rotController.rotation.eulerAngles.z;
        if (angle > 225 && angle <= 345)
        {
            if (isMoving)
            {
                anim.Play("Walk_D");
            }
            else
            {
                anim.Play("Idle_D");
            }
        }
        if (angle > 135 && angle <= 225)
        {
            if (isMoving)
            {
                anim.Play("Walk_S");
            }
            else
            {
                anim.Play("Idle_S");
            }
        }
        if (angle > 45 && angle <= 135)
        {
            if (isMoving)
            {
                anim.Play("Walk_Q");
            }
            else
            {
                anim.Play("Idle_Q");
            }
        }
        if (angle >= 345 || angle <= 45)
        {
            if (isMoving)
            {
                anim.Play("Walk_Z");
            }
            else
            {
                anim.Play("Idle_Z");
            }
        }
        anim.speed = animSpeed;
    }

    void SetAnimation(State state)
    {
        if (state == State.Idle)
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isWalking", false);
            anim.SetBool("isSleeping", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAbsorbed", false);
        }
        if (state == State.Patrol)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", true);
            anim.SetBool("isSleeping", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAbsorbed", false);
        }
        if (state == State.Sleep)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isSleeping", true);
            anim.SetBool("isRunning", false);
            anim.SetBool("isAbsorbed", false);
        }
        if (state == State.Chase)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isSleeping", false);
            anim.SetBool("isRunning", true);
            anim.SetBool("isAbsorbed", false);
        }
        //Absorbtion ? Doit être enclenché par le joueur

    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

}
