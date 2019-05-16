using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianTrafficLightVolumes : MonoBehaviour
{
    private List<PedestrianSlave> pedestrians = new List<PedestrianSlave>();
    public bool isLightGreen = false;


    private void OnTriggerEnter(Collider other)
    {
        PedestrianSlave p = other.transform.GetComponent<PedestrianSlave>();
        if (!p)
            return;

        pedestrians.Add(p);
        p.StopMoving(!isLightGreen);
        p.IsInIntersection = true;
    }

    private void OnTriggerExit(Collider other)
    {
        PedestrianSlave p = other.GetComponent<PedestrianSlave>();

        if (pedestrians.Contains(p))
            pedestrians.Remove(p);

        p.RunFromRed(false);
        p.IsInIntersection = false;
    }

    private void OnTriggerStay(Collider other)
    {
        foreach (PedestrianSlave p in pedestrians)
        {
            if (p.Stopped)
                p.StopMoving(!isLightGreen);
        }
    }

    public void TransmitRed()
    {
        foreach (PedestrianSlave p in pedestrians)
        {
            if (!p.Stopped)
                p.RunFromRed(true);
        }
    }


}
