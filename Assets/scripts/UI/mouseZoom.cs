using UnityEngine;
using System.Collections;

public class mouseZoom : MonoBehaviour {
	
	public float zoomLevel = 30f;
	public float zoomTime = 1f;
	private float startTime;
	private float startFOV = 60;
	private int zooming = 0;

	
	// Use this for initialization
	void Start () {
		startFOV = Camera.main.fieldOfView;
	}
	
	// Update is called once per frame
	void Update () {

		// desktop
		if(Input.GetButtonDown("Fire1")) {
			startTime = Time.time;
			zooming = 1;
		} else if (Input.GetButtonUp("Fire1")) {
			startTime = Time.time;
			zooming = -1;
		}


		// handle zooming

		float progress = (Time.time - startTime) / zoomTime;
		if (progress > 1) {
			zooming = 0;
		}
		if (Camera.main) {
			if(zooming == 1) {
				Camera.main.fieldOfView = Mathf.Lerp(startFOV, zoomLevel, progress);
			} else if (zooming == -1) {
				Camera.main.fieldOfView = Mathf.Lerp(zoomLevel, startFOV, progress);
			}
		}
		//Debug.Log (newZoom);
		
	}
	
}
