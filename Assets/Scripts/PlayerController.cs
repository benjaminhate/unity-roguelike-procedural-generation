using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Rigidbody2D))]
public class PlayerController : Controller {

	public float absorptionDuration;

	public float staminaDrain;
	public float maxStamina;
	[SerializeField]
	private float stamina;

	private float moveHDelay = 0f;
	private float moveVDelay = 0f;

	private Rigidbody2D rd2d;
	private AnimationController anim;
	private FieldOfView fov;
	private bool isMoving;

	private ControllerState savedController = null;
	private bool absorbed = false;
	private float timeAbsorbed = 0f;

	void Awake(){
		SaveCurrentState ();
	}

	void Start(){
		stamina = maxStamina;
		anim = GetComponent<AnimationController> ();
		fov = GetComponentInChildren<FieldOfView> ();
		UpdateController ();
		rd2d = GetComponent<Rigidbody2D> ();
	}

	void FixedUpdate () {
		Move ();
		UpdateAbsorption ();
		UpdateAnim ();
	}

	void Move(){
		float moveVertical = Input.GetAxisRaw ("Vertical");
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");

		float speedRate = Mathf.Max (Mathf.Abs (moveVertical), Mathf.Abs (moveHorizontal))/innerState.characteristics.decceleration;
		float rot = Mathf.Rad2Deg * Mathf.Atan2 (-moveHorizontal, moveVertical);

		isMoving = moveVertical != 0 || moveHorizontal != 0;

		// If the character is accelerating
		if ((Mathf.Abs (moveHorizontal) != 0 && Mathf.Abs (moveHorizontal) - Mathf.Abs (moveHDelay) >= 0 )
			|| (Mathf.Abs (moveVertical) != 0 & Mathf.Abs (moveVertical) - Mathf.Abs (moveVDelay) >= 0)) {
			fov.transform.rotation = Quaternion.AngleAxis (rot, Vector3.forward);
			// Remove the effect of decceleration
			speedRate *= innerState.characteristics.decceleration;
		}

		// Keep a track of the values to check if the player has accelerated since last time
		moveHDelay = moveHorizontal;
		moveVDelay = moveVertical;

		Vector2 forward = new Vector2 (moveHorizontal, moveVertical);

		if (Input.GetKey (KeyCode.LeftShift)) {
			if (isMoving && stamina > 0) {
				speedRate *= 2;
				stamina -= staminaDrain * Time.deltaTime;
				if (stamina < 0)
					stamina = 0;
			}
		} else if(stamina < maxStamina) {
			stamina += staminaDrain * Time.deltaTime / 2f;
			if (stamina > maxStamina)
				stamina = maxStamina;
		}

		transform.Translate (innerState.characteristics.speed * speedRate * forward * Time.fixedDeltaTime);
	}

	void OnTriggerEnted2D(Collider2D other){
		// When we enter the enemy's absorption area
		if (other.CompareTag ("Enemy")) {
			other.GetComponent<AbsorbBar> ().SetAmount (0);
		}
	}

	void OnTriggerStay2D(Collider2D other){
		Debug.Log ("Stay");
		// When we stay in the enemy's absorption area
		if (other.CompareTag ("Enemy")) {
			// Controll the state the enemy is in
			if (other.GetComponent<GuardController> ().EnableAbsorption ()) {
				// Add an amount to the percentage absorbed
				// Each amount is stored within the enemy's AbsorbBar to ensure that each enemy has his own value
				float amount = Time.deltaTime * innerState.characteristics.absorbForce / other.GetComponent<Controller> ().innerState.characteristics.absorbForce;
				other.GetComponent<AbsorbBar> ().AddAmount (amount);
				// When it is at max, we can absorb with the Key E
				if (other.GetComponent<AbsorbBar> ().IsMaxAmount () && Input.GetKey (KeyCode.E)) {
					Debug.Log ("ABSORBED");
					other.GetComponent<AbsorbBar> ().Destroy ();
					Absorbtion (other.gameObject);
					Destroy (other.gameObject);
				}
			}
		}
	}

	void OnTriggerExit2D(Collider2D other){
		// When we quit the enemy's absorption area
		if (other.CompareTag ("Enemy")) {
			other.GetComponent<AbsorbBar> ().SetAmount (0);
		}
	}

	void Absorbtion(GameObject enemy){
		// Save the state if we have not absorbed yet (used to return to previous state when timer is over)
		if (!absorbed) {
			savedController = SaveController ();
			absorbed = true;
		}
		timeAbsorbed = Time.time;
		Controller enemyController = enemy.GetComponent<Controller> ();
		ChangePlayerController (enemyController.innerState);
	}

	void ChangePlayerController(ControllerState newState){
		ChangeController (newState);
		UpdateController ();
		Destroy (GetComponent <PolygonCollider2D> ());
		gameObject.AddComponent<PolygonCollider2D> ();
	}

	void UpdateAbsorption(){
		// If the absorption timer is over
		if (absorbed && Time.time - timeAbsorbed > absorptionDuration) {
			absorbed = false;
			ChangePlayerController (savedController);
			savedController = null;
		}
	}

	void UpdateAnim(){
		anim.animSpeed = innerState.characteristics.speed / 10f;
		anim.rotController = fov.transform;
		anim.isMoving = isMoving;
		anim.UpdateAnimator ();
	}
}
