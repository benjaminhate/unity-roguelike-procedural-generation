using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour {

	public float viewRadius;
	[Range(0,360)]
	public float viewAngle;

	public string targetTag;

	public LayerMask obstacleMask;

	public List<Transform> visibleTargets = new List<Transform>();

	void Start(){
		StartCoroutine ("FindTargetsWithDelay", .2f);
	}

	IEnumerator FindTargetsWithDelay(float delay){
		while (true) {
			yield return new WaitForSeconds (delay);
			FindVisibleTargets ();
		}
	}

	void FindVisibleTargets(){
		visibleTargets.Clear ();
		Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll (transform.position, viewRadius);

		for (int i = 0; i < targetsInViewRadius.Length; i++) {
			Transform target = targetsInViewRadius [i].transform;
			if (target.CompareTag (targetTag)) {
				Vector2 dirToTarget = (target.position - transform.position).normalized;
				if (Vector2.Angle (transform.up, dirToTarget) < viewAngle / 2) {
					float dstToTarget = Vector2.Distance (target.position, transform.position);

					if (!Physics2D.Raycast (transform.position, dirToTarget, dstToTarget, obstacleMask)) {
						visibleTargets.Add (target);
					}
				}
			}
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool isAngleGlobal){
		if (!isAngleGlobal) {
			angleInDegrees += transform.eulerAngles.z;
		}
		return new Vector3 (-Mathf.Sin (angleInDegrees * Mathf.Deg2Rad), Mathf.Cos (angleInDegrees * Mathf.Deg2Rad), 0);
	}
}
