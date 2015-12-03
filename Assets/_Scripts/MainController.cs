using UnityEngine;
using System.Collections;
using Vuforia;

public class MainController : MonoBehaviour
{
    //[SerializeField]
    //protected RenderTextureCamera _SnowManRTC;
    //[SerializeField]
    //protected RenderTextureCamera _SnowTreeRTC;

    private IEnumerator _SendTexture(GameObject go, RenderTexture rt)
    {
        go.SetActive(false);
        //yield return new WaitForSeconds(0.F);

        yield return new WaitForEndOfFrame();

        //var rt = RenderTextureCamera.CameraOutputTexture;
        var FrameTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        FrameTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        FrameTexture.Apply();

        go.GetComponent<ColorObject>().ApplyTexture(FrameTexture);
        go.SetActive(true);        
    }

    void OnGUI()
    {
        //return;
        //if (_snowManGo != null && GUILayout.Button("Apply Color to SnowMan"))
        //{
        //    StartCoroutine(_SendTexture(_snowManGo));
        //}

        //if (_snowManGo != null && GUILayout.Button("Apply Color to SnowTree"))
        //{
        //    StartCoroutine(_SendTexture(_snowTreeGo));
        //}
    }

    private GameObject _snowManGo;
    private GameObject _snowTreeGo;

    public void OnTrackingFound(GameObject go)
    {
        print("OnTrackingFound " + go);

        switch (go.name)
        {
            case "snowman":
                _snowManGo = go;
                if (RenderTextureCamera.CameraOutputTexture != null)
                    StartCoroutine(_SendTexture(_snowManGo, RenderTextureCamera.CameraOutputTexture));
                break;
            case "snowtree":
                _snowTreeGo = go;
                if (RenderTextureCamera.CameraOutputTexture != null)
                    StartCoroutine(_SendTexture(_snowTreeGo, RenderTextureCamera.CameraOutputTexture));
                break;
        }
    }

    public void OnTrackingLost(GameObject go)
    {
        print("OnTrackingLost " + go);
        switch (go.name)
        {
            case "snowman":
                _snowManGo = null;
                break;
            case "snowtree":
                _snowTreeGo = null;
                break;
        }
    }
}
