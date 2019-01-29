using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour {

	public GameObject endText;

	void OnTriggerEnter2D(Collider2D other){
		if (other.CompareTag ("Player")) {
			other.gameObject.SetActive (false);
			endText.SetActive (true);
			StartCoroutine (WaitBeforeRestart ());
		}
	}

	IEnumerator WaitBeforeRestart(){
		GameObject dungeonGenerator = GameObject.Find ("DungeonGenerator");
		dungeonGenerator.GetComponent<GenerateLevel> ().AddDifficulty (0.1f);
		yield return new WaitForSeconds (1f);
		dungeonGenerator.GetComponent<GenerateLevel> ().ResetDungeonVoid ();
		//SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}
}
