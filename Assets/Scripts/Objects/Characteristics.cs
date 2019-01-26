using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Characteristics {
	public float speed;
	public float decceleration;
	public float absorbForce;

	public Characteristics(float speed, float decceleration, float absorbForce){
		this.speed = speed;
		this.decceleration = decceleration;
		this.absorbForce = absorbForce;
	}

	public static Characteristics random(){
		return new Characteristics (
			Random.value * Random.Range (0, 10),
			Random.value * Random.Range (0, 5),
			Random.value * Random.Range (0, 5));
	}
}
