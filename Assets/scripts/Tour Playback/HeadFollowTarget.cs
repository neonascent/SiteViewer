using UnityEngine;
using System.Collections;

public class HeadFollowTarget : MonoBehaviour {

	public float snapback = 2f;
	public bool debug = false;
	public Transform target;
	private Quaternion startRotation;

	// Use this for initialization
	void Start () {
		startRotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Quaternion neededRotation;

		if (target.localPosition == Vector3.zero) {
			neededRotation = transform.parent.rotation * startRotation;
		} else {
			//calculate the rotation needed needed
			neededRotation = Quaternion.LookRotation(target.position-transform.position);
		}

		if (transform.rotation != neededRotation) {			
			//use spherical interpollation over time interpolated
			transform.rotation = Quaternion.Slerp(transform.rotation, neededRotation, Time.deltaTime * snapback);
		}

	}
}

