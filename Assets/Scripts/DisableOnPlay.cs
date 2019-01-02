using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnPlay : MonoBehaviour {

	void Awake(){
		gameObject.SetActive (false);
	}
}
