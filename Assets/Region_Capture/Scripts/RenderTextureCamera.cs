using UnityEngine;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using Vuforia;

public class RenderTextureCamera : MonoBehaviour {

	[Space(20)]
	public int TextureResolution = 512;
	[Space(10)]
	public Texture MarkerBackground;
    private string screensPath;
    private int TextureResolutionX;
	private int TextureResolutionY;
	[Space(20)]
	public bool ShowTexture = false;
	public static Camera ARCamera_Camera;
	public static Camera Render_Texture_Camera;
	public static GameObject Film_Plane;
	public static GameObject DebugGUITexture;
	public static RenderTexture CameraOutputTexture;
	public static Material Film_Textured;

	void Start() {

		Film_Textured = new Material (Shader.Find("Unlit/Texture"));

		StartCoroutine(StartRenderingToTexture());

	}

		IEnumerator StartRenderingToTexture() {

		yield return new WaitForSeconds(0.5f);

		Render_Texture_Camera = GetComponentInChildren<Camera>();
		Render_Texture_Camera.GetComponent<Camera>().orthographicSize = transform.localScale.z * 5;

		if (transform.localScale.x >= transform.localScale.z) {

			TextureResolutionX = TextureResolution;
			TextureResolutionY = (int)(TextureResolution * transform.localScale.z / transform.localScale.x);
		}

		if (transform.localScale.x < transform.localScale.z) {

			TextureResolutionX =  (int)(TextureResolution * transform.localScale.x / transform.localScale.z);
			TextureResolutionY = TextureResolution;
		}

		CameraOutputTexture = new RenderTexture(TextureResolutionX, TextureResolutionY, 0);
		CameraOutputTexture.Create();
		Render_Texture_Camera.GetComponent<Camera>().targetTexture = CameraOutputTexture;

		gameObject.layer = 20;
		Render_Texture_Camera.cullingMask = 1 << 20;

			Film_Plane = new GameObject ("Film_Plane");
			Film_Plane.transform.parent = transform;
			Film_Plane.transform.localPosition = new Vector3 (0.0f, -4.0f, 0.0f);
			Film_Plane.transform.localScale = Vector3.one;
			Film_Plane.transform.localEulerAngles = new Vector3 (0.0f, 180.0f, 0.0f);
			Film_Plane.AddComponent<MeshFilter> ();
			Film_Plane.GetComponent<MeshFilter> ().sharedMesh = GetComponent<MeshFilter> ().sharedMesh;
			Film_Plane.AddComponent<MeshRenderer> ();
			Film_Plane.GetComponent<Renderer> ().sharedMaterial = Film_Textured;
			Film_Plane.GetComponent<Renderer> ().sharedMaterial.SetTexture("_MainTex", MarkerBackground);
			Film_Plane.GetComponent<Renderer> ().shadowCastingMode = 0;
			Film_Plane.GetComponent<Renderer> ().useLightProbes = false;
			Film_Plane.GetComponent<Renderer> ().reflectionProbeUsage = 0;
			Film_Plane.GetComponent<Renderer> ().receiveShadows = false;
			Film_Plane.layer = 20;


		StartCoroutine(ShowTextureOnGUI());

	}


    IEnumerator ShowTextureOnGUI() {

		if (ShowTexture) {

			if (!DebugGUITexture) {
				DebugGUITexture = new GameObject ("Debug GUI Texture");
				GUITexture DebugTexture = DebugGUITexture.AddComponent<GUITexture> ();
				DebugTexture.color = Color.gray;
				DebugTexture.texture = CameraOutputTexture;

				ARCamera_Camera = VuforiaManager.Instance.ARCameraTransform.GetComponentInChildren<Camera>();
				float GuiTextureAspect = TextureResolutionX / (TextureResolutionY * ARCamera_Camera.aspect);

				DebugGUITexture.transform.localScale = new Vector3 (0.3f * GuiTextureAspect, 0.3f, 0.3f);				
				DebugGUITexture.transform.position = new Vector3 ((1.0f - (0.3f * GuiTextureAspect / 2)) - 0.1f * (1 / ARCamera_Camera.aspect), 0.25f, 0.0f);
			}
			else {
				
				float GuiTextureAspect = TextureResolutionX / (TextureResolutionY * ARCamera_Camera.aspect);

				DebugGUITexture.transform.localScale = new Vector3 (0.3f * GuiTextureAspect, 0.3f, 0.3f);				
				DebugGUITexture.transform.position = new Vector3 ((1.0f - (0.3f * GuiTextureAspect / 2)) - 0.1f * (1 / ARCamera_Camera.aspect), 0.25f, 0.0f);
			}

		} 

		else {
			if (DebugGUITexture) Destroy (DebugGUITexture);
		}
		yield return null;
	}


	public void RecalculateTextureSize() {
		StartCoroutine(RecalculateRenderTexture());
	}

	private IEnumerator RecalculateRenderTexture() {

		yield return new WaitForEndOfFrame();

		CameraOutputTexture.Release();
		CameraOutputTexture = null;
		if (DebugGUITexture) Destroy (DebugGUITexture);
		if (Film_Plane) Destroy (Film_Plane);

		StartCoroutine(StartRenderingToTexture());

	}


	void OnValidate(){
		
		#if UNITY_EDITOR
		if (Application.isPlaying) {
			if 	(GetComponent<RenderTextureCamera>().isActiveAndEnabled) StartCoroutine(ShowTextureOnGUI());
		}
		#endif
		
	}
	

    public void MakeScreen() {

        if (screensPath == null) {

		#if UNITY_ANDROID && !UNITY_EDITOR
			screensPath = "/sdcard/DCIM/RegionCapture";

		#elif UNITY_IPHONE && !UNITY_EDITOR
			screensPath = Application.persistentDataPath;

		#else
            screensPath = Application.dataPath + "/Screens";

		#endif
            System.IO.Directory.CreateDirectory(screensPath);
        }

        StartCoroutine(TakeScreen());
    }

    private IEnumerator TakeScreen() {

        yield return new WaitForEndOfFrame();

        Texture2D FrameTexture = new Texture2D(CameraOutputTexture.width, CameraOutputTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = CameraOutputTexture;
        FrameTexture.ReadPixels(new Rect(0, 0, CameraOutputTexture.width, CameraOutputTexture.height), 0, 0);
        RenderTexture.active = null;

        FrameTexture.Apply();
        saveImgToGallery(FrameTexture.EncodeToPNG());

    }

	#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern void ImgToAlbum(string str);
	#endif

    private void saveImgToGallery(byte[] img)
    {
    	string fileName = saveImg(img);

		#if UNITY_IPHONE && !UNITY_EDITOR 
		ImgToAlbum(fileName);
		#endif
    }

    private string saveImg(byte[] imgPng)
    {
        string fileName = screensPath + "/screen_" + System.DateTime.Now.ToString("dd_MM_HH_mm_ss") + ".png";

        Debug.Log("write to " + fileName);

        System.IO.File.WriteAllBytes(fileName, imgPng);
        return fileName;
    }
}