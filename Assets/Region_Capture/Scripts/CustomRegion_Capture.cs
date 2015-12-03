using UnityEngine;
using System.Collections;
using Vuforia;

public class CustomRegion_Capture : MonoBehaviour
{
    public Camera Child_AR_Camera;
    public GameObject AR_Camera_Vector;
    [Space(10)]
    public GameObject ARCamera;
    public GameObject ImageTarget;
    public GameObject BackgroundPlane;
    [Space(20)]
    public bool AutoRegionSize = true;
    public bool HideFromARCamera = true;
    public bool CheckMarkerPosition = false;
    public bool SwitchCheckMarkerPosition = false;
    [Space(20)]
    public bool AutofocusCamera = true;
    public bool HideAndroidToolbar = true;
    [Space(20)]
    public bool ColorDebugMode = false;

    public bool MarkerIsOUT = false;
    public bool MarkerIsIN = true;
    public Texture2D VideoBackgroundTexure;

    public float CPH, CPW;


    void Start()
    {

#if UNITY_ANDROID
        if (HideAndroidToolbar)
        {
            DisableSystemUI.Run();
            DisableSystemUI.DisableNavUI();
        }
#endif

        if (ARCamera == null || ImageTarget == null || BackgroundPlane == null)
        {
            Debug.LogWarning("ARCamera, ImageTarget or BackgroundPlane not assigned!");
            this.enabled = false;
        }
        else
        {

            if (AutoRegionSize)
            {
                transform.position = ImageTarget.transform.position;
                transform.localScale = new Vector3(ImageTarget.GetComponent<ImageTargetBehaviour>().GetSize().x, 10.0f, ImageTarget.GetComponent<ImageTargetBehaviour>().GetSize().y) / 10.0f;
            }

            AR_Camera_Vector = new GameObject("AR Camera Vector");
            AR_Camera_Vector.transform.parent = ARCamera.transform;
            AR_Camera_Vector.transform.localPosition = new Vector3(0.0f, 0.1f, 0.0f);
#if !UNITY_EDITOR
			AR_Camera_Vector.transform.localPosition = Vector3.zero;
#endif
            AR_Camera_Vector.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 180.0f);
            AR_Camera_Vector.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            Child_AR_Camera = ARCamera.GetComponentInChildren<Camera>();
            gameObject.layer = 20;

            if (HideFromARCamera && !ColorDebugMode) Child_AR_Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Region_Capture"));

            CPH = Child_AR_Camera.pixelHeight;
            CPW = Child_AR_Camera.pixelWidth;

            StartCoroutine(Wait());
            StartCoroutine(Autofocus());
        }
    }


    IEnumerator Wait()
    {
        MarkerIsIN = false;
        yield return new WaitForEndOfFrame();

        if (VuforiaRenderer.Instance.IsVideoBackgroundInfoAvailable())
        {

            VuforiaRenderer.VideoTextureInfo videoTextureInfo = VuforiaRenderer.Instance.GetVideoTextureInfo();

            if (videoTextureInfo.imageSize.x == 0 || videoTextureInfo.imageSize.y == 0) goto End;

            float k_x = (float)videoTextureInfo.imageSize.x / (float)videoTextureInfo.textureSize.x * 0.5f;
            float k_y = (float)videoTextureInfo.imageSize.y / (float)videoTextureInfo.textureSize.y * 0.5f;


            GetComponent<Renderer>().material.SetFloat("_KX", k_x);
            GetComponent<Renderer>().material.SetFloat("_KY", k_y);

            VideoBackgroundTexure = VuforiaRenderer.Instance.VideoBackgroundTexture;

            if (!VideoBackgroundTexure || !BackgroundPlane.GetComponent<MeshFilter>()) goto End;

            GetComponent<Renderer>().material.SetTexture("_MainTex", VideoBackgroundTexure);


            float ImageAspect = (float)videoTextureInfo.imageSize.x / (float)videoTextureInfo.imageSize.y;


            if (Child_AR_Camera.aspect >= 1.0f || Application.isEditor)
            {

                if (Child_AR_Camera.aspect >= ImageAspect)
                {

                    float Aspect = Child_AR_Camera.aspect / ImageAspect;
                    AR_Camera_Vector.transform.localScale = new Vector3(1.0f, Aspect, 1.0f);

#if !UNITY_EDITOR
					if (Screen.orientation == ScreenOrientation.LandscapeRight) AR_Camera_Vector.transform.localScale = new Vector3 (1.0f, Aspect, -1.0f);
#endif
                }

                else
                {

                    float Aspect = ImageAspect / Child_AR_Camera.aspect;
                    AR_Camera_Vector.transform.localScale = new Vector3(Aspect, 1.0f, 1.0f);

#if !UNITY_EDITOR
					if (Screen.orientation == ScreenOrientation.LandscapeRight) AR_Camera_Vector.transform.localScale = new Vector3 (Aspect, 1.0f, -1.0f);
#endif
                }
            }


            if (Child_AR_Camera.aspect < 1.0f & !Application.isEditor)
            {

                if (ImageAspect >= (1.0f / Child_AR_Camera.aspect))
                {

                    AR_Camera_Vector.transform.localScale = new Vector3(ImageAspect, Child_AR_Camera.aspect, 1.0f);

#if !UNITY_EDITOR
					if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) AR_Camera_Vector.transform.localScale = new Vector3 (ImageAspect, Child_AR_Camera.aspect, -1.0f);
#endif
                }

                else
                {

                    AR_Camera_Vector.transform.localScale = new Vector3(1.0f / Child_AR_Camera.aspect, 1.0f / ImageAspect, 1.0f);

#if !UNITY_EDITOR
					if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) AR_Camera_Vector.transform.localScale = new Vector3 (1.0f / Child_AR_Camera.aspect, 1.0f / ImageAspect, -1.0f);
#endif
                }

                AR_Camera_Vector.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 270.0f);
            }


            End:
            if (videoTextureInfo.imageSize.x == 0 || videoTextureInfo.imageSize.y == 0 || !VideoBackgroundTexure || !BackgroundPlane.GetComponent<MeshFilter>()) StartCoroutine(Wait());

        }
        else
        {
            yield return new WaitForEndOfFrame();
            StartCoroutine(Wait());

        }

    }

    void LateUpdate()
    {

        Matrix4x4 M = transform.localToWorldMatrix;
        Matrix4x4 V = AR_Camera_Vector.transform.worldToLocalMatrix;
        Matrix4x4 P = GL.GetGPUProjectionMatrix(Child_AR_Camera.projectionMatrix, false);

        GetComponent<Renderer>().material.SetMatrix("_MATRIX_MVP", P * V * M);

        if (CheckMarkerPosition || SwitchCheckMarkerPosition || ColorDebugMode)
        {
            Vector3 boundPoint1 = GetComponent<Renderer>().bounds.min;
            Vector3 boundPoint2 = GetComponent<Renderer>().bounds.max;
            Vector3 boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
            Vector3 boundPoint4 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);

            Vector3 screenPos1 = Child_AR_Camera.WorldToScreenPoint(boundPoint1);
            Vector3 screenPos2 = Child_AR_Camera.WorldToScreenPoint(boundPoint2);
            Vector3 screenPos3 = Child_AR_Camera.WorldToScreenPoint(boundPoint3);
            Vector3 screenPos4 = Child_AR_Camera.WorldToScreenPoint(boundPoint4);

            if (screenPos1.x < 0 || screenPos1.y < 0 || screenPos2.x < 0 || screenPos2.y < 0 || screenPos3.x < 0 || screenPos3.y < 0 || screenPos4.x < 0 || screenPos4.y < 0 || screenPos1.x > CPW || screenPos1.y > CPH || screenPos2.x > CPW || screenPos2.y > CPH || screenPos3.x > CPW || screenPos3.y > CPH || screenPos4.x > CPW || screenPos4.y > CPH)
            {

                if (!MarkerIsOUT)
                {
                    StartCoroutine(MarkerOutOfBounds());
                    MarkerIsOUT = true;
                    MarkerIsIN = false;
                }
            }
            else
            {
                if (!MarkerIsIN)
                {
                    StartCoroutine(MarkerIsReturned());
                    MarkerIsIN = true;
                }
                MarkerIsOUT = false;
            }
        }
    }

    IEnumerator Autofocus()
    {
        yield return new WaitForSeconds(1.0f);
        if (AutofocusCamera) CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        else CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
    }


    IEnumerator MarkerOutOfBounds()
    {
        yield return new WaitForEndOfFrame();

        // Add action here if marker out of bounds

        Debug.Log("Marker out of bounds!");

        if (ColorDebugMode)
        {
            GetComponent<Renderer>().material.SetInt("_KR", 1);
            GetComponent<Renderer>().material.SetInt("_KG", 0);
        }

    }


    IEnumerator MarkerIsReturned()
    {
        yield return new WaitForEndOfFrame();

        // Add action here if marker is visible again

        Debug.Log("Marker is returned!");

        if (ColorDebugMode)
        {
            GetComponent<Renderer>().material.SetInt("_KR", 0);
            GetComponent<Renderer>().material.SetInt("_KG", 1);
        }

    }

    public void RecalculateRegionSize()
    {
        transform.position = ImageTarget.transform.position;
        transform.localScale = new Vector3(ImageTarget.GetComponent<ImageTargetBehaviour>().GetSize().x, 10.0f, ImageTarget.GetComponent<ImageTargetBehaviour>().GetSize().y) / 10.0f;
    }

    void OnApplicationPause()
    {
        StartCoroutine(Autofocus());
    }

}