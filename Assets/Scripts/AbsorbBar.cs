using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbsorbBar : MonoBehaviour {

	private Vector2 positionCorrection = new Vector2(0,75);

	public RectTransform targetCanvas;
	public RectTransform bar;
	public Transform targetFollow;

	void Start(){
		SetBarData (bar, targetCanvas);
	}

	public void SetBarData(RectTransform barRect,RectTransform barPanel){
		targetCanvas = barPanel;
		targetFollow = GetComponent<Transform> ();
		bar = barRect;
		RepositionBar ();
		bar.gameObject.SetActive (true);
	}

	public void OnDataChanged(float fillAmount){
		bar.GetComponent<Image> ().fillAmount = fillAmount;
	}

	void Update(){
		RepositionBar ();
	}

	private void RepositionBar(){
		Vector2 ViewportPosition = Camera.main.WorldToViewportPoint (targetFollow.position);
		Vector2 WorldObject_ScreenPosition = new Vector2 (
			((ViewportPosition.x * targetCanvas.sizeDelta.x) - (targetCanvas.sizeDelta.x * 0.5f)) + positionCorrection.x,
			((ViewportPosition.y * targetCanvas.sizeDelta.y) - (targetCanvas.sizeDelta.y * 0.5f)) + positionCorrection.y
		);
		bar.anchoredPosition = WorldObject_ScreenPosition;
	}
}
