using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TimeOfDay
{
    day,
    night
};

public enum WeatherState
{
    sunny,
    snow,
    rain
}

/// <summary>
/// Use SetWeatherCondition() and SetTimeOfDay() to set said weather states
/// </summary>
public class WeatherSystemController : MonoBehaviour
{
    #region fields

    private TimeOfDay currentTime = TimeOfDay.day;
    public TimeOfDay CurrentTimeOfDay { get { return currentTime; } }

    private WeatherState currentWeather = WeatherState.sunny;
    public WeatherState CurrentWeather { get { return currentWeather; } }

    [SerializeField] float rainAlphaDay = 0.29f;
    [SerializeField] float rainAlphaNight = 0.16f;

    [SerializeField] GameObject snowParticles;
    [SerializeField] GameObject rainParticles;
    [SerializeField] Material daySkyboxMaterial;
    [SerializeField] Material nightSkyboxMaterial;
    [SerializeField] Material rainSkyboxMaterial;
    [SerializeField] Material snowSkyboxMaterial;

    [SerializeField] float dayTimeSunIntensity = 1.3f;
    [SerializeField] float nightTimeSunIntensity = 0.65f;
    [SerializeField] float rainSunIntensity = 0.65f;
    [SerializeField] float snowSunIntensity = 0.65f;
    [SerializeField] Color dayTimeSunTint;
    [SerializeField] Color nightTimeSunTint;
    [SerializeField] Color nightSnowSunTint;
    [SerializeField] Color nightRainSunTint;
    [SerializeField] Color rainSunTint;
    [SerializeField] Color snowSunTint;
    [SerializeField] Color rainFogColor;
    [SerializeField] Color sunnyFogColor;
    [SerializeField] Color nightFogColor;
    [SerializeField] Color snowFogColor;

    [SerializeField] Material[] windowShieldMaterials;

    [SerializeField] Light sunLight;

    private List<GameObject> carLights = new List<GameObject>();
    private GameObject[] lampPostLamps;

    private bool fog = false;

    #endregion

    #region MonoBehaviourMethods
    private void Awake()
    {
        RenderSettings.fog = true;
        //CarLightTag[] temp = GameObject.FindObjectsOfType<CarLightTag>();
        //foreach (CarLightTag c in temp)
        //    carLights.Add(c.gameObject);
        //print(carLights.Count);
        lampPostLamps = GameObject.FindGameObjectsWithTag("LampPostLamp");

        //foreach (GameObject l in carLights)
        //    l.SetActive(false);

        foreach (GameObject l in lampPostLamps)
            l.SetActive(false);

        SetWeatherCondition(currentWeather);

        StartCoroutine(TurnOffCarLightsWithDelay());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (CurrentTimeOfDay == TimeOfDay.day)
                SetTimeOfDay(TimeOfDay.night);
            else
                SetTimeOfDay(TimeOfDay.day);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (currentWeather == WeatherState.sunny)
                currentWeather = WeatherState.rain;
            else if (currentWeather == WeatherState.rain)
                currentWeather = WeatherState.snow;
            else if (currentWeather == WeatherState.snow)
                currentWeather = WeatherState.sunny;

            SetWeatherCondition(currentWeather);
        }
    }
    #endregion

    #region PrivateMethods
    private void ToggleFog()
    {
        RenderSettings.fog = !RenderSettings.fog;
    }
    private IEnumerator TurnOffCarLightsWithDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (carLights.Count == 0)
        {
            CarLightTag[] temp = GameObject.FindObjectsOfType<CarLightTag>();
            foreach (CarLightTag c in temp)
                carLights.Add(c.gameObject);
        }

        foreach (GameObject g in carLights)
            g.GetComponent<Light>().enabled = false;

        foreach (Material m in windowShieldMaterials)
            m.SetFloat("_DoRain", 0);
    }

    #endregion

    #region PublicMehods

    /// <summary>
    /// sets the time of day
    /// </summary>
    /// <param name="tod">enum TimeOfDay: {day, night}</param>
    public void SetTimeOfDay(TimeOfDay tod)
    {

        if (carLights.Count == 0)
        {
            CarLightTag[] temp = GameObject.FindObjectsOfType<CarLightTag>();
            foreach (CarLightTag c in temp)
                carLights.Add(c.gameObject);
        }

        if (tod == TimeOfDay.day)
        {
            currentTime = TimeOfDay.day;
            RenderSettings.skybox = daySkyboxMaterial;
            sunLight.color = dayTimeSunTint;
            sunLight.intensity = dayTimeSunIntensity;

            ToggleFog();

            foreach (GameObject l in carLights)
                l.GetComponent<Light>().enabled = false;

            foreach (GameObject g in lampPostLamps)
                g.SetActive(false);

            foreach (Material m in windowShieldMaterials)
                m.SetFloat("_Alpha", rainAlphaDay);
        }
        else if (tod == TimeOfDay.night)
        {
            currentTime = TimeOfDay.night;
            RenderSettings.skybox = nightSkyboxMaterial;
            sunLight.color = nightTimeSunTint;
            sunLight.intensity = nightTimeSunIntensity;

            ToggleFog();

            foreach (GameObject l in carLights)
                l.GetComponent<Light>().enabled = true;

            foreach (GameObject l in lampPostLamps)
                l.SetActive(true);

            foreach (Material m in windowShieldMaterials)
                m.SetFloat("_Alpha", rainAlphaNight);
        }

        SetWeatherCondition(currentWeather);
    }

    /// <summary>
    /// Sets the weather state
    /// </summary>
    /// <param name="ws">enum WeatherState: {sunny, snow, rain}</param>
    public void SetWeatherCondition(WeatherState ws)
    {
        CameraLensRainDropImageEffectScript[] cameras = FindObjectsOfType<CameraLensRainDropImageEffectScript>();
        currentWeather = ws;

        switch(ws)
        {
            case WeatherState.sunny:
                fog = false;
                snowParticles.SetActive(false);
                rainParticles.SetActive(false);

                foreach (Material m in windowShieldMaterials)
                    m.SetFloat("_DoRain", 0);

                foreach (CameraLensRainDropImageEffectScript c in cameras)
                    c.doRain = 0f;

                if (currentTime == TimeOfDay.day)
                {
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                    RenderSettings.skybox = daySkyboxMaterial;
                    sunLight.color = dayTimeSunTint;
                    sunLight.intensity = dayTimeSunIntensity;
                    RenderSettings.fogColor = sunnyFogColor;
                }
                else
                {
                    sunLight.color = nightTimeSunTint;
                    RenderSettings.fogColor = nightFogColor;
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                }
                break;
            case WeatherState.snow:
                snowParticles.SetActive(true);
                rainParticles.SetActive(false);

                foreach (Material m in windowShieldMaterials)
                    m.SetFloat("_DoRain", 0);

                foreach (CameraLensRainDropImageEffectScript c in cameras)
                    c.doRain = 0f;

                if (currentTime == TimeOfDay.day)
                {
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                    RenderSettings.skybox = snowSkyboxMaterial;
                    sunLight.color = snowSunTint;
                    sunLight.intensity = snowSunIntensity;
                    RenderSettings.fogColor = snowFogColor;
                }
                else
                {
                    sunLight.color = nightSnowSunTint;
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientSkyColor = nightSnowSunTint;
                }
                fog = false;
                break;
            case WeatherState.rain:
                snowParticles.SetActive(false);
                rainParticles.SetActive(true);

                foreach (Material m in windowShieldMaterials)
                    m.SetFloat("_DoRain", 1);

                foreach (CameraLensRainDropImageEffectScript c in cameras)
                    c.doRain = 1f;

                if (currentTime == TimeOfDay.day)
                {
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                    RenderSettings.skybox = rainSkyboxMaterial;
                    sunLight.color = rainSunTint;
                    sunLight.intensity = rainSunIntensity;
                    RenderSettings.fogColor = rainFogColor;
                }
                else
                {
                    sunLight.color = nightRainSunTint;
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                }
                break;
        }
    }

    /// <summary>
    /// This should only be called when cars have changed in some way (e.g. number of them, respawn)
    /// </summary>
    public void ResetCarLights()
    {
        carLights = new List<GameObject>();
    }

    public void AddCarLight(GameObject light)
    {
        if (light.GetComponent<Light>() == null)
            return;

        carLights.Add(light);
    }

    #endregion

}
