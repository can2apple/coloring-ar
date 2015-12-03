using UnityEngine;

public class ColorObject : MonoBehaviour
{
    [SerializeField]
    protected Material _material;

    [SerializeField]
    protected Texture2D _defaultTexture;

    void Awake()
    {
        _material.mainTexture = _defaultTexture;
    }

    public void ApplyTexture(Texture2D tex)
    {
        _material.mainTexture = tex;
    }

}
