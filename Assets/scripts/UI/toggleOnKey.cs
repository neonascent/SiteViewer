using UnityEngine;
using System.Collections;

public class toggleOnKey : MonoBehaviour {
	
	public string inputName = "Toggle Timeperiod";
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown (inputName)) {
			this.transform.GetChild(0).gameObject.SetActive(!this.transform.GetChild(0).gameObject.activeSelf);
		}
	}  
	
}
