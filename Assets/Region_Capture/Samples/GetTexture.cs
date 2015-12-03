using UnityEngine;
using System.Collections;

public class GetTexture : MonoBehaviour {
	
	void Start () {

	if (RenderTextureCamera.CameraOutputTexture)
			GetComponent<Renderer> ().material.SetTexture ("_MainTex", RenderTextureCamera.CameraOutputTexture);

	else StartCoroutine(WaitForTexture());

	}

	private IEnumerator WaitForTexture() {

		yield return new WaitForSeconds (0.1f);

		if (RenderTextureCamera.CameraOutputTexture)
			GetComponent<Renderer> ().material.SetTexture ("_MainTex", RenderTextureCamera.CameraOutputTexture);

		else StartCoroutine(WaitForTexture());

	}

}