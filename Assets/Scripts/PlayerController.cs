using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Controller {

	[SerializeField]
	private float percentAbsorb = 0f;

	private float moveHDelay = 0f;
	private float moveVDelay = 0f;

	private Rigidbody2D rd2d;

	void Start(){
		UpdateController ();
		rd2d = GetComponent<Rigidbody2D> ();
	}

	// Update is called once per frame
	void Update () {
		Move ();
	}

	void Move(){
		float moveVertical = Input.GetAxisRaw ("Vertical");
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");

		float speedRate = Mathf.Max (Mathf.Abs (moveVertical), Mathf.Abs (moveHorizontal))/characteristics.decceleration;
		float rot = Mathf.Rad2Deg * Mathf.Atan2 (-moveHorizontal, moveVertical);
		if ((Mathf.Abs (moveHorizontal) != 0 && Mathf.Abs (moveHorizontal) - Mathf.Abs (moveHDelay) >= 0 )
			|| (Mathf.Abs (moveVertical) != 0 & Mathf.Abs (moveVertical) - Mathf.Abs (moveVDelay) >= 0)) {
			rd2d.rotation = rot;
			speedRate *= characteristics.decceleration;
		}
		moveHDelay = moveHorizontal;
		moveVDelay = moveVertical;
		transform.Translate (characteristics.speed * speedRate * Vector3.up * Time.deltaTime);
	}

	void OnTriggerEnted2D(Collider2D other){
		if (other.CompareTag ("Enemy")) {
			other.GetComponent<AbsorbBar> ().OnDataChanged (0);
		}
	}

	void OnTriggerStay2D(Collider2D other){
		if (other.CompareTag ("Enemy")) {
			if (other.GetComponent<GuardController> ().state == State.Patrol) {
				percentAbsorb += Time.deltaTime * characteristics.absorbForce / other.GetComponent<Controller> ().characteristics.absorbForce;
				other.GetComponent<AbsorbBar> ().OnDataChanged (percentAbsorb);
				if (percentAbsorb > 1f) {
					Debug.Log ("ABSORBED");
					percentAbsorb = 1f;
					Absorbtion (other.gameObject);
					Destroy (other.gameObject);
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

	void Absorbtion(GameObject enemy){
		Controller enemyController = enemy.GetComponent<Controller> ();
		spriteManager = enemyController.spriteManager;
		characteristics = enemyController.characteristics;
		UpdateController ();
		Destroy (GetComponent <PolygonCollider2D> ());
		gameObject.AddComponent<PolygonCollider2D> ();
	}
}
