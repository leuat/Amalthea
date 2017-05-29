using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SRColorDistorter : MonoBehaviour
{

    private Material material;

    public static Vector3 moveDirection;
    public static Vector2 focusPoint;
    public static float lightSpeed = 0;

    public static Vector3 viewDirection, up, right;

    // Creates a private material used to the effect
    void Awake()
    {
        material = new Material(Shader.Find("LemonSpawn/SRColorDistortion"));
    }

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
//            Graphics.Blit(source, destination);
  //          return;

        material.SetVector("_moveDirection", moveDirection);

        

        material.SetVector("_focusPoint", focusPoint);
        material.SetVector("_viewDirection", viewDirection);
        material.SetVector("_up", up);
        material.SetVector("_right", right);
        material.SetFloat("_lightSpeed", lightSpeed);
        Graphics.Blit(source, destination, material);
    }
}