using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualIntersectionCollider : MonoBehaviour
{

    public bool IsGreenLight = true;

    private List<Vehicle> vehicles = new List<Vehicle>();
    int i = 0;

    private TrafficLights trafficLights;

    private List<Vehicle> passedOnGreen = new List<Vehicle>();


    private void Start()
    {
        trafficLights = GetComponentInParent<TrafficLights>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Vehicle v = other.GetComponent<Vehicle>();

        if (!v)
            return;

        if (IsGreenLight)
        {
            passedOnGreen.Add(v);
            return;
        }
        
        if (!passedOnGreen.Contains(v))
        {
            trafficLights.AddStoppedVehicle(v);
            vehicles.Add(v);
            v.IsInIntersection(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Vehicle v = other.GetComponent<Vehicle>();

        if (!v)
            return;

        vehicles.Remove(v);
        passedOnGreen.Remove(v);
    }

    public void TransmitGreenLight()
    {
        foreach (Vehicle v in vehicles)
        {
            trafficLights.RemoveStoppedVehicle(v);
            v.IsInIntersection(false);
        }
    }

}
