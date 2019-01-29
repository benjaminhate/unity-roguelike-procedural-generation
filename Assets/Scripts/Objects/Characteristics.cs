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

	public static Characteristics random(float difficulty){
		return new Characteristics (
			Random.value * Random.Range (1, 10) * difficulty + 2f,
			Random.value * Random.Range (1, 5) * difficulty + 1f,
			Random.value * Random.Range (1, 5) * difficulty + 1f);
	}
}
