using UnityEngine;
using System.Collections;

public class IgnoreUnplacedTarget : MonoBehaviour {
	
	public float transition = 0.7f;
	public bool debug = false;
	public GameObject torch;
	private IKLimb effectComponent;
	private Transform target;
	private Light torchlight;

	
	// Use this for initialization
	void Start () {
		effectComponent = GetComponent<IKLimb>();
		target = effectComponent.target;
		torchlight = torch.GetComponent<Light>();
	}

	// Update is called once per frame, not enough???
	void FixedUpdate () {
		if (target.localPosition == Vector3.zero) {
			effectComponent.enabled = false;
			torchlight.enabled = false;
			if (debug) { Debug.Log("Deactivated IK"); }
		} else {
			effectComponent.enabled = true;
			torchlight.enabled = true;
			if (debug) {Debug.Log("Activated IK"); }
		}

	}
}
