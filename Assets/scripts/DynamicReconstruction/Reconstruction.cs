using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reconstruction
{
	public class Reconstruction
	{
		public Reconstruction() {
			models = new List<RModel> ();
		}
		
		public string guid { get; set; }
		public string format = "JoshReconstruction";
		public string version = "0.8";
		public string remoteURL { get; set; }
		public RPosition position { get; set; }
		public ROrientation rotation { get; set; }
		public RScale scale { get; set; }
		public int model_count  { get; set; }
		public List<RModel> models  { get; set; }

	}
	
	public class RModel {
		public RModel() {
			model = "";
			ShaderName = "";
			parallax = .0f;
			textures = new List<ShaderTexture> ();
		}
		
		public string model { get; set; }
		public string ShaderName { get; set; }
		public float parallax { get; set; }
		public List<ShaderTexture> textures { get; set; }
		
	}

	public class ShaderTexture {
		public string name { get; set; }
		public string texture { get; set; }
		public int compression { get; set; }
		public int resolution { get; set; }
	}
	
	public class RPosition {
		public float x { get; set; }
		public float y { get; set; }
		public float z { get; set; }
	}
	
	public class ROrientation {
		public float qx { get; set; }
		public float qy { get; set; }
		public float qz { get; set; }
		public float qw { get; set; }
	}

	public class RScale {
		public float sx { get; set; }
		public float sy { get; set; }
		public float sz { get; set; }
	}
}