using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class touchLook : MonoBehaviour {
	
	public Camera selectedCamera;
	public Rect[] ignoreAreas;
	public float zoomLevel = 30f;
	public float zoomTime = 1f;
	private float startTime;
	private float startFOV = 60;
	private int zooming = 0;

	private Vector2 oldPosition;
	private bool touching = false;
	private int oldValidTouchCount = 0;

	// touch ignore list
	private List<int> touchIgnoreList;

	// zoom
	private bool twoFingers = false;
	private float startFingerDistance = 0f;

	// Use this for initialization
	void Start () {
		touchIgnoreList = new List<int>();
		startFOV = selectedCamera.fieldOfView;
	}

	// Update is called once per frame
	void Update () {

		// if touch starts in bad area, then it is invalid
		Vector2[] validTouches = getValidTouches ();

		// zoom
			
		// if two fingers 
		if (validTouches.Length == 2) {
			// if just started: 
			if (!twoFingers) {
				// record distance
				startFingerDistance = Vector2.Distance(validTouches[0], validTouches[1])*Camera.main.fieldOfView; //current distance between finger touches
				// set twoFingers
				twoFingers = true;
				// no longer zooming out!
				zooming = 0;
				// else (not just started)
			} else {
				float fingerDistance = Vector2.Distance(validTouches[0], validTouches[1]); //current distance between finger touches
				// (current * startFOV) / old;
				// since startFingerDistance = dist * starFOV
				float newFOV = startFingerDistance/ fingerDistance;
				
				
				// if zoom is > startFOV
				if (newFOV < startFOV) {
					// set zoom based on new distance
					zoomLevel = newFOV;
					if (Camera.main) {
						Camera.main.fieldOfView = zoomLevel;
					}
				}
			}
			// else if (not two fingers)
		} else {
			// if just ended
			if (twoFingers) {
				// set twoFingers = false;
				twoFingers = false;
				startTime = Time.time;
				zooming = -1;
			}
		}
			
		


		
		// positioning
		// if some touches, get average position
		if (validTouches.Length > 0) {
			Vector2 averagePosition = getTouchAverage (validTouches);
			// if was touching, then rotate otherwise this is the initial touch
			if (touching) {
				// if we have not added or removed finger, then rotate
				if (oldValidTouchCount == validTouches.Length) {
					rotateBetween (oldPosition, averagePosition);
				}
			} else {
				touching = true;
			}
			oldPosition = averagePosition;
		} else {
			touching = false;
		}
		oldValidTouchCount = validTouches.Length;





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







	Vector2 getTouchAverage(Vector2[] validTouches) {
		Vector2 ap = Vector2.zero;
		int touchCount = validTouches.Length;
		for (int i = 0; i < touchCount; i++){
			// is this within a touchpad?
			Vector2 p = validTouches[i];
			ap += p;
		}
		ap /= touchCount;
		return ap;
	}

	// get only touches in valid area
	Vector2[] getValidTouches() {
		List<Vector2> list = new List<Vector2>();
		// if touch starts in bad area, add it to ignore list
		// remove it when touch ends
		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Began) {
				if (inIgnoreArea(touch.position)) {
					touchIgnoreList.Add (touch.fingerId);
					Debug.Log ("Adding "+touch.fingerId + " to ignore");
				}
			} else if ((touch.phase == TouchPhase.Ended) || (touch.phase == TouchPhase.Canceled)) {
				Debug.Log (touch.fingerId + " removed from ignore as it has ended");
				touchIgnoreList.Remove(touch.fingerId);
			} else if (!touchIgnoreList.Contains(touch.fingerId)) {
				Debug.Log (touch.fingerId + " allowed");
				list.Add(touch.position);
			}
		}
		return list.ToArray();
	}


	bool inIgnoreArea(Vector2 p) {
		for (int y = 0; y < ignoreAreas.Length; y++){
			if (ignoreAreas[y].Contains(p)) {
				return true;
			}
		}
		return false;
	}

	void rotateBetween(Vector2 a, Vector2 b) {
		Vector2 delta = a - b;
		float rX = (delta.x / Screen.width) * (selectedCamera.fieldOfView * selectedCamera.aspect);
		transform.Rotate(new Vector3(0,rX,0));

		float rY = (delta.y / Screen.height) * selectedCamera.fieldOfView;
		selectedCamera.transform.Rotate (new Vector3 (-rY, 0, 0));

	}

}
