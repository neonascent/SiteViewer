using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class ReconstructionMenu : MonoBehaviour {
	
	public Vector2 scrollPosition = Vector2.zero;
	public Texture background;
	public Texture supportLogos;
	public int maxMenuWidth = 960;
	public Font titleFontSmall;
	public Font titleFontBig;
	public Font UIFontSmall;
	public Font UIFontBig;
	
	private string recon = "";
	private string[] recons;
	private string name;
	private int rename = -1;
	
	private int fontHeight = 20;
	private int fontWidth = 2;
	private int spacer = 20;
	private Font titleFont;
	private Font UIFont;
	private bool help = false;
			
	// Use this for initialization
	void Start () {
		
		if (Screen.currentResolution.width > maxMenuWidth) {		
			fontHeight = 40;
			titleFont = titleFontBig;
			UIFont = UIFontBig;
		} else {
			titleFont = titleFontSmall;	
			UIFont = UIFontSmall;
		}
		// number of reconstructions
		recons = listReconstructions();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI () {
		GUI.skin.label.font = titleFont;	
		GUI.skin.label.normal.textColor = Color.white;
		
		//GUI.skin.label.font = GUI.skin.button.font;
			
		GUI.skin.button.font = UIFont;
		GUI.skin.box.font = UIFont;
		GUI.skin.textField.font = UIFont;
		GUI.skin.textArea.font = UIFont;

		int line = 0;
		Vector2 textSize;
		int x;
       	 	
		// Make a background box
		
		
		Rect guiBox = new Rect(0, spacer, Screen.width, Screen.height);
		
		GUI.DrawTexture(guiBox, background, ScaleMode.ScaleAndCrop, true, 0);
		
		
		// help box
		textSize = GUI.skin.button.CalcSize(new GUIContent("?"));
		//if (GUI.Button(new Rect(Screen.width - spacer - Mathf.FloorToInt(textSize.y), spacer * 2, Mathf.FloorToInt(textSize.x), Mathf.FloorToInt(textSize.y)), "?")) {
		//	help = !help;
		//}
//		if (help) {
//			textSize = GUI.skin.label.CalcSize(new GUIContent("SiteViewer -- Josh Harle <josh.harle@gmail.com>"));
//			GUI.Label(new Rect(0, spacer, Mathf.FloorToInt(textSize.x), Mathf.FloorToInt(textSize.y)), "SiteViewer -- Josh Harle <josh.harle@gmail.com>");
//			
//			Rect helpBox = new Rect(0, Mathf.FloorToInt(textSize.y) + (spacer * 2), Screen.width, Screen.height - Mathf.FloorToInt(textSize.y) - (spacer * 4));
//			GUI.TextArea(helpBox, 
//				"Herein is part of PhD research by Josh Harle, supported under an Australian Research Council Linkage grant as part of Real-time Porosity."+
//				"\r\n\r\nThis project has benefited from an Arts & Design Grant courtesy of Arc @ UNSW Limited." +
//				"\r\n\r\nSmooth movement code by Malcolm Ryan (wordsonplay.com)." +
//				"\r\n\r\nFor more information go to http://tacticalspace.org");
//			Rect logoBox = new Rect((Screen.width/10), Screen.height-(Screen.height/5), Screen.width-(Screen.width/5), (Screen.height/7));
//			GUI.TextArea(logoBox,"");
//			GUI.DrawTexture(logoBox, supportLogos, ScaleMode.ScaleToFit, true, 0);
//			
//		} else {
			textSize = GUI.skin.label.CalcSize(new GUIContent("SiteViewer"));
			GUI.Label(new Rect(20, spacer, Mathf.FloorToInt(textSize.x), Mathf.FloorToInt(textSize.y)), "SiteViewer");
			line = spacer + Mathf.FloorToInt(textSize.y);

			GUI.skin.label.font = UIFont;	
			textSize = GUI.skin.label.CalcSize(new GUIContent("by Josh Harle <josh.harle@gmail.com>"));
			GUI.Label(new Rect(20, Mathf.FloorToInt(line), Mathf.FloorToInt(textSize.x), Mathf.FloorToInt(textSize.y)), "by Josh Harle <josh.harle@gmail.com>");
			line = line + Mathf.FloorToInt(textSize.y);
			textSize = GUI.skin.label.CalcSize(new GUIContent("Dynamically load site models.  For more information see http://tacticalspace.org"));
			GUI.Label(new Rect(20, Mathf.FloorToInt(line), Mathf.FloorToInt(textSize.x), Mathf.FloorToInt(textSize.y)), "Dynamically load site models.  For more information see http://tacticalspace.org");
			line = line + Mathf.FloorToInt(textSize.y);
			
			Rect guiScrollBox = new Rect(0, Mathf.FloorToInt(line), Screen.width, Screen.height - ((fontHeight  + spacer)* 2));
			scrollPosition = GUI.BeginScrollView(guiScrollBox, scrollPosition, new Rect(0, 0, Screen.width, 40 + (fontHeight * 2 * recons.Length)));
			
			for (int i = 0; i < recons.Length; i++) {
				x = Screen.width - spacer; // margin
				
				if (rename == i) {
					// cancel
					textSize =  GUI.skin.button.CalcSize(new GUIContent("Cancel"));
					x = x  - Mathf.FloorToInt(textSize.x);
					if (GUI.Button(new Rect(x, line, Mathf.FloorToInt(textSize.x), fontHeight), "Cancel")) {
						rename = -1;
					}
					
					// okay
					textSize = GUI.skin.button.CalcSize(new GUIContent("Okay"));
					x = x  - Mathf.FloorToInt(textSize.x) - spacer;
					if (GUI.Button(new Rect(x, line,Mathf.FloorToInt(textSize.x), fontHeight), "Okay")) {
						renameRecon(recons[i], name);
						recons = listReconstructions();
						rename = -1;
					}
					
					// edit box
					x = x - (spacer * 3);
					name = GUI.TextField(new Rect(20, line, x, fontHeight), name);
					
					
				} else {
					// delete
					textSize = GUI.skin.button.CalcSize(new GUIContent("Delete"));
					x = x  - Mathf.FloorToInt(textSize.x);
					if (GUI.Button(new Rect(x, line,Mathf.FloorToInt(textSize.x), fontHeight), "Delete")) {
						deleteRecon(recons[i]);
						recons = listReconstructions();
					}
					
//					// rename
//					textSize = GUI.skin.button.CalcSize(new GUIContent("Rename"));
//					x = x  - Mathf.FloorToInt(textSize.x) - spacer;
//					if (GUI.Button(new Rect(x, line,Mathf.FloorToInt(textSize.x), fontHeight), "Rename")) {
//						rename = i;
//						name = recons[rename];
//					}
					
					// open
					textSize = GUI.skin.button.CalcSize(new GUIContent("Open"));
					x = x  - Mathf.FloorToInt(textSize.x) - spacer;
					if (GUI.Button(new Rect(x, line,Mathf.FloorToInt(textSize.x), fontHeight), "Open")) {
						PlayerPrefs.SetString("guid",recons[i]);
						PlayerPrefs.SetInt("type",1);
						Application.LoadLevel("Download");
					}
					
					x = x - (spacer * 3);
					GUI.Box(new Rect(20, line, x, fontHeight), recons[i]);
				
					
					
				}
				line = line + fontHeight + spacer;
			}
			
			GUI.EndScrollView();
			
			// add a reconstruction or photosynth
			x = Screen.width - spacer; // margin
			
//			// add photosynth
//			textSize = GUI.skin.button.CalcSize(new GUIContent("Add PS"));
//			x = x  - Mathf.FloorToInt(textSize.x) - spacer;
//			if (GUI.Button(new Rect(x, Screen.height - fontHeight - spacer, Mathf.FloorToInt(textSize.x), fontHeight), "Add PS")) {
//				// pass this on to other scene
//				PlayerPrefs.SetString("guid",recon);
//				PlayerPrefs.SetInt("type",0);
//				Application.LoadLevel("Download");
//			}
			
			// add reconstruction
			textSize = GUI.skin.button.CalcSize(new GUIContent("Add  "));
			x = x  - Mathf.FloorToInt(textSize.x) - spacer;
			if (GUI.Button(new Rect(x, Screen.height - fontHeight - spacer, Mathf.FloorToInt(textSize.x), fontHeight), "Add")) {

				// get url and path
				recon = recon.TrimEnd('/');
				Uri myuri = new Uri(recon);
				String guid= myuri.Segments[myuri.Segments.Length-1];
				String url = recon.Substring(0,recon.Length - guid.Length);
				Debug.Log ("url: " + url);
				Debug.Log ("Guid: " + guid);

				PlayerPrefs.SetString("url",url);
				PlayerPrefs.SetString("guid",guid);
				PlayerPrefs.SetInt("type",1);
				Application.LoadLevel("Download");
			}
			
			// input name
			x = x - (spacer * 3);
			recon = GUI.TextField(new Rect(20, Screen.height - fontHeight - spacer, x, fontHeight), recon);
		//}
		
		
		
	}

		
	string[] listReconstructions() {
		string[] paths = Directory.GetDirectories(Application.persistentDataPath);
		for (int i = 0; i< paths.Length; i++) {
			int directoryBreak = (paths[i].LastIndexOf('\\') > paths[i].LastIndexOf('/'))? paths[i].LastIndexOf('\\')+ 1: paths[i].LastIndexOf('/') + 1;
			paths[i] = paths[i].Substring(directoryBreak);
		}
		return paths;
		 
	}
				
	void deleteRecon (string recon) {
		Directory.Delete(Application.persistentDataPath + "/" + recon, true);
	}
	
	void renameRecon (string recon, string newName) {
		if (recon != newName) {
			Directory.Move(Application.persistentDataPath + "/" + recon, Application.persistentDataPath + "/" + newName);
		}
	}
}
