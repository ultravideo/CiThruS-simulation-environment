/*
 * Acts as a commanding center for all of the pedestrians
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PedestrianHiveMind : MonoBehaviour
{

    [SerializeField] GameObject[] pedestrianPrefabs;
    [SerializeField] Transform[] spawnPoints;

    private List<PedestrianSlave> pedestrians = new List<PedestrianSlave>();
    private List<Vector3[]> pedestrianPaths = new List<Vector3[]>();
    private NavMeshPath navPath;

    private Vector3 maxBounds = new Vector3(1000, 1000, 1000);


    private void Start()
    {
        navPath = new NavMeshPath();

        if (ConfigManager.SpawnPedestrians)
            SpawnPedestrians(ConfigManager.AmountOfPedestrians);
        
    }

    private void SpawnPedestrians(uint amount)
    {
        uint spawnPointIndex = 0;

        for (int i = 0; i < amount; i++)
        {
            if (spawnPointIndex == spawnPoints.Length)
                spawnPointIndex = 0;

            Vector3 newSpawnPoint = spawnPoints[spawnPointIndex].position; //spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            newSpawnPoint = new Vector3(newSpawnPoint.x + Random.Range(-10f, 10f), newSpawnPoint.y, newSpawnPoint.z + Random.Range(-10f, 10f));

            NavMesh.SamplePosition(newSpawnPoint, out NavMeshHit nhit, 10f, NavMesh.AllAreas);
            newSpawnPoint = nhit.position;
            GameObject newPedestrian = pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Length)];

            if (newSpawnPoint == Vector3.positiveInfinity || newSpawnPoint == Vector3.negativeInfinity)
                newSpawnPoint = spawnPoints[spawnPointIndex].position;

            GameObject ins;

            //newPedestrian.GetComponent<PedestrianSlave>().InitData(this);
            if (Mathf.Abs(newSpawnPoint.x) > maxBounds.x || Mathf.Abs(newSpawnPoint.y) > maxBounds.y || Mathf.Abs(newSpawnPoint.z) > maxBounds.z)
                continue;
            else
                ins = Instantiate(newPedestrian, newSpawnPoint, Quaternion.identity);

            if (Mathf.Abs(ins.transform.position.y - newSpawnPoint.y) > 0.3f)
            {
                i--;
                Destroy(ins);
            }

            pedestrians.Add(ins.GetComponent<PedestrianSlave>());
            spawnPointIndex++;
        }
    }

    /// <summary>
    /// toggles all pedestrians movement on/off
    /// </summary>
    public void TogglePedestrianEnabled(bool value)
    {
        PedestrianSlave[] peds = FindObjectsOfType<PedestrianSlave>();
        foreach (PedestrianSlave p in peds)
        {
            p.enabled = value;
            Animator pAnimator = p.GetComponent<Animator>();
            pAnimator.enabled = value;
        }

        TrafficLights[] tfl = FindObjectsOfType<TrafficLights>();
        foreach (TrafficLights t in tfl)
            t.paused = !value;
    }

    /// <summary>
    /// A PedestrianSlave can request a new path
    /// </summary>
    /// <param name="currentPosition">Vector3</param>
    /// <returns>Vector3[]</returns>
    public Vector3[] RequestNewPath(Vector3 currentPosition)
    {
        Vector3 targetPosition;

        while (true)
        {
            targetPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            if (Vector3.Distance(targetPosition, currentPosition) > 1f)
                break;
        }
        //NavMeshHit hit;

        //targetPosition = new Vector3(currentPosition.x + Random.Range(-50f, 50f), currentPosition.y, currentPosition.z + Random.Range(-50f, 50f));
        //NavMesh.SamplePosition(targetPosition, out hit, 70, NavMesh.AllAreas);
        //targetPosition = hit.position;

        NavMesh.CalculatePath(currentPosition, targetPosition, NavMesh.AllAreas, navPath);        
        return navPath.corners;
    }

}