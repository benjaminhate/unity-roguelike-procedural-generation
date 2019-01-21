using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Structure to store the infos for the ViewCast method
public struct ViewCastInfo{
	public bool hit;
	public Vector3 point;
	public float dist;
	public float angle;

	public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle){
		hit = _hit;
		point = _point;
		dist = _dist;
		angle = _angle;
	}
}

// Structure to store the infos for the FindEdge method
public struct EdgeInfo{
	public Vector3 pointA;
	public Vector3 pointB;

	public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
		pointA = _pointA;
		pointB = _pointB;
	}
}

public class FieldOfView : MonoBehaviour {

	public float viewRadius;
	[Range(0,360)]
	public float viewAngle;

	public string targetTag;

	public LayerMask obstacleMask;

	public List<Transform> visibleTargets = new List<Transform>();

	// drawFov if set to true for the player for the limited view effect
	public bool drawFov = false;
	public float meshResolution;
	public float maskCutaway;

	public int edgeResolveIterations;
	public float edgeDistThreshold;

	public MeshFilter viewMeshFilter;
	Mesh viewMesh;

	void Start(){
		if (drawFov) {
			viewMesh = new Mesh ();
			viewMesh.name = "View Mesh";
			viewMeshFilter.mesh = viewMesh;
		}
		StartCoroutine ("FindTargetsWithDelay", .2f);
	}

	void LateUpdate(){
		if (drawFov) {
			DrawFieldOfView ();
		}
	}

	IEnumerator FindTargetsWithDelay(float delay){
		while (true) {
			yield return new WaitForSeconds (delay);
			FindVisibleTargets ();
		}
	}

	// Detect the targets within a certains radius with the obstacleMask blocking the view
	void FindVisibleTargets(){
		visibleTargets.Clear ();
		Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll (transform.position, viewRadius);

		for (int i = 0; i < targetsInViewRadius.Length; i++) {
			Transform target = targetsInViewRadius [i].transform;
			bool isTrigger = targetsInViewRadius [i].isTrigger;
			// If the target is the target wanted + the collider is not trigger (to avoid two detections for the enemies)
			if (target.CompareTag (targetTag) && !isTrigger) {
				Vector2 dirToTarget = (target.position - transform.position).normalized;
				// check if the direction to the target is in the fov
				if (Vector2.Angle (transform.up, dirToTarget) < viewAngle / 2) {
					float distToTarget = Vector2.Distance (target.position, transform.position);

					// check for any obstacle on the way
					if (!Physics2D.Raycast (transform.position, dirToTarget, distToTarget, obstacleMask)) {
						visibleTargets.Add (target);
					}
				}
			}
		}
	}

	void DrawFieldOfView(){
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngleSize = viewAngle / stepCount;

		List<Vector3> viewPoints = new List<Vector3> ();
		ViewCastInfo oldViewCast = new ViewCastInfo ();

		// throw raycasts within the fov
		for (int i = 0; i <= stepCount; i++) {
			float angle = transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
			ViewCastInfo newViewCast = ViewCast(angle);
			//Debug.DrawLine (transform.position, transform.position + DirFromAngle (angle, true) * newViewCast.dist, Color.red);

			// when the oldViewCast is set
			if (i > 0) {
				bool edgeDistThresholdExceeded = Mathf.Abs (oldViewCast.dist - newViewCast.dist) > edgeDistThreshold;
				// Dichotomical method to find the edges of walls for better draw fov
				if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistThresholdExceeded)) {
					EdgeInfo edge = FindEdge (oldViewCast, newViewCast);
					if (edge.pointA != Vector3.zero) {
						viewPoints.Add (edge.pointA);
					}
					if (edge.pointB != Vector3.zero) {
						viewPoints.Add (edge.pointB);
					}
				}
			}

			viewPoints.Add (newViewCast.point);
			oldViewCast = newViewCast;
		}

		// Create the mesh
		int vertexCount = viewPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int triangleCount = (vertexCount - 2) * 3;
		int[] triangles = new int[triangleCount];

		// The vertices are in local space
		vertices [0] = Vector3.zero;
		for (int i = 0; i < vertexCount - 1; i++) {
			vertices [i + 1] = transform.InverseTransformPoint (viewPoints [i]);

			if (i < vertexCount - 2) {
				triangles [i * 3] = i + 2;
				triangles [i * 3 + 1] = i + 1;
				triangles [i * 3 + 2] = 0;
			}
		}

		// Reset the mesh each Update
		viewMesh.Clear ();
		viewMesh.vertices = vertices;
		viewMesh.triangles = triangles;
		viewMesh.RecalculateNormals ();
		//viewMeshFilter.transform.rotation = transform.rotation;
	}

	// Dichotomical method to find wall edges
	EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast){
		float minAngle = minViewCast.angle;
		float maxAngle = maxViewCast.angle;
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;

		for (int i = 0; i < edgeResolveIterations; i++) {
			float angle = (minAngle + maxAngle) / 2;
			ViewCastInfo newViewCast = ViewCast (angle);

			bool edgeDistThresholdExceeded = Mathf.Abs (minViewCast.dist - newViewCast.dist) > edgeDistThreshold;
			if (newViewCast.hit == minViewCast.hit && !edgeDistThresholdExceeded) {
				minAngle = angle;
				minPoint = newViewCast.point;
			} else {
				maxAngle = angle;
				maxPoint = newViewCast.point;
			}
		}

		return new EdgeInfo (minPoint, maxPoint);
	}

	// Throw raycast at angle and return the infos
	ViewCastInfo ViewCast(float globalAngle){
		Vector3 dir = DirFromAngle (globalAngle, true);
		RaycastHit2D hit = Physics2D.Raycast (transform.position, dir, viewRadius, obstacleMask);

		if (hit.collider != null) {
			return new ViewCastInfo (true, hit.point, hit.distance, globalAngle);
		} else {
			return new ViewCastInfo (false, transform.position + dir * viewRadius, viewRadius, globalAngle);
		}
	}

	// Convert an angle to a direction
	public Vector3 DirFromAngle(float angleInDegrees, bool isAngleGlobal){
		if (!isAngleGlobal) {
			angleInDegrees += transform.eulerAngles.z;
		}
		return new Vector3 (-Mathf.Sin (angleInDegrees * Mathf.Deg2Rad), Mathf.Cos (angleInDegrees * Mathf.Deg2Rad), 0);
	}
}
