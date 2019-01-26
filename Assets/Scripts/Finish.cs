using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Finish : MonoBehaviour {

	public GameObject endText;

	void OnTriggerEnter2D(Collider2D other){
		if (other.CompareTag ("Player")) {
			other.gameObject.SetActive (false);
			endText.SetActive (true);
		}
	}
}
