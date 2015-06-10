using UnityEngine;
using System.Collections;

public class DownloadStage : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int type = PlayerPrefs.GetInt("type");
		string guid = PlayerPrefs.GetString("guid");
		string url = PlayerPrefs.GetString("url");
//		if (type == 0) { // photosynth
//			GetComponent<downloadPhotoSynth>().guid = guid;
//			GetComponent<downloadPhotoSynth>().enabled = true;
//			Destroy(this, 0);
//		} else
		if (type == 1) { // recon
			GetComponent<downloadBundle>().guid = guid;
			GetComponent<downloadBundle>().bundleLocation = url;
			GetComponent<downloadBundle>().enabled = true;
			Destroy(this, 0);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
