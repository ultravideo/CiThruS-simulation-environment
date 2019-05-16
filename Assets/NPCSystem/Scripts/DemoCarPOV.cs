/*
 * Democar is used to, well, demo the working of cars from 1st person view.
 * The democar also acts as a central place to test new features on.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles demoCar usage
/// </summary>
public class DemoCarPOV : MonoBehaviour
{

#if (UNITY_EDITOR)
    private string CONFIG_PATH = "Assets/NPCSystem/PersistentData/NPC.config";
#endif

#if !(UNITY_EDITOR)
    private string CONFIG_PATH = "PersistentData/NPC.config";
#endif

    [SerializeField] private Transform steeringWheel;
    [SerializeField] private Camera povCamera;
    [SerializeField] private bool disableOtherCameras = true;
    private Vehicle vehicle;
    private float spawnThreshold = 30f;

    private float steeringWheelLerpSlider = 0f;
    private float currentAngle;
    private float targetAngle;
    private Quaternion previousRotation;

    private bool useGlobalCamera = false;

    [SerializeField] private Camera ToggleFlyCamera;

    private Vector3 initialSpawnPoint;

    private WeatherSystemController weatherSystemController;

    private void Start()
    {
        vehicle = GetComponent<Vehicle>();
        initialSpawnPoint = transform.position;

        if (NPCDataHolder.waypoints == null)
            ExtensionMethods.PopulateNPCDataHolder();

        weatherSystemController = GameObject.FindObjectOfType<WeatherSystemController>();

        SetInitialData();

        if (disableOtherCameras)
        {
            Camera[] sceneCameras = FindObjectsOfType<Camera>();

            foreach (Camera cam in sceneCameras)
            {
                if (cam != povCamera)
                    cam.gameObject.SetActive(false);
            }
        }
        else
            povCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        currentAngle = vehicle.TurnAngle;
        SteerWheel(currentAngle);

        // switch to flycam or back
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleFlyCamera.gameObject.SetActive(!ToggleFlyCamera.gameObject.activeSelf);
            povCamera.gameObject.SetActive(!povCamera.gameObject.activeSelf);

            if (!ToggleFlyCamera.gameObject.activeSelf)
                Cursor.lockState = CursorLockMode.None;
        }

        //// reset scene
        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    FindObjectOfType<NPCMovementManager>().RespawnVehicles();

        //    // read config
        //    string configData = System.IO.File.ReadAllText(CONFIG_PATH);
        //    string[] configDataArr = configData.Split(';');

        //    GameObject.FindObjectOfType<WeatherSystemController>().ResetCarLights();
        //    //SetInitialData();
        //}
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    Vehicle[] vehiclesInScene = FindObjectsOfType<Vehicle>();
        //    foreach (Vehicle v in vehiclesInScene)
        //    {
        //        v.GetComponent<Vehicle>().enabled = !v.GetComponent<Vehicle>().enabled;

        //        foreach (MeshRenderer mr in v.GetComponentsInChildren<MeshRenderer>())
        //        {
        //            mr.enabled = !mr.enabled;
        //        }
        //    }
        //}

    }


    private void SteerWheel(float angle)
    {
        steeringWheel.localRotation = Quaternion.RotateTowards(steeringWheel.localRotation, Quaternion.Euler(0f, 0f, -angle), 65f * Time.deltaTime);
    }


    private void SetInitialData()
    {
        foreach (UniqueWaypoint u in NPCDataHolder.waypoints)
        {
            if (Vector3.Distance(initialSpawnPoint, u.Position) < spawnThreshold)
            {
                UniqueWaypoint f = u;
                UniqueWaypoint s = f.GetRandomChild();
                UniqueWaypoint t = s.GetRandomChild();

                transform.position = f.Position;
                transform.rotation = Quaternion.LookRotation((s.Position - f.Position).normalized, Vector3.up);

                vehicle.InitializeSpawnData(f, s, t);

                return;
            }

        }
    }

}
