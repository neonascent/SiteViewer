using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class QualitySetting : MonoBehaviour {
	
	public GameObject QualitySlider;
	public GameObject text;
	public GameObject canvas;
	public string nextLevel = "Record";

	private Slider s;
	private Text t;
	private QualityLevel q;


	// Use this for initialization
	void Start () {
		s = QualitySlider.GetComponent<Slider> ();
		t = text.GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		QualitySettings.SetQualityLevel ((int)s.value);
		t.text = ((QualityLevel)QualitySettings.GetQualityLevel()).ToString ();
	}

	public void loadLevel(){
		Canvas c = canvas.GetComponent<Canvas> ();
		c.enabled = false;
		Application.LoadLevel (nextLevel); 
	}
}
