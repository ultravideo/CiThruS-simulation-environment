using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pedestrian : MonoBehaviour
{
    private NavMeshPath path;
    private NavMeshAgent agent;
    private float nextTime = 0f;
    private float interval = 6f;
    private Vector3 nextPosition;
    private Vector3 previousPosition;
    private Vector3 targetPosition;
    uint currentIndex = 0;
    public bool allowContinuous = true;

    private bool isAvoidCars = false;
    public bool IsAvoidCars { get { return isAvoidCars; } set { isAvoidCars = value; } }

    private RaycastHit[] sphereCastHits;
    private float avoidRadius = 5f;

    private int sphereCastLayerMask = (1 << 9);

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();

        GetNewPointNearNode();
    }

    private void Update()
    {
        if (agent.path != null && !ShouldAvoidVehicles())
        {
            if (Vector3.Distance(transform.position, agent.path.corners[agent.path.corners.Length - 1]) < 4f)
            {
                GetNewPointNearNode();
            }
        }
    }

    private void GetNewPointOnNavmesh()
    {
        //targetPosition = transform.position + new Vector3(Random.Range(-15f, 15f), 0, Random.Range(-15f, 15f));

        NavMeshHit hit;
        Vector3 randomDirection = Random.insideUnitSphere * 15f + new Vector3(0, 0, 10f);
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out hit, 15f, NavMesh.AllAreas))
        {
            agent.CalculatePath(hit.position, path);
            agent.SetPath(path);
        }
        else
            GetNewPointOnNavmesh();

    }

    private void GetNewPointNearNode()
    {
        NavMeshHit hit;
        Vector3 randomNodePosition = NPCDataHolder.waypoints[Random.Range(0, NPCDataHolder.waypoints.Count - 1)].Position;
        Vector3 targetposition = Random.insideUnitSphere * 20f + randomNodePosition;

        if (NavMesh.SamplePosition(targetposition, out hit, 20f, NavMesh.AllAreas))
        {
            agent.CalculatePath(hit.position, path);
            agent.SetPath(path);
        }
        else
            GetNewPointNearNode();
    }

    public void CanMove(bool status)
    {
        agent.isStopped = !status;
    }

    public void SetNewPosition(Vector3 newPos)
    {
        if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.CalculatePath(hit.position, path);
            agent.SetPath(path);
        }
        else
            Debug.LogError("Point not near NavMesh!");

    }

    /// <summary>
    /// Checks around Pedestrian if it should wait for cars to pass
    /// </summary>
    /// <returns>bool</returns>
    private bool ShouldAvoidVehicles()
    {
        if (!IsAvoidCars)
            return false;

        Vehicle v;
        sphereCastHits = Physics.SphereCastAll(transform.position, avoidRadius, transform.forward, sphereCastLayerMask);

        if (sphereCastHits.Length != 0)
        {
            foreach (RaycastHit h in sphereCastHits)
            {
                v = h.transform.GetComponent<Vehicle>();
                if (v)
                {
                    if (v.DriveSpeed != 0f)
                        return true;
                }
            }
        }

        return false;
    }
}
