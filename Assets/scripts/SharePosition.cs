using UnityEngine;
using System.Collections;

public class SharePosition : MonoBehaviour {
	
	public Transform player;
	public Transform target;
	private Light pointlight;
	private string saveString;
	private bool showBox;

	// Use this for initialization
	void Start () {
		// if thing showing then 
		pointlight = Camera.main.GetComponent<Light>();
	}

	
	string GetPositionURL() {

		//Debug.Log("push!");
		Ray ray = Camera.main.ScreenPointToRay (new Vector2((Screen.width /2),(Screen.height /2)));
		RaycastHit hit;
		Vector3 destination;

		if(Physics.Raycast(ray, out hit, 100)){
			destination = hit.point;
			target.transform.position = destination;
		}

		string objPos = "0," + player.position.x + "," + player.position.y  + "," + player.position.z + "," + player.rotation.x + "," + player.rotation.y  + "," + player.rotation.z + "," + player.rotation.w + "," + target.position.x + "," + target.position.y  + "," + target.position.z;
		return Application.absoluteURL + "?sharedview=" + objPos;

	}
	
	void Update() {
		if(Input.GetKeyDown(KeyCode.U)) {
			saveString = GetPositionURL();
			showBox = true;
			pointlight.enabled = true;
		}
	}


	void OnGUI() {
		if (showBox) {
			int multiplier =  (Screen.currentResolution.width > 960)?2:1;
			// Make a background box
			int boxWidth = 530 * multiplier;
			int boxHeight = 90 * multiplier;
			int boxTop = (Screen.height-boxHeight)/2;
			Rect guiBox = new Rect((Screen.width-boxWidth)/2, boxTop, boxWidth, boxHeight);
			GUI.Box(guiBox, "Share this URL:");
			
			int titleWidth = 490 * multiplier;
			int titleHeight = 20 * multiplier;
			
			GUI.TextField(new Rect((Screen.width-titleWidth)/2, 20 + boxTop + 10, titleWidth,titleHeight), saveString);
			
			int buttonWidth = 100 * multiplier;
			int buttonHeight = 20 * multiplier;
			//GUI.TextField(new Rect(20,35, 490,20), Application.persistentDataPath.ToString());
			
			if (GUI.Button(new Rect((Screen.width-buttonWidth)/2,20 + boxTop + 10 + titleHeight + 10,buttonWidth,buttonHeight), "Copy to Clipboard and Close")) {
				TextEditor te = new TextEditor();
				te.content = new GUIContent(saveString);
				te.SelectAll();
				te.Copy();

				showBox = false;
				pointlight.enabled = false;
				target.transform.position = Vector3.zero;
			}
		}



	}

}

