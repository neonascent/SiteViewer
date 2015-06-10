using UnityEngine;
using System.Collections;
using UnityEngine.UI;




public class PlaybackGuide : MonoBehaviour {
	
	public TextAsset playbackFile;
	public Transform guide;
	public Renderer guideMeshComponent;
	public Transform target;
	public float AudioOffset = 0.0f;
	public AudioClip audio;
	public Texture2D thumbnail;
	public GUISkin guiskin;
	public bool guideLabel = false;
	public GUIStyle play_button;
	public GUIStyle cancel_button;
	public GUIStyle firstperson_button;
	public GUIStyle thirdperson_button;
	public GameObject tourMenu;
	public GameObject firstPersonController;
	public Slider tourPositionSlider;
	// toggle views and audio
	public Camera mainEyes;
	public Camera guideEyes;
	public AudioListener mainEars;
	public AudioListener guideEars;
	public tourObjectScriptLine[] objectScript;


	
	private GameObject guideStartPosition;
	
	private int lastIndex = 0;
	private string lastLine;
	private string[] recording;
	private float lastTime = 0f;
	private bool playing = false;
	private bool snapshot = false;
	private AudioSource audioguide = null; 
	private GUITexture thumbimage;
	private string commands = "";

	private bool firstPerson = true;


	// Use this for initialization
	void Start () {

		guideStartPosition = new GameObject ();
		guideStartPosition.transform.position = guide.position;
		guideStartPosition.transform.localPosition = guide.localPosition;
		guideStartPosition.transform.rotation = guide.rotation;
		guideStartPosition.transform.localRotation = guide.localRotation;


		resetData ();


		// if url parameters, then load

		if (PreviewLabs.RequestParameters.HasKey("sharedview")) {
			LoadSnapshotPosition(PreviewLabs.RequestParameters.GetValue("sharedview"));
			Debug.Log (PreviewLabs.RequestParameters.GetValue("sharedview"));
		}


		// share view enabled?
		if (((SharePosition)transform.GetComponent<SharePosition>()).enabled) {
			commands = commands + "\r\nU = Share View as URL";
		}

	}

	public void resetData() {
		audioguide = guide.GetComponent<AudioSource>();
		audioguide.clip = audio;
		
		string temp = playbackFile.text;
		Debug.Log ("File: " + temp);
		recording = temp.Replace("\r\n", "\n").Replace("\r","\n").Split("\n"[0]);
		Debug.Log ("Playback: " + recording.Length);
		
		Transform thumb = transform.FindChild("Thumbnail").transform;
		Vector3 imagescale = new Vector3((((float)thumbnail.width) / ((float)Screen.width)), (((float)thumbnail.height)/((float)Screen.height)), 1);
		thumb.localScale = imagescale;
		thumbimage = ((GUITexture)thumb.GetComponent<GUITexture>());
		thumbimage.texture = thumbnail;
		

		// tour position
		tourPositionSlider.maxValue = audioguide.clip.length;
		tourPositionSlider.value = 0;

		// reset thumbnail position and guide visibility
		guideMeshComponent.enabled = false;
		thumbimage.enabled = false;

		audioguide.volume = 1;

		// do actions
		foreach (tourObjectScriptLine a in objectScript) {
			a.setExecuted(false);
		}

		// guide position
		guide.position = guideStartPosition.transform.position;
		guide.rotation = guideStartPosition.transform.rotation;
	}

	public void openTourMenu() {
		tourMenu.SetActive(true);
		firstPersonController.SetActive (false);
	}

	public void StartPlayback() {
		// reset positions
		lastIndex = 0;

		// start audio
		audioguide.Play();

		// read first 
		lastLine = recording[lastIndex];
		lastTime = GetTime(lastLine);

		playing = true;
		snapshot = false;
		thumbimage.enabled = true;
		guideMeshComponent.enabled = true;

		// tour position
		tourPositionSlider.transform.parent.gameObject.SetActive(true);
	}

	public void SetTourPosition(float s) {
		if ((audioguide != null) && (audioguide.clip.loadState == AudioDataLoadState.Loaded)) {
			if (System.Math.Abs(audioguide.time-s) > 0.5) {
				if (s < audioguide.clip.length)		{
					audioguide.time = s;	
					// reset positions
					lastIndex = 0;
					// read first 
					lastLine = recording[lastIndex];
					lastTime = GetTime(lastLine);
					Debug.Log (s.ToString ());

					// reset actions
					foreach (tourObjectScriptLine a in objectScript) {
						if ((a.triggerTime + AudioOffset) < audioguide.time) {
							a.targetObject.gameObject.SetActive(a.changeState);
							a.setExecuted(true);
						} else {
							a.setExecuted(false);
						}
					}

				}
			}
		}
	}

	public void StopPlayback() {

		// stop audio
		audioguide.Stop();
		target.position = Vector3.zero;

		// kill object
		//if (player)
		playing = false;
		snapshot = false;
		thumbimage.enabled = false;
		SetFirstPerson(true); // switch back to first person
		guideMeshComponent.enabled = false;

		// tour position
		tourPositionSlider.transform.parent.gameObject.SetActive(false);

		// run through final actions to the end

		// do actions
		foreach (tourObjectScriptLine a in objectScript) {
			if (!a.hasExecuted()) {
				a.targetObject.gameObject.SetActive(a.changeState);
				a.setExecuted(true);
			}
		}
	}
	
	
	void OnGUI() {
		GUI.skin = guiskin;
		if (!playing) {
			//if (GUI.Button(new Rect((Screen.width /2) - 75, Screen.height - 70, 150, 30), "Play Guided Tour (P)")) 
			if (!tourMenu.activeSelf)
			if (GUI.Button(new Rect((Screen.width /2) - (Screen.width / 10) - 5, Screen.height - (Screen.width / 10)-10, (Screen.width / 10), (Screen.width / 10)), "P", play_button)) 
				openTourMenu();
		} else {
			// display thumbnail if playing
			GUI.DrawTexture (new Rect (Screen.width - thumbnail.width, Screen.height -thumbnail.height, thumbnail.width, thumbnail.height), thumbnail);
			if (snapshot) {
				//if (GUI.Button(new Rect((Screen.width /2) - 75, Screen.height - 70, 150, 30), "Hide Guide (P)"))
				if (GUI.Button(new Rect((Screen.width /2) - (Screen.width / 10) - 5, Screen.height - (Screen.width / 10)-10, (Screen.width / 10), (Screen.width / 10)), "P", cancel_button)) 
					StopPlayback();
			} else {
				//if (GUI.Button(new Rect((Screen.width /2) - 75, Screen.height - 70, 150, 30), "Stop Tour (P)"))
				if (GUI.Button(new Rect((Screen.width /2) - (Screen.width / 10) - 5, Screen.height - (Screen.width / 10)-10, (Screen.width / 10), (Screen.width / 10)), "P", cancel_button)) 
					StopPlayback();
			}

			// is first?
			if (firstPerson) {
				if (GUI.Button(new Rect((Screen.width /2) + 5, Screen.height - (Screen.width / 10)-10, (Screen.width / 10), (Screen.width / 10)), "T", firstperson_button)) 
					SetFirstPerson(!firstPerson);  // ToggleView 
			} else {
				if (GUI.Button(new Rect((Screen.width /2) + 5, Screen.height - (Screen.width / 10)-10, (Screen.width / 10), (Screen.width / 10)), "T", thirdperson_button)) 
					SetFirstPerson(!firstPerson);  // ToggleView 
			}
			//if (GUI.Button(new Rect((Screen.width /2) - 75, Screen.height - 40, 150, 30), "Toggle View (T)")) 

		}

		GUI.Label(new Rect((Screen.width /2) - 50, Screen.height - 40, 150, 50), commands);
	}


	void SetFirstPerson(bool status) {
		firstPerson = status;
		mainEars.enabled = status;
		mainEyes.enabled = status;
		guideEars.enabled = !status;
		guideMeshComponent.enabled = status;
		guideEyes.enabled = !status;
	}


	// Update is called once per frame
	void Update () {
		if (playing) {
			// toggle
			if(Input.GetKeyDown(KeyCode.T)) {
				SetFirstPerson(!firstPerson);  // ToggleView 
			}

			// stop
			if(Input.GetKeyDown(KeyCode.P)) {
				StopPlayback();
			}

			// if not snapshot, then update
			if (!snapshot) {
				// if we are at the end 
				// or if the last position has now happened
				if ((lastIndex == recording.Length - 1)|| ((lastTime + AudioOffset) > audioguide.clip.length)) {
					StopPlayback();
				} else {
					// if the last position has now happened
					if ((lastTime + AudioOffset) < audioguide.time) {
						// move
						UpdatePositions(lastLine);
						tourPositionSlider.value = audioguide.time;

						// do actions
						foreach (tourObjectScriptLine a in objectScript) {
							if (!a.hasExecuted()) {
								if ((a.triggerTime + AudioOffset) < audioguide.time) {
									a.targetObject.gameObject.SetActive(a.changeState);
									a.setExecuted(true);
								}
							}
						}


						//Debug.Log (lastLine);
						// get next line that is in the future
						//lastLine = recording[lastIndex++];
						//lastTime = GetTime(lastLine);
						while ((lastIndex < recording.Length) && ((lastTime + AudioOffset) < audioguide.time) && ((lastTime + AudioOffset) < audioguide.clip.length)) {
							lastLine = recording[lastIndex++];
							lastTime = GetTime(lastLine);
						}
					// wait for it to happen
					// and end if out of lines?
					}  
				}
			}
		} else {
			// start
			if(Input.GetKeyDown(KeyCode.P)) {
				openTourMenu();
			}
		}
	}


	public void loadTour(GameObject to) {
		Debug.Log ("LoadTOur");

		// move tour reference 
		this.transform.parent.position = to.transform.position;
		this.transform.parent.rotation = to.transform.rotation;
		this.transform.parent.localScale = to.transform.localScale;

		Debug.Log (to.transform.position);

		TourScript s = to.GetComponent<TourScript> ();
		// load settings
		thumbnail = s.thumbnail;
		audio = s.audio;
		playbackFile = s.playbackFile;
		objectScript = s.actions;
		resetData();
	}


	public void SetThumbnail(Texture2D t) {
		thumbnail = t;
	}
	
	public void SetTourAudio(AudioClip a) {
		audio = a;
	}
	
	public void SetTourMovementFile(TextAsset f) {
		playbackFile = f;
	}

	float GetTime(string l) {
		string[] segments = l.Split (',');
		float t = float.MaxValue;
		try {
			t = float.Parse(segments[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}
		catch (System.FormatException e) {
			Debug.Log (e);
		}
		return t;
	}


	void UpdatePositions(string l) {
		if (l != null) {
			//Debug.Log (l);
			string[] segments = l.Split (',');
			if (segments.Length == 11) {
				Vector3 characterPosition = new Vector3 (float.Parse (segments [1]), float.Parse (segments [2]), float.Parse (segments [3]));
				Quaternion characterRotation = new Quaternion (float.Parse (segments [4]), float.Parse (segments [5]), float.Parse (segments [6]), float.Parse (segments [7]));
				Vector3 targetPosition = new Vector3 (float.Parse (segments [8]), float.Parse (segments [9]), float.Parse (segments [10]));


				// modified to be relative to TourSystem
				guide.localPosition = characterPosition;
				guide.localRotation = characterRotation;
				target.localPosition = targetPosition;
			}
		}
	}


	void LoadSnapshotPosition(string p) {
		// comments
		playing = true;
		snapshot = true;
		guideMeshComponent.enabled = true;

		UpdatePositions(p);
		SetFirstPerson(false);
	}



}
