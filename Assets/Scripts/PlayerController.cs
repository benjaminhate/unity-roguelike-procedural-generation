using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float speed;

	[SerializeField]
	private float percentAbsorb = 0f;
	// Update is called once per frame
	void Update () {
		Move ();
	}

	void Move(){
		float vertical = Input.GetAxisRaw ("Vertical");
		float horizontal = Input.GetAxisRaw ("Horizontal");

		transform.position += (Vector3)((horizontal * Vector2.right + vertical * Vector2.up) * speed * Time.deltaTime);
	}

	void OnTriggerEnted2D(Collider2D other){
		if (other.CompareTag ("Enemy")) {
			other.GetComponent<AbsorbBar> ().OnDataChanged (0);
		}
	}

	void OnTriggerStay2D(Collider2D other){
		if (other.CompareTag ("Enemy")) {
			if (other.GetComponent<GuardController> ().state == State.Patrol) {
				percentAbsorb += Time.deltaTime / 2;
				other.GetComponent<AbsorbBar> ().OnDataChanged (percentAbsorb);
				if (percentAbsorb > 1f) {
					Debug.Log ("ABSORBED");
					percentAbsorb = 1f;
				}
			}
		}
	}

	void OnTriggerExit2D(Collider2D other){
		if (other.CompareTag ("Enemy")) {
			percentAbsorb = 0f;
			other.GetComponent<AbsorbBar> ().OnDataChanged (0);
		}
	}
}
