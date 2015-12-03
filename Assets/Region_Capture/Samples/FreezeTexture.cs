using UnityEngine;
using System.Collections;

public class FreezeTexture : MonoBehaviour {
	
	[Space(20)]
	public bool FreezeEnable = false;

	void Start () {
		if (FreezeEnable) Region_Capture.SwitchCheckMarkerPosition = true;
	}

	void LateUpdate () {
	
		if (Region_Capture.MarkerIsOUT && FreezeEnable && RenderTextureCamera.CameraOutputTexture)
			RenderTextureCamera.Render_Texture_Camera.GetComponent<Camera> ().enabled = false;

		if (Region_Capture.MarkerIsIN && FreezeEnable && RenderTextureCamera.CameraOutputTexture)
			RenderTextureCamera.Render_Texture_Camera.GetComponent<Camera> ().enabled = true;

	}
	
	void OnValidate(){
		
		#if UNITY_EDITOR
		if (FreezeEnable) Region_Capture.SwitchCheckMarkerPosition = true;
		#endif
		
	}
}