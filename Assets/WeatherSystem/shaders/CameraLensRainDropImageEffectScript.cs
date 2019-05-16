using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CameraLensRainDropImageEffectScript : MonoBehaviour
{
    private Material effectMat;
    public float alpha = 32f;
    public float rainSpeed = 4f;
    public float clipping = 0.2f;
    public float distortion = 9f;
    public float distortionStrength = 0f;
    public float rainWhitenessDay = 1f;
    public float rainWhitenessNight = 1f;
    private float rainWhiteness = 1f;
    [Range(0,4)]
    public float blurAmount = 2.4f;
    public Texture2D rainTex;
    private float rainSpeedMultiplier;
    private float rainAmountMultiplier = 1f;

    public float doRain = 0;

    private WeatherSystemController weatherSystemController;

    void Awake()
    {
        effectMat = new Material(Shader.Find("LensEffects/CameraLensRainDrop"));

        if (rainTex == null)
            rainTex = Texture2D.whiteTexture;

        weatherSystemController = FindObjectOfType<WeatherSystemController>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effectMat.SetFloat("_DoRain", doRain);
        effectMat.SetFloat("_Alpha", alpha);
        effectMat.SetTexture("_RainTex", rainTex);
        effectMat.SetFloat("_Cliping", clipping);
        effectMat.SetFloat("_BlurAmount", blurAmount);
        effectMat.SetFloat("_Distortion", distortion);
        effectMat.SetFloat("_DistStrength", distortionStrength);
        effectMat.SetFloat("_VerticalScrollSpeed", rainSpeed);
        effectMat.SetFloat("_RainWhiteness", rainWhiteness);
        //effectMat.SetFloat("_SpeedMultiplier", rainSpeedMultiplier);
        effectMat.SetFloat("_AmountMultiplier", rainAmountMultiplier);

        Graphics.Blit(source, destination, effectMat);
    }

    private void Update()
    {
        if (doRain != 1f)
            return;

        // take camera rotation and use it to set the flow of water
        rainSpeedMultiplier = transform.rotation.eulerAngles.x;
        rainAmountMultiplier = rainSpeedMultiplier;

        if (rainSpeedMultiplier <= 90f && rainSpeedMultiplier > 0f)
        {
            rainAmountMultiplier -= 90f;
            rainAmountMultiplier = Mathf.Abs(rainAmountMultiplier);
        }
        else if (rainSpeedMultiplier > 90f)
        {
            rainAmountMultiplier -= 270f;
            rainAmountMultiplier = Mathf.InverseLerp(90f, 0f, rainAmountMultiplier) * 90f + 90f;
        }

        //if (rainSpeedMultiplier > 90f)
        //    rainSpeedMultiplier = 90f - rainSpeedMultiplier + 270f;

        //rainSpeedMultiplier = ExtensionMethods.Remap(rainSpeedMultiplier, 90f, 0f, 0f, 1f);
        rainAmountMultiplier = ExtensionMethods.Remap(rainAmountMultiplier, 0f, 180f, 0.15f, 1f);

        if (weatherSystemController.CurrentTimeOfDay == TimeOfDay.day)
            rainWhiteness = rainWhitenessDay;
        else
            rainWhiteness = rainWhitenessNight;
    }
}
