using UnityEngine;
using System.Collections;

public class PlatformBehaviour : MonoBehaviour {
	
	public MouseLook mouseLookX;
	public MouseLook mouseLookY;
	public touchLook touchLook;
	public mouseZoom mouseZoom;
	public GameObject leftTouchPad;
	public PlaybackGuide tourPlayer;
	public GameObject iPadIntroGuide;
	public float        fAlpha;

	public float iPadSensitivity = 0.5f;

	private bool showIPADHelp = false;

	// Use this for initialization
	void Start () {



		// turn off mouse look for ipad
		if ((Application.platform == RuntimePlatform.IPhonePlayer) || (Application.platform == RuntimePlatform.Android)) {
			// set things
			mouseLookX.enabled = false;
			mouseLookY.enabled = false;
			mouseZoom.enabled = false;
			leftTouchPad.SetActive(true);
			touchLook.enabled = true;
			showIPADHelp = true;
			

		} else {
			mouseLookX.enabled = true;
			mouseLookY.enabled = true;
			mouseZoom.enabled = true;
			leftTouchPad.SetActive(false);
			touchLook.enabled = false;
			showIPADHelp = false;
		}
	}
	
	// Update is called once per frame
	void Update () {

		

	}

	void OnGUI() {
		if (showIPADHelp) {
			tourPlayer.loadTour(iPadIntroGuide);
			tourPlayer.StartPlayback();
			showIPADHelp = false;
		}
	}
	

}
