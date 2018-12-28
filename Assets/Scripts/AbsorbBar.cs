using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbsorbBar : MonoBehaviour {

	private Vector2 positionCorrection = new Vector2(0,75);

	public RectTransform targetCanvas;
	public RectTransform bar;
	public Transform targetFollow;

	private float amount;
	public float maxAmout;

	void Start(){
		SetBarData (bar, targetCanvas);
	}

	// Initialise the bar's values
	public void SetBarData(RectTransform barRect,RectTransform barPanel){
		targetCanvas = barPanel;
		targetFollow = transform;
		bar = barRect;
		RepositionBar ();
		bar.gameObject.SetActive (true);
	}

	// Fill the bar with the percentage of amount
	public void OnDataChanged(){
		bar.GetComponent<Image> ().fillAmount = amount / maxAmout;
	}

	public void AddAmount(float addAmount){
		SetAmount (amount + addAmount);
	}

	public void SetAmount(float setAmount){
		amount = setAmount;
		if (amount < 0) {
			amount = 0;
		}
		if (amount > maxAmout) {
			amount = maxAmout;
		}
		OnDataChanged ();
	}

	public bool IsMaxAmount(){
		return amount == maxAmout;
	}

	void Update(){
		RepositionBar ();
	}

	// Place the bar on the target's position
	private void RepositionBar(){
		Vector2 ViewportPosition = Camera.main.WorldToViewportPoint (targetFollow.position);
		Vector2 WorldObject_ScreenPosition = new Vector2 (
			((ViewportPosition.x * targetCanvas.sizeDelta.x) - (targetCanvas.sizeDelta.x * 0.5f)) + positionCorrection.x,
			((ViewportPosition.y * targetCanvas.sizeDelta.y) - (targetCanvas.sizeDelta.y * 0.5f)) + positionCorrection.y
		);
		bar.anchoredPosition = WorldObject_ScreenPosition;
	}
}
