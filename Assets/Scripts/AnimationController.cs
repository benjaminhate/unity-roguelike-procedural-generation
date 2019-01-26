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

    void Start(){
		anim = GetComponent<Animator> ();
        controller = GetComponent<GuardController>();
        isAGuard = false;
        if (controller != null)
            isAGuard = true;
	}

	public void UpdateAnimator()
    {
		float angle = rotController.rotation.eulerAngles.z;

        if (isAGuard)
        {
            state = controller.GetState();
        }

		if(angle > 225 && angle <= 345)
        {
            if (isMoving & isAGuard & state == State.Chase)
            {
                anim.Play("Run_D");
            }
            else if (isMoving)
            {
                anim.Play("Walk_D");
            } else if (isAGuard & state == State.Sleep)
            {
                anim.Play("Sleep_D");
			} else {
				anim.Play ("Idle_D");
			}
		}
		if(angle > 135 && angle <= 225)
        {
            if (isMoving & isAGuard & state == State.Chase)
            {
                anim.Play("Run_S");
            }
            else if (isMoving)
            {
                anim.Play ("Walk_S");
            }
            else if (isAGuard & state == State.Sleep)
            {
                anim.Play("Sleep_D");
            }
            else
            {
                anim.Play ("Idle_S");
			}
		}
		if(angle > 45 && angle <= 135)
        {
            if (isMoving & isAGuard & state == State.Chase)
            {
                anim.Play("Run_Q");
            }
            else if (isMoving)
            {
                anim.Play ("Walk_Q");
            }
            else if (isAGuard & state == State.Sleep)
            {
                anim.Play("Sleep_Q");
            }
            else
            {
                anim.Play ("Idle_Q");
			}
		}
		if(angle >= 345 || angle <= 45)
        {
            if (isMoving & isAGuard & state == State.Chase)
            {
                anim.Play("Run_Z");
            }
            else if (isMoving)
            {
                anim.Play ("Walk_Z");
            }
            else if (isAGuard & state == State.Sleep)
            {
                anim.Play("Sleep_Q");
            }
            else
            {
                anim.Play ("Idle_Z");
			}
		}
		anim.speed = animSpeed;

	}
}
