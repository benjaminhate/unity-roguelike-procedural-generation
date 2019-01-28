using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnPlay : MonoBehaviour {

	public bool disable;

	void Awake(){
		gameObject.SetActive (!disable);
	}
}
