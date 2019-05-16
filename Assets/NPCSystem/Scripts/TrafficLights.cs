using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLights : MonoBehaviour
{

    [SerializeField] IndividualIntersectionCollider[] forward_1;
    [SerializeField] IndividualIntersectionCollider[] forward_2;
    [SerializeField] IndividualIntersectionCollider[] left_1;
    [SerializeField] IndividualIntersectionCollider[] left_2;

    [SerializeField] private uint GreenLightInterval = 5;
    [SerializeField] private uint EmptyIntersectionInterval = 2;

    [SerializeField] TrafficLightControllerScript[] forward_1_light;
    [SerializeField] TrafficLightControllerScript[] forward_2_light;
    [SerializeField] TrafficLightControllerScript[] left_1_light;
    [SerializeField] TrafficLightControllerScript[] left_2_light;

    [SerializeField] PedestrianTrafficLightVolumes[] forward;
    [SerializeField] PedestrianTrafficLightVolumes[] sideways;

    private float nextTime;
    private float nextTimeEmptyIntersection;

    private bool greenLightForward = true;

    public bool paused = false;

    private List<Vehicle> stoppedVehicles = new List<Vehicle>();
    public List<Vehicle> StoppedVehicles { get { return stoppedVehicles; } }

    private void Start()
    {
        nextTime = Time.time;

        foreach (PedestrianTrafficLightVolumes p in forward)
            p.isLightGreen = true;
        foreach (PedestrianTrafficLightVolumes p in sideways)
            p.isLightGreen = false;

        foreach (IndividualIntersectionCollider i in forward_1)
            i.IsGreenLight = true;
        foreach (TrafficLightControllerScript t in forward_1_light)
            t.SetStatus(TrafficLightStatus.green);

        foreach (IndividualIntersectionCollider i in forward_2)
            i.IsGreenLight = true;
        foreach (TrafficLightControllerScript t in forward_2_light)
            t.SetStatus(TrafficLightStatus.green);

        foreach (IndividualIntersectionCollider i in left_1)
            i.IsGreenLight = false;
        foreach (TrafficLightControllerScript t in left_1_light)
            t.SetStatus(TrafficLightStatus.red);

        foreach (IndividualIntersectionCollider i in left_2)
            i.IsGreenLight = false;
        foreach (TrafficLightControllerScript t in left_2_light)
            t.SetStatus(TrafficLightStatus.red);
    }

    private void Update()
    {
        if (!paused)
        {
            if (Time.time > nextTime)
            {
                nextTime = Time.time + GreenLightInterval + EmptyIntersectionInterval;
                nextTimeEmptyIntersection = Time.time + GreenLightInterval;
                SwitchLights();
            }
            else if (Time.time > nextTimeEmptyIntersection)
            {
                EmptyIntersection();
                // some arbitrary big number
                nextTimeEmptyIntersection = Time.time + 100f;
            }
        }
        else
        {
            nextTime += Time.deltaTime;
            nextTimeEmptyIntersection += Time.deltaTime;
        }
    }

    private void EmptyIntersection()
    {
        // show red to all
        foreach(IndividualIntersectionCollider i in forward_1)
            i.IsGreenLight = false;
        foreach (TrafficLightControllerScript t in forward_1_light)
            t.SetStatus(TrafficLightStatus.yellow);

        foreach (IndividualIntersectionCollider i in forward_2)
            i.IsGreenLight = false;
        foreach (TrafficLightControllerScript t in forward_2_light)
            t.SetStatus(TrafficLightStatus.yellow);

        foreach (IndividualIntersectionCollider i in left_1)
            i.IsGreenLight = false;
        foreach (TrafficLightControllerScript t in left_1_light)
            t.SetStatus(TrafficLightStatus.yellow);

        foreach (IndividualIntersectionCollider i in left_2)
            i.IsGreenLight = false;
        foreach (TrafficLightControllerScript t in left_2_light)
            t.SetStatus(TrafficLightStatus.yellow);

        foreach (PedestrianTrafficLightVolumes p in forward)
        {
            p.isLightGreen = false;
            p.TransmitRed();
        }
        foreach (PedestrianTrafficLightVolumes p in sideways)
        {
            p.isLightGreen = false;
            p.TransmitRed();
        }

    }

    private void SwitchLights()
    {
        greenLightForward = !greenLightForward;

        if (greenLightForward)
        {

            foreach (IndividualIntersectionCollider i in forward_1)
            {
                i.IsGreenLight = true;
                i.TransmitGreenLight();
            }
            foreach (TrafficLightControllerScript t in forward_1_light)
                t.SetStatus(TrafficLightStatus.green);

            foreach (IndividualIntersectionCollider i in forward_2)
            {
                i.IsGreenLight = true;
                i.TransmitGreenLight();
            }
            foreach (TrafficLightControllerScript t in forward_2_light)
                t.SetStatus(TrafficLightStatus.green);

            foreach (IndividualIntersectionCollider i in left_1)
                i.IsGreenLight = false;
            foreach (TrafficLightControllerScript t in left_1_light)
                t.SetStatus(TrafficLightStatus.red);

            foreach (IndividualIntersectionCollider i in left_2)
                i.IsGreenLight = false;
            foreach (TrafficLightControllerScript t in left_2_light)
                t.SetStatus(TrafficLightStatus.red);

            foreach (PedestrianTrafficLightVolumes p in forward)
                p.isLightGreen = true;
            foreach (PedestrianTrafficLightVolumes p in sideways)
            {
                p.isLightGreen = false;
                p.TransmitRed();
            }

        }
        else
        {

            foreach (IndividualIntersectionCollider i in forward_1)
                i.IsGreenLight = false;
            foreach (TrafficLightControllerScript t in forward_1_light)
                t.SetStatus(TrafficLightStatus.red);

            foreach (IndividualIntersectionCollider i in forward_2)
                i.IsGreenLight = false;
            foreach (TrafficLightControllerScript t in forward_2_light)
                t.SetStatus(TrafficLightStatus.red);

            foreach (IndividualIntersectionCollider i in left_1)
            {
                i.IsGreenLight = true;
                i.TransmitGreenLight();
            }
            foreach (TrafficLightControllerScript t in left_1_light)
                t.SetStatus(TrafficLightStatus.green);

            foreach (IndividualIntersectionCollider i in left_2)
            {
                i.IsGreenLight = true;
                i.TransmitGreenLight();
            }
            foreach (TrafficLightControllerScript t in left_2_light)
                t.SetStatus(TrafficLightStatus.green);

            foreach (PedestrianTrafficLightVolumes p in forward)
            {
                p.isLightGreen = false;
                p.TransmitRed();
            }
            foreach (PedestrianTrafficLightVolumes p in sideways)
                p.isLightGreen = true;

        }
    }

    public void AddStoppedVehicle(Vehicle v)
    {
        stoppedVehicles.Add(v);
    }

    public void RemoveStoppedVehicle(Vehicle v)
    {
        stoppedVehicles.Remove(v);
    }
}
