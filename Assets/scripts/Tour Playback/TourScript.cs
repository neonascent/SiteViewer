using UnityEngine;
using System.Collections;


using UnityEngine;
using System.Collections;


[System.Serializable]
public class tourObjectScriptLine {
	public float triggerTime;
	public GameObject targetObject;
	public bool changeState;
	private bool executed = false; 

	public bool hasExecuted() {
		return executed;
	}

	public void setExecuted(bool b) {
		executed = b;
	}
}


public class TourScript : MonoBehaviour {

	public TextAsset playbackFile;
	public AudioClip audio;
	public Texture2D thumbnail;
	public tourObjectScriptLine[] actions;
}
