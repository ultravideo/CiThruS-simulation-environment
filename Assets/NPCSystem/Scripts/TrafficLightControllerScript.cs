using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrafficLightStatus
{
    red,
    yellow,
    green
};

public class TrafficLightControllerScript : MonoBehaviour
{

    [SerializeField] private Light red, yellow, green;

    public void SetStatus(TrafficLightStatus tls)
    {
        if (tls == TrafficLightStatus.red)
        {
            red.enabled = true;
            yellow.enabled = false;
            green.enabled = false;
        }
        else if (tls == TrafficLightStatus.yellow)
        {
            red.enabled = false;
            yellow.enabled = true;
            green.enabled = false;
        }
        else if (tls == TrafficLightStatus.green)
        {
            red.enabled = false;
            yellow.enabled = false;
            green.enabled = true;
        }
    }


}
