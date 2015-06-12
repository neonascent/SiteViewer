using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

[XmlRoot("document")]
public class document
{
	[XmlAttribute("version")]
	public string version;
	
	public chunk chunk = new chunk();
}

public class chunk
{
	[XmlArray("cameras"),XmlArrayItem("camera")]
	public List<camera> cameras = new List<camera>();
}

// camera class
public class camera
{
	[XmlAttribute("label")]
	public string label;
	
	[XmlAttribute("id")]
	public string id;
	
	[XmlAttribute("enabled")]
	public string enabled;
	
	[XmlAttribute("sensor_id")]
	public string sensor_id;
	
	public string transform;
	public string orientation;
	
}











public class loadMotionFromXML : EditorWindow {
	
	
	// xml location 
	public string  inputXMLfilename = 		@"G:\Josh\2015\UWA\Unity Projects\secret-ninja\Assets\Objects\tree\cameras.xml";
	public string  outputTourFile = 		@"G:\Josh\2015\UWA\Unity Projects\secret-ninja\Assets\Objects\tree\tour.txt";
	public int framerate = 4;
	public bool showMotion = false;
	
	
	private Camera cameraProxy = null;
	
	// xml variables
	private document container = null;
	private float delta = 0;
	private float time = 0;
	private string saveString = "";
	private Vector3 tourposition = Vector3.zero;
	private Quaternion tourrotation = Quaternion.identity;
	private int xmlIndex = 0;
	private int cameraCount = 0;

	private RenderTexture tourPreview = null;
	
	private bool moreXML = false;
	
	private char[] charsToTrim = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
	
	// Add menu item named "My Window" to the Window menu
	[MenuItem("TacticalSpace/Import/Tour Motion from XML", false, 10)]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow e = EditorWindow.GetWindow(typeof(loadMotionFromXML));
		e.autoRepaintOnSceneChange = true;
		e.title = "Import Tour";
	}
	
	
	void OnGUI()
	{
		GUILayout.Label ("Import Tour Motion from XML", EditorStyles.boldLabel);
		
		cameraProxy = (Camera)EditorGUILayout.ObjectField (cameraProxy, typeof(Camera), true);
		
		inputXMLfilename = EditorGUILayout.TextField ("XML filename", inputXMLfilename);
		outputTourFile = EditorGUILayout.TextField ("Tour filename", outputTourFile);
		framerate = EditorGUILayout.IntSlider("Framerate",  framerate, 1, 120);
		//showMotion = EditorGUILayout.Toggle("Show Motion while importing", showMotion);
		
		
		EditorGUI.BeginDisabledGroup (File.Exists(inputXMLfilename));
		if( GUILayout.Button("test export XML")) {
			exportTestXML(inputXMLfilename);
		}
		EditorGUI.EndDisabledGroup ();



		EditorGUI.BeginDisabledGroup (true);
		xmlIndex = EditorGUILayout.IntSlider("Frame",  xmlIndex, 0, cameraCount);
		EditorGUI.EndDisabledGroup ();


		//EditorGUI.BeginDisabledGroup (!File.Exists(inputXMLfilename) || (guide == null));
		EditorGUI.BeginDisabledGroup (!File.Exists(inputXMLfilename));
		if( GUILayout.Button("Load XML")) {
			loadXML();
		}
		EditorGUI.EndDisabledGroup ();
		EditorGUI.BeginDisabledGroup (!moreXML);
		if (GUILayout.Button("Get next position")) {
			nextXML ();
		}
		if (GUILayout.Button ("Play")) {
			showMotion = !showMotion;
		}
		EditorGUI.EndDisabledGroup ();


		if (cameraProxy != null) {
			GUI.DrawTexture (new Rect (.0f, 200f, position.width, position.height-200f), tourPreview);
		}

	}

	void Awake() {
		CreateRenderTexture ();
	}

	void CreateRenderTexture () {
		tourPreview = new RenderTexture( Mathf.RoundToInt(position.width), Mathf.RoundToInt(position.height), (int)RenderTextureFormat.ARGB32);
	}


	
	void loadXML() {
		XmlSerializer serializer = new XmlSerializer(typeof(document));
		FileStream stream = new FileStream(inputXMLfilename, FileMode.Open);
		container = serializer.Deserialize(stream) as document;
		stream.Close();
		xmlIndex = 0;
		
		if (container.chunk.cameras.Count > 0) moreXML = true;
		
		delta = 1.0f / framerate;
		time = 0;
		saveString = "";
		tourposition = Vector3.forward;
		tourrotation = Quaternion.identity;
		cameraCount = container.chunk.cameras.Count;
		Debug.Log ("XML loaded: " + cameraCount + " cameras");

	}
	
	
	void nextXML() {
		
		if (xmlIndex < cameraCount) {
			camera c = container.chunk.cameras[xmlIndex];
			Debug.Log ("Processing Camera " + c.id);
			
			
			//			// skip unaligned cameras
			//			if (c.transform != null) {
			//				Matrix4x4 m = transformToMatrix(c.transform);
			//				position = matrixToPosition(m);
			//				rotation = matrixToRotation(m);
			//				rotation = Quaternion.Euler(Vector3.up * 180) * rotation; 
			//			}
			
			// skip unaligned cameras
			if (c.transform != null) {
				Matrix4x4 m = transformToMatrix (c.transform);
				tourposition = matrixToPosition (m);
				tourposition = new Vector3(-tourposition.x, -tourposition.y, tourposition.z);
				//rotation = matrixToRotation (m);
				//rotation = Quaternion.Euler (Vector3.forward * 180) * rotation;
				
				tourrotation = ExtractRotationFromMatrix(m);
			}
			
			// //add time since program start and position and rotation of the recorded object to the string, and position of target
			// minus to rotate
			string objPos = time + "," + tourposition.x + "," + tourposition.y + "," + tourposition.z + "," + tourrotation.x + "," + tourrotation.y + "," + tourrotation.z + "," + tourrotation.w + ",0,0,0";
			
			//keep adding a new line with information for each save
			saveString = saveString + objPos + "\r\n";
			
			// increment time
			time += delta;
			
			// if playing motion
			//gizmo.transform.localPosition = 
			
			cameraProxy.transform.localPosition = tourposition;
			cameraProxy.transform.localRotation = tourrotation;
			
			
			xmlIndex++;
		} else {
			// write everything to file
			string f = UniqueFilename (outputTourFile);
			
			System.IO.StreamWriter writer = new System.IO.StreamWriter (f); // Does this work?
			writer.Write (saveString);
			writer.Close ();
			moreXML = false;
			Debug.Log ("Done!");
		}
	}
	
	
	string UniqueFilename(string f) {
		int i = 0;
		while (System.IO.File.Exists(f)) {
			string fs = f.Substring(0,f.Length - 4);
			fs = fs.TrimEnd(charsToTrim);
			f = fs + i.ToString() + ".txt";
			i++;
		}	
		return f;
		
	}
	
	Matrix4x4 transformToMatrix(string t) {
		string[] parts = t.Split (' ');
		
		Matrix4x4 m = new Matrix4x4 ();
		m.m00 = float.Parse(parts[0], CultureInfo.InvariantCulture.NumberFormat);
		m.m01 = float.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat);
		m.m02 = float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat);
		m.m03 = float.Parse(parts[3], CultureInfo.InvariantCulture.NumberFormat);
		m.m10 = float.Parse(parts[4], CultureInfo.InvariantCulture.NumberFormat);
		m.m11 = float.Parse(parts[5], CultureInfo.InvariantCulture.NumberFormat);
		m.m12 = float.Parse(parts[6], CultureInfo.InvariantCulture.NumberFormat);
		m.m13 = float.Parse(parts[7], CultureInfo.InvariantCulture.NumberFormat);
		m.m20 = float.Parse(parts[8], CultureInfo.InvariantCulture.NumberFormat);
		m.m21 = float.Parse(parts[9], CultureInfo.InvariantCulture.NumberFormat);
		m.m22 = float.Parse(parts[10], CultureInfo.InvariantCulture.NumberFormat);
		m.m23 = float.Parse(parts[11], CultureInfo.InvariantCulture.NumberFormat);
		m.m30 = float.Parse(parts[12], CultureInfo.InvariantCulture.NumberFormat);
		m.m31 = float.Parse(parts[13], CultureInfo.InvariantCulture.NumberFormat);
		m.m32 = float.Parse(parts[14], CultureInfo.InvariantCulture.NumberFormat);
		m.m33 = float.Parse(parts[15], CultureInfo.InvariantCulture.NumberFormat);
		
		return m;
	}
	
	/// <summary>
	/// Extract rotation quaternion from transform matrix.
	/// </summary>
	/// <param name="matrix">Transform matrix. This parameter is passed by reference
	/// to improve performance; no changes will be made to it.</param>
	/// <returns>
	/// Quaternion representation of rotation transform.
	/// </returns>
	public Quaternion ExtractRotationFromMatrix(Matrix4x4 matrix) {
		Vector3 forward;
		forward.x = -matrix.m02; // - 
		forward.y = matrix.m12; // +
		forward.z = matrix.m22; // +
		
		Vector3 upwards;
		upwards.x = -matrix.m01; // - 
		upwards.y = -matrix.m11;  //-
		upwards.z = matrix.m21; // +
		
		return Quaternion.LookRotation(forward, upwards);
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	Vector3 matrixToPosition (Matrix4x4 m) {
		Vector3 v = Vector3.zero;
		
		// Find our current location in the camera's projection space.
		return m.MultiplyPoint(v);
	}
	
	void exportTestXML(string f) {
		camera camera = new camera();
		camera.label = 		"GOPR42863500.jpg";
		camera.id 	=		"0";
		camera.enabled =	"true";
		camera.sensor_id = 	"0";
		camera.transform = 	"9.7777159160850247e-001 6.6405210378488858e-002 1.9887951799510312e-001 3.9895954075435107e+001 -5.1202620277217573e-002 9.9542750570003768e-001 -8.0637290210847576e-002 -4.3891185974191007e+000 -2.0332487875349387e-001 6.8661699151637820e-002 9.7670085735064605e-001 -1.4292909338802835e+001 0.0000000000000000e+000 0.0000000000000000e+000 0.0000000000000000e+000 1.0000000000000000e+000";
		camera.orientation ="1";
		
		List<camera> cameras = new List<camera>();
		cameras.Add(camera);
		
		chunk chunk = new chunk();
		chunk.cameras = cameras;
		document document = new document();
		document.chunk = chunk;
		
		
		XmlSerializer serializer = new XmlSerializer(typeof(document));
		TextWriter textWriter = new StreamWriter(f);
		serializer.Serialize(textWriter, document);
		textWriter.Close();
		
	}
	
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {		
		// play frames if showmotion is on
		 
		if (showMotion && moreXML) {
			nextXML();
		}

		if(cameraProxy != null) {
			cameraProxy.targetTexture = tourPreview;
			cameraProxy.Render();
			cameraProxy.targetTexture = null;    
		}
		if(tourPreview.width != position.width || tourPreview.height != position.height)
			tourPreview = new RenderTexture(Mathf.RoundToInt(position.width), Mathf.RoundToInt(position.height),(int) RenderTextureFormat.ARGB32);
	}
}
