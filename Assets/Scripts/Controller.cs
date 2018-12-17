using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour{
	public Characteristics characteristics;
	public SpriteManager spriteManager;

	public void UpdateController(){
		GetComponent<SpriteRenderer> ().sprite = spriteManager.sprite;
		GetComponent<SpriteRenderer> ().color = spriteManager.color;
		transform.localScale = spriteManager.scale;
	}
}
