using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public enum VehicleType
{
    Sedan,
    Truck,
    Bus
};

/// <summary>
/// Responds to requests made by class Vehicle and distributes new paths
/// </summary>
public class NPCMovementManager : MonoBehaviour
{
#if (UNITY_EDITOR)
    private string CONFIG_PATH = "Assets/NPCSystem/PersistentData/NPC.config";
#endif

#if !(UNITY_EDITOR)
    private string CONFIG_PATH = "PersistentData/NPC.config";
#endif

    private uint trucksInScene = 0;
    [SerializeField] private List<GameObject> differentVehicles;

    [SerializeField] private List<GameObject> differentPedestrians;

    [SerializeField] private GameObject[] panels;


    private List<Vehicle> inCirculation = new List<Vehicle>();
    private PedestrianSlave[] inCirculationPedestrians;
    bool areNPCsEnabled = true;

    public bool disableCollision = false;

    private WeatherSystemController weatherSystemController;

    private GameObject democar;
    private Vehicle democarVehicle;
    private bool isPaused = false;

    private void Start()
    {
        weatherSystemController = GameObject.FindObjectOfType<WeatherSystemController>();
        ExtensionMethods.PopulateNPCDataHolder();

        SpawnVehicles(ConfigManager.AmountOfVehicles, ConfigManager.AmountOfTrucks);

        try
        {
            democar = FindObjectOfType<DemoCarPOV>().gameObject;
            democarVehicle = democar.GetComponent<Vehicle>();
            inCirculation.Add(democarVehicle);
        } catch { }
    }

    private void Update()
    {
        // reset scene
        if (Input.GetKeyDown(KeyCode.Y))
        {
            FindObjectOfType<NPCMovementManager>().RespawnVehicles();

            // read config
            string configData = System.IO.File.ReadAllText(CONFIG_PATH);
            string[] configDataArr = configData.Split(';');

            GameObject.FindObjectOfType<WeatherSystemController>().ResetCarLights();
            //SetInitialData();
        }
        // toggles cars on and off
        if (Input.GetKeyDown(KeyCode.T))
        {
            areNPCsEnabled = !areNPCsEnabled;

            foreach (Vehicle v in inCirculation)
                v.gameObject.SetActive(areNPCsEnabled);

            if (inCirculationPedestrians == null)
                inCirculationPedestrians = FindObjectsOfType<PedestrianSlave>();

            foreach (PedestrianSlave p in inCirculationPedestrians)
            {
                p.gameObject.SetActive(areNPCsEnabled);
                if (areNPCsEnabled)
                    p.GetComponent<Animator>().SetInteger("walkState", (int)WalkStates.walk);
            }

            if (areNPCsEnabled)
                inCirculationPedestrians = null;

        }
        // un/pause all NPCs
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (Vehicle v in inCirculation)
                v.enabled = isPaused;

            FindObjectOfType<PedestrianHiveMind>().TogglePedestrianEnabled(isPaused);
            isPaused = !isPaused;
        }
    }

    /// <summary>
    /// Kills vehicle
    /// </summary>
    /// <param name="v">Vehicle</param>
    public void KillVehicle(Vehicle v)
    {
        inCirculation.Remove(v);
        Destroy(v.gameObject);
        //SpawnVehicle();
    }

    /// <summary>
    /// Kills all vehicles in the scene and respawns them
    /// </summary>
    public void RespawnVehicles()
    {
        foreach (Vehicle v in inCirculation)
        {
            if (v != democarVehicle)
                Destroy(v.gameObject);
        }
        inCirculation = new List<Vehicle>();
        inCirculation.Add(democarVehicle);

        trucksInScene = 0;
        weatherSystemController.ResetCarLights();
        SpawnVehicles(ConfigManager.AmountOfVehicles, ConfigManager.AmountOfTrucks);
    }
    
    public void SpawnVehicle()
    {
        UniqueWaypoint spawnPos = NPCDataHolder.waypoints[UnityEngine.Random.Range(0, NPCDataHolder.waypoints.Count)];
        UniqueWaypoint nextPos = spawnPos.GetRandomChild();
        if (nextPos == null)
        {
            SpawnVehicle();
            return;
        }
        UniqueWaypoint third = spawnPos.GetRandomChild();
        if (third == null)
        {
            SpawnVehicle();
            return;
        }

        GameObject newVehicleObject = differentVehicles[UnityEngine.Random.Range(0, differentVehicles.Count)];

        if (newVehicleObject.GetComponent<Vehicle>().VehicleType == VehicleType.Truck)
        {
            trucksInScene++;
            if (trucksInScene > ConfigManager.AmountOfTrucks)
                SpawnVehicle();
        }

        Quaternion rot = Quaternion.LookRotation((nextPos.Position - spawnPos.Position).normalized, Vector3.up);
        Vehicle newVehicle = Instantiate(newVehicleObject, spawnPos.Position, rot).GetComponent<Vehicle>();

        newVehicle.InitializeSpawnData(spawnPos, nextPos, third);
        inCirculation.Add(newVehicle);

        if (disableCollision)
            newVehicle.OverrideCollisionDetection(true);
    }

    /// <summary>
    /// Spawns n -amount of vehicles
    /// </summary>
    private void SpawnVehicles(uint n, uint trucks)
    {
        if (NPCDataHolder.waypoints == null)
            return;

        for (int i = 0; i < n; i++)
        {
            UniqueWaypoint currentWaypoint = NPCDataHolder.waypoints[UnityEngine.Random.Range(0, NPCDataHolder.waypoints.Count)];
            UniqueWaypoint nextPos = currentWaypoint.GetRandomChild();
            if (nextPos == null)
            {
                i--;
                continue;
            }

            UniqueWaypoint third = nextPos.GetRandomChild();
            if (nextPos == null || currentWaypoint == null)
            {
                i--;
                continue;
            }

            GameObject newVehicleObject = differentVehicles[UnityEngine.Random.Range(0, differentVehicles.Count)];

            if (newVehicleObject.GetComponent<Vehicle>().VehicleType == VehicleType.Truck)
            {
                trucksInScene++;
                if (trucksInScene > trucks)
                {
                    i--;
                    continue;
                }
            }

            Quaternion rot = Quaternion.LookRotation((nextPos.Position - currentWaypoint.Position).normalized, Vector3.up);

            Vehicle newVehicle = Instantiate(newVehicleObject, currentWaypoint.Position, rot).GetComponent<Vehicle>();

            newVehicle.InitializeSpawnData(currentWaypoint, nextPos, third);
            inCirculation.Add(newVehicle);

            if (disableCollision)
                newVehicle.OverrideCollisionDetection(true);
        }

        weatherSystemController.SetTimeOfDay(weatherSystemController.CurrentTimeOfDay);
    }

}
