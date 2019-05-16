using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LensGrimeDirtImageEffectScript : MonoBehaviour
{
    private Material effectMat;
    public Texture2D dirtTex;
    public float blendingFactor;

    private void Awake()
    {
        effectMat = new Material(Shader.Find("LensEffects/CameraLensDirtGrime"));

        effectMat.SetTexture("_DirtTex", dirtTex);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effectMat.SetFloat("_BlendingFactor", blendingFactor);
        Graphics.Blit(source, destination, effectMat);
    }


}
