using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BlackHoleEffect : MonoBehaviour
{

    public static float intensity = 0;
    private Material material;

    public static Vector2 centerPoint;
    public static float size = 0;

    // Creates a private material used to the effect
    void Awake()
    {
        material = new Material(Shader.Find("LemonSpawn/BlackHole"));
    }

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (intensity == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat("_bwBlend", intensity);
        material.SetFloat("_aspectRatio", Screen.width/(float)Screen.height);
        material.SetVector("_centerPoint", centerPoint);
        material.SetFloat("_size", size);
        Graphics.Blit(source, destination, material);
    }
}